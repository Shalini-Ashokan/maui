namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36816, "Android: Clicks being passed through to controls under other Views", PlatformAffected.Android)]
public class Issue36816 : ContentPage
{
	const string ButtonClickedText = "Button Clicked";
	const string InitialText = "Not Clicked";

	public Issue36816()
	{
		var resultLabel = new Label
		{
			Text = InitialText,
			AutomationId = "ResultLabel"
		};

		var button = new Button
		{
			Text = "Click me",
			AutomationId = "CounterBtn",
			HorizontalOptions = LayoutOptions.Fill
		};

		button.Clicked += (sender, e) => resultLabel.Text = ButtonClickedText;

		// The opaque ContentView sits directly on top of the Button in the same
		// Grid cell. It has no gesture recognizers and InputTransparent is left
		// at its default (false), so it should absorb touches and prevent them
		// from reaching the Button underneath.
		var overlay = new ContentView
		{
			BackgroundColor = Colors.Green,
			HeightRequest = 100,
			HorizontalOptions = LayoutOptions.Fill,
			AutomationId = "OverlayView"
		};

		var overlappingGrid = new Grid
		{
			AutomationId = "OverlappingGrid"
		};
		overlappingGrid.Children.Add(button);
		overlappingGrid.Children.Add(overlay);

		Content = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = 30,
			Children =
			{
				overlappingGrid,
				resultLabel
			}
		};
	}
}
