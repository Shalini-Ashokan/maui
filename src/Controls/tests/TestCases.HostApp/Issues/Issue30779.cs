namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "30779", "SearchBar CursorPosition is not updated when the user types", PlatformAffected.All)]
public class Issue30779 : ContentPage
{
	public Issue30779()
	{
		var searchBar = new SearchBar
		{
			AutomationId = "SearchBar",
			Placeholder = "Type here...",
			BackgroundColor = Colors.LightGray,
			HeightRequest = 50
		};

		var label = new Label
		{
			Text = $"SearchBar CursorPosition: {searchBar.CursorPosition}",
			AutomationId = "CursorPositionLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 16
		};

		var button = new Button
		{
			Text = "Change the text",
			AutomationId = "ChangeTextButton",
			HorizontalOptions = LayoutOptions.Center,
			Margin = 10
		};

		button.Clicked += (sender, args) =>
		{
			label.Text = $"SearchBar CursorPosition: {searchBar.CursorPosition}";
		};

		searchBar.TextChanged += (sender, args) =>
		{
			label.Text = $"SearchBar CursorPosition: {searchBar.CursorPosition}";
		};

		Content = new StackLayout
		{
			Children = { searchBar, label, button },
			Spacing = 20,
			Padding = new Thickness(20),
			VerticalOptions = LayoutOptions.Center
		};
	}
}