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

		label.SetBinding(Label.TextProperty, new Binding
		{
			Path = "CursorPosition",
			Source = searchBar,
			StringFormat = "SearchBar CursorPosition: {0}"
		});

		Content = new StackLayout
		{
			Children = { searchBar, label },
			Spacing = 20,
			Padding = new Thickness(20),
			VerticalOptions = LayoutOptions.Center
		};
	}
}