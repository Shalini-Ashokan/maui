using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, ContentViewGroup>
	{
		protected override ContentViewGroup CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ContentViewGroup");
			}

			var viewGroup = new ContentViewGroup(Context)
			{
				CrossPlatformLayout = VirtualView,
				// A ContentView represents a solid, hit-testable rectangle (unless InputTransparent),
				// so it should consume touches like it does on iOS/Windows instead of leaking clicks
				// through to whatever is stacked behind it.
				ConsumesUnhandledTouches = true
			};

			viewGroup.SetClipChildren(false);

			return viewGroup;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformLayout = VirtualView;
		}

		static void UpdateContent(IContentViewHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			handler.PlatformView.RemoveAllViews();

			if (handler.VirtualView.PresentedContent is IView view)
			{
				var platformView = view.ToPlatform(handler.MauiContext);
				// Ensure the view is detached from any existing parent before adding it
				platformView.RemoveFromParent();
				handler.PlatformView.AddView(platformView);
			}
		}

		public static partial void MapContent(IContentViewHandler handler, IContentView page)
		{
			UpdateContent(handler);
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.InputTransparent"/> property to the platform-specific implementation.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IContentView"/> instance.</param>
		public static partial void MapInputTransparent(IContentViewHandler handler, IContentView view)
		{
			// Preserve the base behavior of propagating InputTransparent to a WrapperView container
			// (used when this ContentView has a Shadow, Clip, or Border applied).
			ViewHandler.MapInputTransparent(handler, view);

			if (handler.PlatformView is ContentViewGroup contentViewGroup)
			{
				contentViewGroup.InputTransparent = view.InputTransparent;
			}
		}

		protected override void DisconnectHandler(ContentViewGroup platformView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its children
			platformView.CrossPlatformLayout = null;
			platformView.RemoveAllViews();
			base.DisconnectHandler(platformView);
		}
	}
}
