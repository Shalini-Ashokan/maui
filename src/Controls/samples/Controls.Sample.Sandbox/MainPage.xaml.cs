namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
#if WINDOWS
	readonly Label _titleBarStatus = new() { AutomationId = "TitleBarStatus" };
#endif

	public MainPage()
	{
		InitializeComponent();
		Title = "Home";

#if WINDOWS
		var nativeButton = new Button
		{
			Text = "Use native title bar (false)",
			AutomationId = "UseNativeTitleBar"
		};
		nativeButton.Clicked += (_, _) => SetTitleBarMode(false);

		var mauiButton = new Button
		{
			Text = "Use MAUI title bar (true)",
			AutomationId = "UseMauiTitleBar"
		};
		mauiButton.Clicked += (_, _) => SetTitleBarMode(true);

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 12,
			Children =
			{
				new Label { Text = "Tap false, then true, after the window opens. Check for duplicate or missing title bars." },
				_titleBarStatus,
				nativeButton,
				mauiButton
			}
		};

		Loaded += (_, _) =>
		{
			Window.Title = "TitleBarBugRepro";
			var nativeWindow = (Microsoft.UI.Xaml.Window)Window.Handler.PlatformView;
			_titleBarStatus.Text = $"ExtendsContentIntoTitleBar = {nativeWindow.AppWindow.TitleBar.ExtendsContentIntoTitleBar}";
		};
#else
		Content = new Label { Text = "This title bar reproduction is Windows-only.", Margin = 20 };
#endif
	}

#if WINDOWS
	void SetTitleBarMode(bool extendsContentIntoTitleBar)
	{
		var nativeWindow = (Microsoft.UI.Xaml.Window)Window.Handler.PlatformView;
		var titleBar = nativeWindow.AppWindow.TitleBar;
		titleBar.ExtendsContentIntoTitleBar = extendsContentIntoTitleBar;
		_titleBarStatus.Text = $"ExtendsContentIntoTitleBar = {titleBar.ExtendsContentIntoTitleBar}";
	}
#endif
}