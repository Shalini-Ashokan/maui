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
					Background = Colors.LightBlue,
					Content = label
				};
			})
		};

		var layout = new Grid
		{
			Padding = 20,
			RowSpacing = 20,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			}
		};
		layout.Add(new Label
		{
			AutomationId = "Issue31141Label",
			Text = "Test passes if the IndicatorView indicators are visible when FlowDirection is RightToLeft with a custom IndicatorTemplate.",
			HorizontalOptions = LayoutOptions.Center
		});
		layout.Add(carouselView, row: 1);
		layout.Add(indicatorView, row: 2);
		Content = layout;
	}
}
