using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17523, "Border clip does not constrain scaled content on Windows", PlatformAffected.UWP)]
public class Issue17523 : TestContentPage
{
	Label _scaleLabel;
	Image _roundRectangleImage;
	Image _triangleImage;
	double _scale = 1;

	protected override void Init()
	{
		Title = "Issue 17523 - Border clip vs Scale";

		var instructionsLabel = new Label
		{
			AutomationId = "InstructionsLabel",
			Text = "Scaled Image inside a Border must stay clipped to the Border's shape - it should never spill outside the Border's edges, regardless of Scale."
		};

		_scaleLabel = new Label
		{
			AutomationId = "ScaleLabel",
			Text = "Scale: 1",
			FontAttributes = FontAttributes.Bold
		};

		var increaseScaleButton = new Button
		{
			AutomationId = "IncreaseScaleButton",
			Text = "Increase Scale"
		};
		increaseScaleButton.Clicked += OnIncreaseScaleClicked;

		_roundRectangleImage = new Image
		{
			AutomationId = "RoundRectangleImage",
			Source = "dotnet_bot.png"
		};

		var roundRectangleBorder = new Border
		{
			AutomationId = "RoundRectangleBorder",
			WidthRequest = 150,
			HeightRequest = 150,
			BackgroundColor = Colors.LightBlue,
			Stroke = Colors.LightGreen,
			StrokeThickness = 8,
			StrokeShape = new RoundRectangle { CornerRadius = 12 },
			Content = _roundRectangleImage
		};

		_triangleImage = new Image
		{
			AutomationId = "TriangleImage",
			Source = "dotnet_bot.png"
		};

		// Non-rectangular shape: a triangle drawn as a Polygon. Border clipping to arbitrary
		// (non-IRoundRectangle) shapes exercises a different code path than a plain/rounded
		// rectangle - see ContentPanel.UpdateClip on Windows.
		var triangleBorder = new Border
		{
			AutomationId = "TriangleBorder",
			WidthRequest = 150,
			HeightRequest = 150,
			BackgroundColor = Colors.LightBlue,
			Stroke = Colors.LightGreen,
			StrokeThickness = 8,
			StrokeShape = new Polygon
			{
				Points = new PointCollection { new Point(75, 0), new Point(150, 150), new Point(0, 150) },
				StrokeThickness = 3
			},
			Content = _triangleImage
		};

		var shapesGrid = new Grid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star }
			},
			ColumnSpacing = 20,
			RowSpacing = 20
		};

		shapesGrid.Add(roundRectangleBorder, 0, 0);
		shapesGrid.Add(triangleBorder, 1, 0);

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				instructionsLabel,
				_scaleLabel,
				increaseScaleButton,
				shapesGrid
			}
		};
	}

	void OnIncreaseScaleClicked(object sender, System.EventArgs e)
	{
		// Step the scale up (and wrap back down) so repeated taps keep exercising the
		// Border clip vs Scale interaction described in https://github.com/dotnet/maui/issues/17523.
		_scale = _scale >= 2.5 ? 1 : _scale + 0.5;

		_roundRectangleImage.Scale = _scale;
		_triangleImage.Scale = _scale;

		_scaleLabel.Text = $"Scale: {_scale}";
	}
}
