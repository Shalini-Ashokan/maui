using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiPageControl : UIPageControl, IUIViewLifeCycleEvents
	{
		const int DefaultIndicatorSize = 6;

		WeakReference<IIndicatorView>? _indicatorView;
		bool _updatingPosition;

		public MauiPageControl()
		{
			ValueChanged += MauiPageControlValueChanged;
			if (OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsMacCatalystVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14))
			{
				AllowsContinuousInteraction = false;
				BackgroundStyle = UIPageControlBackgroundStyle.Minimal;
			}
		}

		public void SetIndicatorView(IIndicatorView? indicatorView)
		{
			if (indicatorView == null)
			{
				ValueChanged -= MauiPageControlValueChanged;
			}
			_indicatorView = indicatorView is null ? null : new(indicatorView);

		}

		public bool IsSquare { get; set; }

		public double IndicatorSize { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				ValueChanged -= MauiPageControlValueChanged;

			base.Dispose(disposing);
		}


		// Track if we have shadow applied
		private bool _hasShadow;

		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "This reference is used only for rendering and does not create a reference cycle")]
		private IShadow? _appliedShadow;

		// Apply shadow to the control
		internal void ApplyShadow(IShadow shadow)
		{
			_hasShadow = shadow != null && shadow.Paint != null;
			_appliedShadow = shadow;

			if (_hasShadow && IndicatorSize != DefaultIndicatorSize)
			{
				// When both custom size and shadow are applied, update our custom indicators
				UpdateIndicatorSize();
				ApplyShadowToCustomDots();
			}
			else if (_hasShadow)
			{
				// Just apply shadow normally if no custom size
				Layer.SetShadow(shadow);
			}
		}

		// Apply shadow to our custom dots
		private void ApplyShadowToCustomDots()
		{
			if (_customDots == null || _appliedShadow == null || !_hasShadow)
				return;

			foreach (var dot in _customDots)
			{
				// Apply shadow to each dot
				if (_appliedShadow?.Paint is SolidPaint solidPaint && solidPaint.Color is Color color)
				{
					var uiColor = color.ToPlatform();
					var radius = _appliedShadow.Radius;
					var opacity = _appliedShadow.Opacity;
					var offset = new CGSize(_appliedShadow.Offset.X, _appliedShadow.Offset.Y);

					dot.Layer.ShadowColor = uiColor.CGColor;
					dot.Layer.ShadowOpacity = opacity;
					dot.Layer.ShadowRadius = radius / 2;
					dot.Layer.ShadowOffset = offset;
					dot.Layer.MasksToBounds = false;
				}
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (Subviews.Length == 0)
				return;

			// Handle custom indicator size and/or shadow
			if (IndicatorSize != DefaultIndicatorSize && IndicatorSize > 0)
			{
				if (_hasShadow)
				{
					// When shadow + custom size, use our custom implementation
					UpdateIndicatorSize();
					ApplyShadowToCustomDots();
				}
				else
				{
					// Just size change without shadow
					UpdateIndicatorSize();
				}
			}

			// Apply square shape if specified
			if (IsSquare)
			{
				UpdateSquareShape();
			}

			// Additional check - handle case where UIKit might reset our dots during scrolling
			if ((_customDots != null || _hasShadow) && Window != null)
			{
				BeginInvokeOnMainThread(() =>
				{
					// Only re-apply if window is still available
					if (Window != null)
					{
						if (_customDots != null)
						{
							UpdateCustomIndicators();
						}
						if (_hasShadow && _customDots != null)
						{
							ApplyShadowToCustomDots();
						}
					}
				});
			}
		}

		public void UpdateIndicatorSize()
		{
			if (IndicatorSize == 0 || IndicatorSize == DefaultIndicatorSize)
				return;

			// Reset any existing transform that might be causing issues
			Transform = CGAffineTransform.MakeIdentity();

			// Create custom indicators instead of using transform
			CreateCustomIndicators();
		}

		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "These references are owned by the view hierarchy and will be cleaned up when the control is disposed")]
		private UIView? _customIndicatorsContainer;

		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "These references are owned by the view hierarchy and will be cleaned up when the control is disposed")]
		private UIView[]? _customDots;

		private void CreateCustomIndicators()
		{
			// Clean up any existing custom indicators
			_customIndicatorsContainer?.RemoveFromSuperview();
			_customIndicatorsContainer = null;

			if (_customDots != null)
			{
				foreach (var dot in _customDots)
				{
					dot.RemoveFromSuperview();
				}
				_customDots = null;
			}

			// Get page count
			int pageCount = (int)Pages;
			if (pageCount == 0)
				return;

			// Hide original indicators
			HideOriginalIndicators();

			// Create a container for our custom indicators
			_customIndicatorsContainer = new UIView(Bounds)
			{
				BackgroundColor = UIColor.Clear,
				UserInteractionEnabled = true,
				Tag = 999 // Tag to identify our custom container
			};

			// Make sure our container is always on top of any native dots
			BringSubviewToFront(_customIndicatorsContainer);

			// Create indicators with the correct size
			_customDots = new UIView[pageCount];

			// Get colors
			var normalColor = PageIndicatorTintColor;
			var selectedColor = CurrentPageIndicatorTintColor;

			// Calculate layout
			float dotSize = (float)IndicatorSize;
			float spacing = dotSize; // Space between dots equal to dot size
			float totalWidth = (pageCount * dotSize) + ((pageCount - 1) * spacing);
			float startX = (float)(Bounds.Width - totalWidth) / 2;
			float centerY = (float)Bounds.Height / 2;

			// Create each dot
			for (int i = 0; i < pageCount; i++)
			{
				var dot = new UIView(new CGRect(
					startX + (i * (dotSize + spacing)),
					centerY - (dotSize / 2),
					dotSize,
					dotSize))
				{
					Tag = i,
					BackgroundColor = i == CurrentPage ? selectedColor : normalColor
				};

				// Make dots circular (or square if IsSquare is true)
				dot.Layer.CornerRadius = IsSquare ? 0 : dotSize / 2;

				// Add tap gesture
				var tapGesture = new UITapGestureRecognizer(() =>
				{
					CurrentPage = (int)dot.Tag;
					SendActionForControlEvents(UIControlEvent.ValueChanged);
					UpdateCustomIndicators(); // Update visual state after selection
				});
				dot.AddGestureRecognizer(tapGesture);
				dot.UserInteractionEnabled = true;

				// Add to container
				_customIndicatorsContainer.AddSubview(dot);
				_customDots[i] = dot;
			}

			// Add the container to our view
			AddSubview(_customIndicatorsContainer);
		}

		private void UpdateCustomIndicators()
		{
			if (_customDots == null)
				return;

			// Make sure our container is visible and in the right position
			if (_customIndicatorsContainer != null)
			{
				_customIndicatorsContainer.Frame = Bounds;
				_customIndicatorsContainer.Hidden = false;
				_customIndicatorsContainer.Alpha = 1.0f;
				BringSubviewToFront(_customIndicatorsContainer);

				// Hide native indicators again
				HideOriginalIndicators();
			}

			// Update colors based on current page
			for (int i = 0; i < _customDots.Length; i++)
			{
				var dot = _customDots[i];

				// Update the color based on whether this dot is the current page
				dot.BackgroundColor = i == CurrentPage ?
					CurrentPageIndicatorTintColor : PageIndicatorTintColor;

				// Make sure each dot is visible
				dot.Hidden = false;
				dot.Alpha = 1.0f;

				// Reapply shadow if needed
				if (_hasShadow)
				{
					// Shadow was getting lost during updates - reapply it
					if (_appliedShadow?.Paint is SolidPaint solidPaint && solidPaint.Color is Color color)
					{
						var uiColor = color.ToPlatform();
						var radius = _appliedShadow.Radius;
						var opacity = _appliedShadow.Opacity;
						var offset = new CGSize(_appliedShadow.Offset.X, _appliedShadow.Offset.Y);

						dot.Layer.ShadowColor = uiColor.CGColor;
						dot.Layer.ShadowOpacity = opacity;
						dot.Layer.ShadowRadius = radius / 2;
						dot.Layer.ShadowOffset = offset;
						dot.Layer.MasksToBounds = false;
					}
				}
			}
		}

		private void HideOriginalIndicators()
		{
			// Completely hide all the original indicators
			// We need to make all existing UIKit dots invisible
			foreach (var subview in Subviews)
			{
				// Skip our custom container
				if (subview == _customIndicatorsContainer)
					continue;

				// First try to hide the content view directly
				if (subview.Subviews.Length > 0)
				{
					foreach (var innerView in subview.Subviews)
					{
						// Completely hide all subviews of the UIPageControl
						innerView.Hidden = true;
						innerView.Alpha = 0;

						// Also check nested views (iOS 14+ has deeper nesting)
						if (innerView.Subviews.Length > 0)
						{
							foreach (var indicatorDot in innerView.Subviews)
							{
								indicatorDot.Hidden = true;
								indicatorDot.Alpha = 0;
							}
						}
					}
				}

				// Also apply to the main subview for good measure (pre-iOS 14)
				if (subview != _customIndicatorsContainer)
				{
					subview.Hidden = true;
					subview.Alpha = 0;
				}
			}
		}

		public void UpdatePosition()
		{
			_updatingPosition = true;
			this.UpdateCurrentPage(GetCurrentPage());
			_updatingPosition = false;

			int GetCurrentPage()
			{
				if (_indicatorView is null || !_indicatorView.TryGetTarget(out var indicatorView))
					return -1;

				var maxVisible = indicatorView.GetMaximumVisible();
				var position = indicatorView.Position;
				var index = position >= maxVisible ? maxVisible - 1 : position;
				return index;
			}
		}

		public void UpdateIndicatorCount()
		{
			if (_indicatorView is null || !_indicatorView.TryGetTarget(out var indicatorView))
				return;

			// Update the native page count
			this.UpdatePages(indicatorView.GetMaximumVisible());

			// If we're using custom indicators, recreate them with the new count
			if (_customIndicatorsContainer != null || IndicatorSize != DefaultIndicatorSize)
			{
				// The page count has changed, so we need to recreate our custom indicators
				CreateCustomIndicators();
			}

			// Update the position
			UpdatePosition();
		}

		void UpdateSquareShape()
		{
			if (!(OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14)))
			{
				UpdateCornerRadius();
				return;
			}

			var uiPageControlContentView = Subviews[0];
			if (uiPageControlContentView.Subviews.Length > 0)
			{
				var uiPageControlIndicatorContentView = uiPageControlContentView.Subviews[0];

				foreach (var view in uiPageControlIndicatorContentView.Subviews)
				{
					if (view is UIImageView imageview)
					{
						if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
							imageview.Image = UIImage.GetSystemImage("squareshape.fill");
						var frame = imageview.Frame;
						//the square shape is not the same size as the circle so we might need to correct the frame
						imageview.Frame = new CGRect(frame.X - 6, frame.Y, frame.Width, frame.Height);
					}
				}
			}
		}

		void UpdateCornerRadius()
		{
			foreach (var view in Subviews)
			{
				view.Layer.CornerRadius = 0;
			}
		}

		void MauiPageControlValueChanged(object? sender, System.EventArgs e)
		{
			if (_updatingPosition || _indicatorView is null || !_indicatorView.TryGetTarget(out var indicatorView))
				return;

			indicatorView.Position = (int)CurrentPage;

			// Update our custom indicators to reflect the new position
			if (_customDots != null)
			{
				UpdateCustomIndicators();
			}

			// If using square shape in iOS13 or lower, update the corner radius
			if (IsSquare && !(OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14)))
				LayoutSubviews();
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			_movedToWindow?.Invoke(this, EventArgs.Empty);

			// Force layout refresh when added to window
			if (Window != null)
			{
				// Wait for the current run loop to complete to ensure UIKit has finished its own layout
				BeginInvokeOnMainThread(() =>
				{
					// Trigger layout refresh
					SetNeedsLayout();
				});
			}
		}
	}
}
