using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8, "[Android] Switch Shadow Does Not Follow Thumb when Toggle On or Off",
	PlatformAffected.Android)]
public partial class Issue8SwitchShadow : ContentPage
{
	public Issue8SwitchShadow()
	{
		InitializeComponent();
		TestSwitch.Toggled += OnSwitchToggled;
		UpdateStatusLabel();
	}

	private void OnSwitchToggled(object sender, ToggledEventArgs e)
	{
		UpdateStatusLabel();
	}

	private void OnToggleClicked(object sender, EventArgs e)
	{
		TestSwitch.IsToggled = !TestSwitch.IsToggled;
	}

	private void UpdateStatusLabel()
	{
		StatusLabel.Text = $"Switch is: {(TestSwitch.IsToggled ? "ON" : "OFF")}";
	}
}