namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	void OnLightClicked(object sender, EventArgs e)
	{
		if (Application.Current is { } app)
			app.UserAppTheme = AppTheme.Light;
	}

	void OnDarkClicked(object sender, EventArgs e)
	{
		if (Application.Current is { } app)
			app.UserAppTheme = AppTheme.Dark;
	}
}