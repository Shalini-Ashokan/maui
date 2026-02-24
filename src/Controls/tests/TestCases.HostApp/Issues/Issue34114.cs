using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34114, "Label with background clip is not working properly", PlatformAffected.iOS | PlatformAffected.macOS | PlatformAffected.UWP)]
public class Issue34114 : ContentPage
{
	Label _clipTestLabel;
	Button _changeClipButton;
	public Issue34114()
	{
		_clipTestLabel = new Label
		{
			Text = "Sample Text for Clipping Test Multiple Lines To Show Clip Effect",
			BackgroundColor = Colors.Red,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			WidthRequest = 200,
			HeightRequest = 200,
			AutomationId = "ClippedLabel",
			Clip = new EllipseGeometry
			{
				Center = new Point(100, 100),
				RadiusX = 100,
				RadiusY = 100
			}
		};

		_changeClipButton = new Button
		{
			Text = "Change Clip",
			AutomationId = "ChangeClip"
		};

		_changeClipButton.Clicked += (s, e) =>
		{
			_clipTestLabel.Clip = new RectangleGeometry
			{
				Rect = new Rect(0, 0, 200, 100)
			};
		};

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				_clipTestLabel,
				_changeClipButton
			}
		};
	}
}
