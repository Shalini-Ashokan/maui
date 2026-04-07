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
			var clipPath = GetClipPath(clipBounds, strokeThickness);

			// When content has a scale transform (e.g., the MAUI Scale property), the mask on
			// content.Layer lives in the layer's pre-transform coordinate space. Core Animation
			// then applies the content's transform to the masked result, making the effective clip
			// region appear shifted or incorrectly sized in screen space.
			// Fix: apply an inverse-scale transform to the clip path so it maps correctly in
			// the content layer's pre-transform coordinates.
			var contentLayerTransform = content.Layer.Transform;
			if (!contentLayerTransform.IsIdentity)
			{
				// Extract scale factors from the transform matrix row magnitudes.
				// This handles both pure scale and combined scale+rotation transforms.
				float scaleX = MathF.Sqrt(
					(float)(contentLayerTransform.M11 * contentLayerTransform.M11 +
							contentLayerTransform.M12 * contentLayerTransform.M12));
				float scaleY = MathF.Sqrt(
					(float)(contentLayerTransform.M21 * contentLayerTransform.M21 +
							contentLayerTransform.M22 * contentLayerTransform.M22));

				// Only compensate when actual scaling is present. Pure translation or rotation
				// without scale should use the standard mask placement to avoid behavior changes.
				bool hasScale = MathF.Abs(scaleX - 1.0f) > 0.001f || MathF.Abs(scaleY - 1.0f) > 0.001f;
				if (hasScale && scaleX > 0.001f && scaleY > 0.001f)
				{
					// Map clip-bounds-space coordinates into content layer pre-transform coordinates:
					//   contentLayer = (clipPt + strokeThickness - layerPos) / scale + anchorInLayer
					var layerPos = content.Layer.Position;
					var layerBounds = content.Layer.Bounds;
					var layerAnchor = content.Layer.AnchorPoint;

					float anchorInLayerX = (float)(layerAnchor.X * layerBounds.Width);
					float anchorInLayerY = (float)(layerAnchor.Y * layerBounds.Height);

					float tx = (strokeThickness - (float)layerPos.X) / scaleX + anchorInLayerX;
					float ty = (strokeThickness - (float)layerPos.Y) / scaleY + anchorInLayerY;

					var pathTransform = new CGAffineTransform(
						(nfloat)(1.0f / scaleX), 0,
						0, (nfloat)(1.0f / scaleY),
						(nfloat)tx, (nfloat)ty);

					_contentMask.Path = clipPath?.CopyByTransformingPath(pathTransform);

					// Size the mask to cover the full content layer; the transformed path
					// already encodes the correct clip region in pre-transform coords.
					_contentMask.Bounds = new CGRect(0, 0, layerBounds.Width, layerBounds.Height);
					_contentMask.Position = new CGPoint(layerBounds.Width / 2, layerBounds.Height / 2);

					SetContentMask(content);
					return;
				}
			}

			// No scale transform (or degenerate scale): use straightforward mask placement.
			_contentMask.Path = clipPath;

			// Since the mask is on the content's CALayer, it's anchored to the content. But we need it to be
			// relative to _this_ container. So we need to compute an adjusted position for it.
			var contentFrame = content.Frame;
			var clipBoundsCenter = clipBounds.Center;

			CGPoint adjustedMaskPosition = new(
				clipBoundsCenter.X + strokeThickness - contentFrame.X,
				clipBoundsCenter.Y + strokeThickness - contentFrame.Y);

			_contentMask.Bounds = clipBounds;
			_contentMask.Position = adjustedMaskPosition;

			SetContentMask(content);
		}

		void SetContentMask(UIView content)
		{
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