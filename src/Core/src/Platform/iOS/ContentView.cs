using System;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class ContentView : MauiView
	{
		WeakReference<IBorderStroke>? _clip;
		CAShapeLayer? _contentMask;

		// When the BorderHandler sets the content UIView, it tags it with this so we can 
		// verify we're using the correct subview for masking (and any other purposes)
		internal const nint ContentTag = 0x63D2A0;

		public ContentView()
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				Layer.CornerCurve = CACornerCurve.Continuous; // Available from iOS 13. More info: https://developer.apple.com/documentation/quartzcore/calayercornercurve/3152600-continuous
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			UpdateClip();
		}

		internal IBorderStroke? Clip
		{
			get => _clip is not null && _clip.TryGetTarget(out var clip) ? clip : null;
			set
			{
				_clip = value is null ? null : new(value);

				if (value is not null)
				{
					UpdateClip();
				}
			}
		}

		UIView? PlatformContent
		{
			get
			{
				// It's a fair bet that Subviews[0] will always be the content for the ContentView
				// But just in case, we're going to iterate over the views and check the tag
				foreach (var subview in Subviews)
				{
					if (subview.Tag == ContentTag)
					{
						return subview;
					}
				}

				return null;
			}
		}

		void RemoveContentMask()
		{
			_contentMask?.RemoveFromSuperLayer();
			_contentMask = null;
		}

		void UpdateClip()
		{
			var content = PlatformContent;

			if (Clip is null || Bounds == CGRect.Empty || content == null || content.Frame == CGRect.Empty)
			{
				RemoveContentMask();
				return;
			}

			_contentMask ??= new StaticCAShapeLayer();

			var bounds = Bounds;

			var strokeThickness = (float)Clip.StrokeThickness;

			// We need to inset the content clipping by the width of the stroke on both sides
			// (top and bottom, left and right), so we remove it twice from the total width/height 
			var strokeInset = 2 * strokeThickness;
			var clipWidth = (float)bounds.Width - strokeInset;
			var clipHeight = (float)bounds.Height - strokeInset;

			var clipBounds = new RectF(0, 0, clipWidth, clipHeight);

			// When content has a scale transform (e.g., the MAUI Scale property), the mask on
			// content.Layer lives in the layer's pre-transform coordinate space. Core Animation
			// then applies the content's transform to the masked result, making the effective clip
			// region appear too large (or too small) in screen space.
			//
			// Fix: Instead of changing the mask layer's bounds or transform, apply a CGAffineTransform
			// directly to the clip PATH so it is expressed in content layer pre-transform coordinates.
			// The mask then covers the full content layer while the transformed path defines the
			// precise opaque region. After the content's transform is applied by Core Animation, the
			// visible region aligns exactly with the Border's visible bounds in screen space.
			var contentLayerTransform = content.Layer.Transform;
			if (!contentLayerTransform.IsIdentity)
			{
				// Extract scale factors from the 3×3 upper-left submatrix row magnitudes.
				// This is correct for pure scale and for combined scale+rotation transforms.
				float scaleX = MathF.Sqrt(
					(float)(contentLayerTransform.M11 * contentLayerTransform.M11 +
							contentLayerTransform.M12 * contentLayerTransform.M12 +
							contentLayerTransform.M13 * contentLayerTransform.M13));
				float scaleY = MathF.Sqrt(
					(float)(contentLayerTransform.M21 * contentLayerTransform.M21 +
							contentLayerTransform.M22 * contentLayerTransform.M22 +
							contentLayerTransform.M23 * contentLayerTransform.M23));

				if (scaleX > 0.001f && scaleY > 0.001f)
				{
					// Build a CGAffineTransform that maps clip-bounds-space coordinates into
					// content layer pre-transform coordinates. For a point (x, y) in clip-bounds:
					//
					//   cv = (x + strokeThickness, y + strokeThickness)          [ContentView coords]
					//   contentLayer = (cv - layerPos) / scale + anchorInLayer   [content layer coords]
					//
					// Combining: contentLayer_x = x / scaleX + (strokeThickness − layerPos.X) / scaleX + anchorInLayerX
					//            contentLayer_y = y / scaleY + (strokeThickness − layerPos.Y) / scaleY + anchorInLayerY
					var layerPos = content.Layer.Position;
					var layerBounds = content.Layer.Bounds;
					var layerAnchor = content.Layer.AnchorPoint;

					var anchorInLayerX = (float)(layerAnchor.X * layerBounds.Width);
					var anchorInLayerY = (float)(layerAnchor.Y * layerBounds.Height);

					float tx = (strokeThickness - (float)layerPos.X) / scaleX + anchorInLayerX;
					float ty = (strokeThickness - (float)layerPos.Y) / scaleY + anchorInLayerY;

					// CGAffineTransform: | a  b  0 |   transforms: x' = a*x + c*y + tx
					//                    | c  d  0 |               y' = b*x + d*y + ty
					//                    | tx ty 1 |
					var pathTransform = new CGAffineTransform(
						(nfloat)(1.0f / scaleX), 0,
						0, (nfloat)(1.0f / scaleY),
						(nfloat)tx, (nfloat)ty);

					// Apply the affine transform to the path so it is positioned correctly in
					// content layer pre-transform coords. Use UIBezierPath for the transform API.
					var clipPath = GetClipPath(clipBounds, strokeThickness);
					if (clipPath is not null)
					{
						var bezier = UIBezierPath.FromPath(clipPath);
						bezier.ApplyTransform(pathTransform);
						_contentMask.Path = bezier.CGPath;
					}
					else
					{
						_contentMask.Path = null;
					}

					// Position the mask to cover the full content layer. The path already encodes
					// the correct clip position in content layer pre-transform coords.
					_contentMask.Bounds = new CGRect(0, 0, layerBounds.Width, layerBounds.Height);
					_contentMask.AnchorPoint = new CGPoint(0.5, 0.5);
					_contentMask.Position = new CGPoint(layerBounds.Width / 2, layerBounds.Height / 2);

					if (content.Layer.Mask != _contentMask)
					{
						content.Layer.Mask = _contentMask;
					}

					return;
				}
			}

			// No scale transform (or degenerate): use the original straightforward mask placement.
			_contentMask.Path = GetClipPath(clipBounds, strokeThickness);

			// Since the mask is on the content's CALayer, it's anchored to the content. But we need it to be
			// relative to _this_ container. So we need to compute an adjusted position for it.

			var contentFrame = content.Frame;
			var contentOffsetX = contentFrame.X;
			var contentOffsetY = contentFrame.Y;

			var clipBoundsCenter = clipBounds.Center;
			var clipCenterX = clipBoundsCenter.X + (strokeThickness);
			var clipCenterY = clipBoundsCenter.Y + (strokeThickness);

			CGPoint adjustedMaskPosition = new(clipCenterX - contentOffsetX, clipCenterY - contentOffsetY);

			_contentMask.Bounds = clipBounds;
			_contentMask.AnchorPoint = new CGPoint(0.5, 0.5);
			_contentMask.Position = adjustedMaskPosition;

			// Set the mask on the content, if it isn't already
			if (content.Layer.Mask != _contentMask)
			{
				content.Layer.Mask = _contentMask;
			}
		}

		CGPath? GetClipPath(RectF bounds, float strokeThickness)
		{
			IShape? clipShape = Clip?.Shape;
			PathF? path;

			if (clipShape is IRoundRectangle roundRectangle)
				path = roundRectangle.InnerPathForBounds(bounds, strokeThickness);
			else
				path = clipShape?.PathForBounds(bounds);

			return path?.AsCGPath();
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			// Make sure we're not holding a mask for content we no longer own
			if (uiview == PlatformContent)
			{
				RemoveContentMask();
			}

			base.WillRemoveSubview(uiview);
		}
	}
}