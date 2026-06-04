using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 14577, "Width or Height x Reference bindings show wrong values on initial render on Windows", PlatformAffected.UWP)]
	public class Issue14577 : TestContentPage
	{
		protected override void Init()
		{
			var lblDistanceText = new Label
			{
				AutomationId = "ReferenceLabel",
				Text = "Distance",
				FontSize = 18,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			};

			var line = new Line
			{
				Stroke = new SolidColorBrush(Colors.DarkGreen),
				StrokeThickness = 3,
				StrokeLineCap = PenLineCap.Round,
				Opacity = 0.85,
				HorizontalOptions = LayoutOptions.Center,
				Shadow = new Shadow
				{
					Brush = new SolidColorBrush(Colors.DarkGreen),
					Offset = new Point(0, 0),
					Radius = 6,
					Opacity = 0.9f
				}
			};
			line.SetBinding(Line.X2Property, new Binding(nameof(Label.Width), source: lblDistanceText));

			var lblTotalDistance = new Label
			{
				Text = "TotalDistance here",
				FontSize = 18,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			};

			Content = new VerticalStackLayout
			{
				Children = { lblDistanceText, line, lblTotalDistance }
			};
		}
	}
}
