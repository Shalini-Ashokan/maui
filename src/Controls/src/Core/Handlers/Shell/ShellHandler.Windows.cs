#nullable disable
using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using WBorder = Microsoft.UI.Xaml.Controls.Border;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		// P/Invoke declarations for window style manipulation
		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true)]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		const int GWL_EXSTYLE = -20;
		const int WS_EX_LAYOUTRTL = 0x00400000;
		const uint SWP_NOSIZE = 0x0001;
		const uint SWP_NOMOVE = 0x0002;
		const uint SWP_NOZORDER = 0x0004;
		const uint SWP_FRAMECHANGED = 0x0020;

		ScrollViewer _scrollViewer;
		double? _topAreaHeight = null;
		double? _headerHeight = null;
		double? _headerOffset = null;

		protected override ShellView CreatePlatformView()
		{
			var shellView = new ShellView();
			shellView.SetElement(VirtualView);
			return shellView;
		}

		protected override void ConnectHandler(ShellView platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is MauiNavigationView mauiNavigationView)
				mauiNavigationView.OnApplyTemplateFinished += OnApplyTemplateFinished;

			platformView.Loaded += OnLoaded;
			platformView.PaneOpened += OnPaneOpened;
			platformView.PaneOpening += OnPaneOpening;
			platformView.PaneClosing += OnPaneClosing;
			platformView.ItemInvoked += OnMenuItemInvoked;
		}

		private void OnLoaded(object sender, UI.Xaml.RoutedEventArgs e)
		{
			UpdateValue(nameof(Shell.FlyoutIcon));
			UpdateValue(nameof(Shell.FlyoutBackground));
			UpdateValue(nameof(Shell.FlyoutBackgroundImage));
			UpdateValue(nameof(Shell.FlowDirection));
		}

		protected override void DisconnectHandler(ShellView platformView)
		{
			base.DisconnectHandler(platformView);

			if (platformView is MauiNavigationView mauiNavigationView)
				mauiNavigationView.OnApplyTemplateFinished -= OnApplyTemplateFinished;

			platformView.Loaded -= OnLoaded;
			platformView.PaneOpened -= OnPaneOpened;
			platformView.PaneOpening -= OnPaneOpening;
			platformView.PaneClosing -= OnPaneClosing;
			platformView.ItemInvoked -= OnMenuItemInvoked;
		}

		void OnMenuItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{
			var item = args.InvokedItemContainer?.DataContext;

			if (item is NavigationViewItemViewModel nvm && nvm.Data is Element e)
				(VirtualView as IShellController)?.OnFlyoutItemSelected(e);
			else if (item is Element e2)
				(VirtualView as IShellController)?.OnFlyoutItemSelected(e2);
		}

		void OnApplyTemplateFinished(object sender, System.EventArgs e)
		{
			if (PlatformView == null)
				return;

			_scrollViewer = PlatformView.MenuItemsScrollViewer;

			UpdateValue(nameof(Shell.FlyoutHeaderBehavior));
		}

		void OnPaneOpened(UI.Xaml.Controls.NavigationView sender, object args)
		{
			PlatformView.UpdateFlyoutBackdrop();
		}

		void OnPaneClosing(UI.Xaml.Controls.NavigationView sender, UI.Xaml.Controls.NavigationViewPaneClosingEventArgs args)
		{
			args.Cancel = true;
			VirtualView.FlyoutIsPresented = false;
		}

		void OnPaneOpening(UI.Xaml.Controls.NavigationView sender, object args)
		{
			UpdateValue(nameof(Shell.FlyoutBackground));
			UpdateValue(nameof(Shell.FlyoutVerticalScrollMode));
			UpdateValue(nameof(Shell.FlyoutBackgroundImage));
			PlatformView.UpdateFlyoutBackdrop();
			PlatformView.UpdateFlyoutPosition();
			VirtualView.FlyoutIsPresented = true;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (PlatformView.Element != view)
				PlatformView.SetElement((Shell)view);
		}

		public static void MapFlyoutBackdrop(ShellHandler handler, Shell view)
		{
			if (Brush.IsNullOrEmpty(view.FlyoutBackdrop))
				handler.PlatformView.FlyoutBackdrop = null;
			else
				handler.PlatformView.FlyoutBackdrop = view.FlyoutBackdrop;
		}

		public static void MapCurrentItem(ShellHandler handler, Shell view)
		{
			handler.PlatformView.SwitchShellItem(view.CurrentItem, true);
		}

		public static void MapFlyoutBackground(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdatePaneBackground(
				!Brush.IsNullOrEmpty(view.FlyoutBackground) ?
					view.FlyoutBackground :
					view.FlyoutBackgroundColor?.AsPaint());
		}

		//TODO: Make it public in .NET 10.
		internal static void MapFlyoutBackgroundImage(ShellHandler handler, Shell view)
		{
			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
			if (handler?.PlatformView is not null && provider is not null)
			{
				handler.PlatformView.UpdateBackgroundImageSourceAsync(view.FlyoutBackgroundImage, provider, view.FlyoutBackgroundImageAspect).FireAndForget();
			}
		}

		public static void MapFlyoutIcon(ShellHandler handler, Shell view)
		{
			var flyoutIcon = view.FlyoutIcon;
			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			handler.PlatformView.UpdateFlyoutIconAsync(flyoutIcon, provider).FireAndForget();
		}

		public static void MapFlyoutVerticalScrollMode(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateFlyoutVerticalScrollMode((WScrollMode)(int)view.FlyoutVerticalScrollMode);
		}

		public static void MapFlyout(ShellHandler handler, IFlyoutView flyoutView)
		{
			if (handler.PlatformView is RootNavigationView rnv)
				rnv.FlyoutView = flyoutView.Flyout;

			handler.PlatformView.FlyoutCustomContent = flyoutView.Flyout?.ToPlatform(handler.MauiContext);

		}

		internal static void MapFlowDirection(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateFlowDirection(view);

			// Space required for Windows title bar system buttons: ~138px (46px * 3 buttons)
			//const double WindowsSystemButtonsWidth = 138;

			var windowRootView = handler.MauiContext?.GetNavigationRootManager()?.RootView as WindowRootView;

			// Get the native window handle to apply WS_EX_LAYOUTRTL for dynamic RTL changes
			IntPtr hwnd = IntPtr.Zero;
			var window = handler.MauiContext?.Services?.GetService<Microsoft.UI.Xaml.Window>();
			if (window != null)
			{
				hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
			}
			if (view.FlowDirection == FlowDirection.RightToLeft)
			{
				// RTL: Apply WS_EX_LAYOUTRTL to mirror the entire window including title bar buttons
				if (hwnd != IntPtr.Zero)
				{
					int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
					if ((exStyle & WS_EX_LAYOUTRTL) == 0)
					{
						SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYOUTRTL);
						SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
							SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					}
				}

				// Set FlowDirection at the root level
				if (windowRootView != null && windowRootView.FlowDirection != Microsoft.UI.Xaml.FlowDirection.RightToLeft)
				{
					windowRootView.FlowDirection = Microsoft.UI.Xaml.FlowDirection.RightToLeft;
				}
			}
			else
			{
				// LTR: Remove WS_EX_LAYOUTRTL to restore normal layout
				if (hwnd != IntPtr.Zero)
				{
					int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
					if ((exStyle & WS_EX_LAYOUTRTL) != 0)
					{
						SetWindowLong(hwnd, GWL_EXSTYLE, exStyle & ~WS_EX_LAYOUTRTL);
						SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
							SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					}
				}

				// Reset WindowRootView FlowDirection to LTR
				if (windowRootView != null && windowRootView.FlowDirection != Microsoft.UI.Xaml.FlowDirection.LeftToRight)
				{
					windowRootView.FlowDirection = Microsoft.UI.Xaml.FlowDirection.LeftToRight;
				}
			}
		}
		public static void MapIsPresented(ShellHandler handler, IFlyoutView flyoutView)
		{
			// WinUI Will close the pane inside of the apply template code
			// so we wait until the control is loaded before applying IsPresented
			if (handler.PlatformView.IsLoaded)
				handler.PlatformView.IsPaneOpen = flyoutView.IsPresented;
		}

		public static void MapFlyoutWidth(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutWidth(flyoutView);
		}

		public static void MapFlyoutBehavior(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutBehavior(flyoutView);
		}

		public static void MapFlyoutFooter(ShellHandler handler, Shell view)
		{
			if (handler.PlatformView.PaneFooter == null)
				handler.PlatformView.PaneFooter = new ShellFooterView(view);
		}

		public static void MapFlyoutHeader(ShellHandler handler, Shell view)
		{
			if (handler.PlatformView.PaneHeader == null)
				handler.PlatformView.PaneHeader = new ShellHeaderView(view);
		}

		public static void MapFlyoutHeaderBehavior(ShellHandler handler, Shell view)
		{
			handler.UpdateFlyoutHeaderBehavior(view);
		}

		public static void MapItems(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateMenuItemSource();
			handler.UpdateValue(nameof(Shell.CurrentItem));
		}

		public static void MapFlyoutItems(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateMenuItemSource();
			handler.UpdateValue(nameof(Shell.CurrentItem));
		}

		void UpdateFlyoutHeaderBehavior(Shell view)
		{
			var flyoutHeader = (ShellHeaderView)PlatformView.PaneHeader;

			if (view.FlyoutHeaderBehavior == FlyoutHeaderBehavior.Default ||
				view.FlyoutHeaderBehavior == FlyoutHeaderBehavior.Fixed)
			{
				var defaultHeight = _headerHeight;
				var defaultTranslateY = _headerOffset;

				UpdateFlyoutHeaderTransformation(flyoutHeader, defaultHeight, defaultTranslateY);
				return;
			}

			if (_scrollViewer != null)
			{
				_scrollViewer.ViewChanged -= OnScrollViewerViewChanged;
				_scrollViewer.ViewChanged += OnScrollViewerViewChanged;
			}
		}

		void UpdateFlyoutHeaderTransformation(ShellHeaderView flyoutHeader, double? height, double? translationY)
		{
			if (translationY.HasValue)
			{
				flyoutHeader.RenderTransform = new CompositeTransform
				{
					TranslateY = translationY.Value
				};
			}

			if (height.HasValue)
			{
				flyoutHeader.Height = height.Value;
			}
		}

		void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			if (_scrollViewer == null)
				return;

			var flyoutHeader = PlatformView?.PaneHeader as ShellHeaderView;

			if (flyoutHeader == null)
				return;

			if (_headerHeight == null)
				_headerHeight = flyoutHeader.ActualHeight;

			if (_headerOffset == null)
			{
				if (flyoutHeader.RenderTransform is CompositeTransform compositeTransform)
					_headerOffset = compositeTransform.TranslateY;
				else
					_headerOffset = 0;
			}

			switch (VirtualView?.FlyoutHeaderBehavior)
			{
				case FlyoutHeaderBehavior.Scroll:
					var scrollHeight = Math.Max(_headerHeight.Value - _scrollViewer.VerticalOffset, 0);
					var scrollTranslateY = -_scrollViewer.VerticalOffset;

					UpdateFlyoutHeaderTransformation(flyoutHeader, scrollHeight, scrollTranslateY);
					break;
				case FlyoutHeaderBehavior.CollapseOnScroll:

					if (_topAreaHeight == null)
						_topAreaHeight = Math.Max(PlatformView.TopNavArea?.ActualHeight ?? 0, _headerHeight.Value);

					var calculatedHeight = _headerHeight.Value - _scrollViewer.VerticalOffset;
					var collapseOnScrollHeight = calculatedHeight < _topAreaHeight.Value ? _topAreaHeight.Value : calculatedHeight;

					var offsetY = -_scrollViewer.VerticalOffset;
					var maxOffsetY = -_topAreaHeight.Value;
					var collapseOnScrollTranslateY = offsetY < maxOffsetY ? maxOffsetY : offsetY;

					UpdateFlyoutHeaderTransformation(flyoutHeader, collapseOnScrollHeight, collapseOnScrollTranslateY);
					break;
			}
		}
	}
}
