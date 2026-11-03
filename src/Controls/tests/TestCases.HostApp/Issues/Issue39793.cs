#if WINDOWS
using Microsoft.UI.Windowing;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39793, "Native title bar should not display a second MAUI title bar", PlatformAffected.UWP)]
public class Issue39793 : ContentPage
{
	AppWindowTitleBar _platformTitleBar;
	bool _originalExtendsContentIntoTitleBar;

	public Issue39793()
	{
		Content = new Label
		{
			Text = "Only one native Windows title bar should be visible.",
			AutomationId = "Issue39793Content",
			Margin = 20
		};
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		var nativeWindow = Window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window
			?? throw new InvalidOperationException("A Windows platform window is required.");

		var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
		var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
		var appWindow = AppWindow.GetFromWindowId(windowId);

		_platformTitleBar = appWindow.TitleBar;
		_originalExtendsContentIntoTitleBar = _platformTitleBar.ExtendsContentIntoTitleBar;
		_platformTitleBar.ExtendsContentIntoTitleBar = false;
	}

	protected override void OnDisappearing()
	{
		if (_platformTitleBar is not null)
		{
			_platformTitleBar.ExtendsContentIntoTitleBar = _originalExtendsContentIntoTitleBar;
			_platformTitleBar = null;
		}

		base.OnDisappearing();
	}
}
#endif
