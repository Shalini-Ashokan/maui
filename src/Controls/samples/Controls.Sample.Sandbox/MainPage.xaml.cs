namespace Maui.Controls.Sample;

public partial class MainPage : Issue35613NavigationReproPageBase
{
	public MainPage()
	{
		InitializeComponent();
		SetPageName("Page1");
		SetLogEditor(ReproLogEditor);
	}

	async void OnGotoPage2Clicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue35613Page2());
	}

	void OnClearLogClicked(object sender, EventArgs e)
	{
		ClearLog();
	}
}