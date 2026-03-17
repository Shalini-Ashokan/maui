using Microsoft.Maui.Controls.Shapes;
using Maui.Controls.Sample;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30, "Clipped image shadow should render correctly", PlatformAffected.UWP)]
public class Issue30 : TestContentPage
{
	protected override void Init()
	{
		var shadow = new Shadow
		{
			Brush = Colors.Red,
			Radius = 10,
			Opacity = 1
		};

		var clippedShadowView = new Image
		{
			AutomationId = "ClippedShadowView",
			Aspect = Aspect.AspectFill,
			Source = "header_background.png",
			HeightRequest = 100,
			WidthRequest = 100,
			Shadow = shadow,
			Clip = new EllipseGeometry
			{
				Center = new Point(50, 50),
				RadiusX = 50,
				RadiusY = 50
			}
		};

		var shadowColor = new UITestEntry
		{
			AutomationId = "ShadowColor",
			BackgroundColor = Colors.Blue,
			Text = "#FF0000",
			Placeholder = "Shadow Color Hex",
			IsReadOnly = true,
			IsCursorVisible = false
		};

		var shadowRadiusSlider = new Slider
		{
			AutomationId = "ShadowRadiusSlider",
			Minimum = 0,
			Maximum = 20,
			Value = 10
		};

		var shadowOpacitySlider = new Slider
		{
			AutomationId = "ShadowOpacitySlider",
			Minimum = 0,
			Maximum = 1,
			Value = 1
		};

		shadowRadiusSlider.ValueChanged += (_, e) => shadow.Radius = (float)e.NewValue;
		shadowOpacitySlider.ValueChanged += (_, e) => shadow.Opacity = (float)e.NewValue;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				AutomationId = "Issue30Root",
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children =
				{
					clippedShadowView,
					shadowColor,
					shadowRadiusSlider,
					shadowOpacitySlider
				}
			}
		};
	}
}
