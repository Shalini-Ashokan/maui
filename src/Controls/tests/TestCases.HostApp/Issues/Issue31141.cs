namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31141, "IndicatorView not visible when applying FlowDirection RightToLeft with IndicatorTemplate", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31141 : ContentPage
{
	public Issue31141()
	{
		var indicatorView = new IndicatorView
		{
			AutomationId = "IndicatorView31141",
			IndicatorColor = Colors.LightGray,
			SelectedIndicatorColor = Colors.Red,
			HorizontalOptions = LayoutOptions.Center,
			FlowDirection = FlowDirection.RightToLeft,
		};

		indicatorView.IndicatorTemplate = new DataTemplate(() =>
		{
			return new Image
			{
				Source = new FontImageSource
				{
					FontFamily = "Ion",
					Glyph = "\uf30c",
					Size = 40,
					Color = Colors.DodgerBlue
				},
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(4, 0)
			};
		});

		var carouselView = new CarouselView
		{
			AutomationId = "CarouselView31141",
			Loop = false,
			HeightRequest = 200,
			IndicatorView = indicatorView,
			ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 24
				};
				label.SetBinding(Label.TextProperty, ".");
				return new Frame
				{
					BackgroundColor = Colors.LightBlue,
					Content = label
				};
			})
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label
				{
					AutomationId = "Issue31141Label",
					Text = "Test passes if the IndicatorView indicators are visible when FlowDirection is RightToLeft with a custom IndicatorTemplate.",
					HorizontalOptions = LayoutOptions.Center
				},
				carouselView,
				indicatorView
			}
		};
	}
}
