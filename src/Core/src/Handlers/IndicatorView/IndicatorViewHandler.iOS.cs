using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, MauiPageControl>
	{
		protected override MauiPageControl CreatePlatformView() => new MauiPageControl();

		protected override void ConnectHandler(MauiPageControl platformView)
		{
			base.ConnectHandler(platformView);

			platformView?.SetIndicatorView(VirtualView);

			UpdateIndicator();
		}

		protected override void DisconnectHandler(MauiPageControl platformView)
		{
			base.DisconnectHandler(platformView);

			platformView?.SetIndicatorView(null);
		}

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateIndicatorCount();
		}

		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdatePosition();
		}

		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateHideSingle(indicator);
		}

		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateIndicatorCount();
		}

		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateIndicatorSize(indicator);
		}

		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdatePagesIndicatorTintColor(indicator);
		}

		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateCurrentPagesIndicatorTintColor(indicator);
		}

		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateIndicatorShape(indicator);
		}

		void UpdateIndicator()
		{
			if (VirtualView is ITemplatedIndicatorView iTemplatedIndicatorView)
			{
				var indicatorsLayoutOverride = iTemplatedIndicatorView.IndicatorsLayoutOverride;
				UIView? handler;
				if (MauiContext != null && indicatorsLayoutOverride != null)
				{
					ClearIndicators();
					handler = indicatorsLayoutOverride.ToPlatform(MauiContext);
					
					// Disable autoresizing mask to use Auto Layout constraints
					handler.TranslatesAutoresizingMaskIntoConstraints = false;
					
					PlatformView.AddSubview(handler);
					
					// Center the custom template view horizontally in the container
					// Don't set leading/trailing constraints - let the view size itself naturally
					NSLayoutConstraint.ActivateConstraints(new[]
					{
						handler.CenterXAnchor.ConstraintEqualTo(PlatformView.CenterXAnchor),
						handler.CenterYAnchor.ConstraintEqualTo(PlatformView.CenterYAnchor),
						handler.TopAnchor.ConstraintGreaterThanOrEqualTo(PlatformView.TopAnchor),
						handler.BottomAnchor.ConstraintLessThanOrEqualTo(PlatformView.BottomAnchor)
					});
				}
			}

			void ClearIndicators()
			{
				foreach (var child in PlatformView.Subviews)
					child.RemoveFromSuperview();
			}
		}
	}
}