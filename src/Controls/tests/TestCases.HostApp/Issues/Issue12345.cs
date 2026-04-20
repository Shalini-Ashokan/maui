namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 12345, "Control backgrounds remain visible after being set to null",
		PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue12345 : ContentPage
	{
		public Issue12345()
		{
			var label = new Label
			{
				Text = "Label",
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundLabel",
				Padding = new Thickness(10),
			};

			var checkBox = new CheckBox
			{
				IsChecked = true,
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundCheckBox",
			};

			var switchControl = new Switch
			{
				IsToggled = true,
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundSwitch",
			};

			var image = new Image
			{
				Source = "dotnet_bot.png",
				HeightRequest = 50,
				WidthRequest = 50,
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundImage",
			};

			var imageButton = new ImageButton
			{
				Source = "dotnet_bot.png",
				HeightRequest = 50,
				WidthRequest = 50,
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundImageButton",
			};

			var slider = new Slider
			{
				Minimum = 0,
				Maximum = 100,
				Value = 50,
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundSlider",
			};

			var progressBar = new ProgressBar
			{
				Progress = 0.5,
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundProgressBar",
			};

			var stepper = new Stepper
			{
				Minimum = 0,
				Maximum = 10,
				Value = 5,
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundStepper",
			};

			var radioButton = new RadioButton
			{
				Content = "RadioButton",
				Background = new SolidColorBrush(Colors.Blue),
				AutomationId = "BackgroundRadioButton",
			};

			var verticalStack = new VerticalStackLayout
			{
				Background = new SolidColorBrush(Colors.Blue),
				Padding = new Thickness(10),
				AutomationId = "BackgroundVerticalStack",
				Children =
				{
					new Label { Text = "VerticalStackLayout" },
				}
			};

			var grid = new Grid
			{
				Background = new SolidColorBrush(Colors.Blue),
				Padding = new Thickness(10),
				AutomationId = "BackgroundGrid",
				Children =
				{
					new Label { Text = "Grid" },
				}
			};

			var setButton = new Button
			{
				Text = "Set Background to null",
				AutomationId = "SetBackgroundButton",
			};
			setButton.Clicked += (s, e) =>
			{
				label.Background = null;
				checkBox.Background = null;
				switchControl.Background = null;
				image.Background = null;
				imageButton.Background = null;
				slider.Background = null;
				progressBar.Background = null;
				stepper.Background = null;
				radioButton.Background = null;
				verticalStack.Background = null;
				grid.Background = null;
			};

			Content = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					Spacing = 10,
					Margin = new Thickness(20),
					Children =
					{
						label,
						checkBox,
						switchControl,
						image,
						imageButton,
						slider,
						progressBar,
						stepper,
						radioButton,
						verticalStack,
						grid,
						setButton,
					}
				}
			};
		}
	}
}
