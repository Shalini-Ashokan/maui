using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17560, "[macOS/iOS] Border clipping incorrect when Content is scaled", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue17560 : ContentPage
{
	public Issue17560()
	{
		// Scaled BoxView inside Ellipse border - reproduces clipping bug when content is scaled
		var ellipseBorderScaledContent = new Border
		{
			AutomationId = "EllipseBorderScaledContent",
			StrokeShape = new Ellipse(),
			WidthRequest = 120,
			HeightRequest = 120,
			BackgroundColor = Colors.LightBlue,
			StrokeThickness = 4,
			Stroke = new SolidColorBrush(Colors.LightGreen),
			Content = new BoxView
			{
				Color = Colors.OrangeRed,
				Scale = 2.0
			}
		};

		// Scaled Label inside Ellipse border
		var ellipseBorderScaledLabel = new Border
		{
			AutomationId = "EllipseBorderScaledLabel",
			StrokeShape = new Ellipse(),
			WidthRequest = 120,
			HeightRequest = 120,
			BackgroundColor = Colors.LightBlue,
			StrokeThickness = 4,
			Stroke = new SolidColorBrush(Colors.LightGreen),
			Content = new Label
			{
				Text = "A",
				BackgroundColor = Colors.Pink,
				FontSize = 48,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				Scale = 1.5
			}
		};

		// Scaled BoxView inside RoundRectangle border
		var roundRectBorderScaledContent = new Border
		{
			AutomationId = "RoundRectBorderScaledContent",
			StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
			WidthRequest = 120,
			HeightRequest = 120,
			BackgroundColor = Colors.LightBlue,
			StrokeThickness = 4,
			Stroke = new SolidColorBrush(Colors.LightGreen),
			Content = new BoxView
			{
				Color = Colors.DodgerBlue,
				Scale = 2.0
			}
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = new Thickness(20),
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						AutomationId = "PageReadyLabel",
						Text = "Border Clip with Scaled Content",
						HorizontalTextAlignment = TextAlignment.Center,
						FontAttributes = FontAttributes.Bold
					},
					new Label { Text = "Ellipse + Scale=2 BoxView", HorizontalTextAlignment = TextAlignment.Center },
					ellipseBorderScaledContent,
					new Label { Text = "Ellipse + Scale=1.5 Label", HorizontalTextAlignment = TextAlignment.Center },
					ellipseBorderScaledLabel,
					new Label { Text = "RoundRect + Scale=2 BoxView", HorizontalTextAlignment = TextAlignment.Center },
					roundRectBorderScaledContent
				}
			}
		};
	}
}
