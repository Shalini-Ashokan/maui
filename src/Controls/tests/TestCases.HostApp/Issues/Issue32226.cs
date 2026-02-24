namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32226,
	"TapGestureRecognizer Tapped not called when Android Touch handler has e.Handled = false",
	PlatformAffected.Android)]
public class Issue32226 : ContentPage
{
	public Issue32226()
	{
		var resultLabel = new Label
		{
			AutomationId = "ResultLabel",
			Text = "Waiting",
			HorizontalOptions = LayoutOptions.Center,
		};

		var tapLabel = new Label
		{
			AutomationId = "TapLabel",
			Text = "Tap me",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 24,
			BackgroundColor = Colors.LightBlue,
			Padding = new Thickness(20),
		};

		var tapGestureRecognizer = new TapGestureRecognizer();
		tapGestureRecognizer.Tapped += (s, e) =>
		{
			resultLabel.Text = "Tapped";
		};
		tapLabel.GestureRecognizers.Add(tapGestureRecognizer);

		var resetButton = new Button
		{
			AutomationId = "ResetButton",
			Text = "Reset",
		};
		resetButton.Clicked += (s, e) => resultLabel.Text = "Waiting";

#if ANDROID
		tapLabel.HandlerChanged += (s, e) =>
		{
			if (tapLabel.Handler?.PlatformView is Android.Views.View platformView)
			{
				// Reproduce the bug: subscribe a Touch handler that sets e.Handled = false.
				// This should NOT prevent TapGestureRecognizer from firing.
				platformView.Touch += (sender, touchEvent) =>
				{
					touchEvent.Handled = false;
				};
			}
		};

		// Simulates TalkBack activation by calling PerformClick() directly on the platform view.
		// When Clickable = true, TalkBack ultimately calls performClick() (via ACTION_CLICK in
		// performAccessibilityActionInternal). Our OnClickListener handles this and fires the tap gesture.
		var accessibilityTapButton = new Button
		{
			AutomationId = "AccessibilityTapButton",
			Text = "Simulate TalkBack Tap",
		};
		accessibilityTapButton.Clicked += (s, e) =>
		{
			if (tapLabel.Handler?.PlatformView is Android.Views.View platformView)
				platformView.PerformClick();
		};
#endif

		var layout = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20),
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				tapLabel,
				resultLabel,
				resetButton,
#if ANDROID
				accessibilityTapButton,
#endif
			},
		};

		Content = layout;
	}
}
