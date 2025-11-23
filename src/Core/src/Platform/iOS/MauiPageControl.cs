using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
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


		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (Subviews.Length == 0)
				return;

			// Check if we have a templated indicator view
			// Templated indicators have a single subview (the custom layout)
			// Native UIPageControl has nested subview structure
			bool hasTemplatedIndicator = HasTemplatedIndicator();

			if (hasTemplatedIndicator)
			{
				// For templated indicators, set the frame to match the bounds
				// so the custom layout is visible and properly positioned
				if (Subviews.Length > 0)
				{
					Subviews[0].Frame = Bounds;
				}
			}
			else
			{
				// For native indicators, apply the indicator size transform
				UpdateIndicatorSize();

				if (!IsSquare)
					return;

				UpdateSquareShape();
			}
		}

		bool HasTemplatedIndicator()
		{
			// If we have a weak reference to the indicator view, check if it's templated
			if (_indicatorView != null && _indicatorView.TryGetTarget(out var indicatorView))
			{
				return indicatorView is ITemplatedIndicatorView templated && templated.IndicatorsLayoutOverride != null;
			}
			return false;
		}
		public void UpdateIndicatorSize()
		{
			if (IndicatorSize == 0 || IndicatorSize == DefaultIndicatorSize)
				return;

			float scale = (float)IndicatorSize / DefaultIndicatorSize;
			var newTransform = CGAffineTransform.MakeScale(scale, scale);

			foreach (var view in Subviews)
			{
				view.Transform = newTransform;
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
			this.UpdatePages(indicatorView.GetMaximumVisible());
			UpdatePosition();
		}

#pragma warning disable CA1822 // Mark members as static
		public void UpdateIndicatorFlowDirection(IIndicatorView indicatorView)
#pragma warning restore CA1822 // Mark members as static
		{
			// When using a templated indicator view, we need to re-update the indicators
			// to apply the FlowDirection changes to the templated layout
			// The actual propagation happens in UpdateIndicator() in the handler
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
			//if we are iOS13 or lower and we are using a Square shape
			//we need to update the CornerRadius of the new shape.
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
		}
	}
}
