namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34975, "Title view memory leak when using Shell TitleView and x Name", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34975 : Shell
{
	public Issue34975()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;
		Routing.RegisterRoute("Issue34975_second", typeof(Issue34975SecondPage));

		var navigateButton = new Button
		{
			Text = "Navigate to Second Page",
			AutomationId = "NavigateButton",
		};

		var checkButton = new Button
		{
			Text = "Check Memory",
			AutomationId = "CheckMemoryButton",
			IsVisible = false,
		};

		var statusLabel = new Label
		{
			Text = "1. Tap Navigate, then Tap Check Memory",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "StatusLabel",
		};

		navigateButton.Clicked += async (s, e) =>
		{
			Issue34975SecondPage.Instances.Clear();

			// Round 1: track the page instance we want to verify gets collected.
			Issue34975SecondPage.IsTracking = true;
			await Shell.Current.GoToAsync("Issue34975_second");
			await Shell.Current.GoToAsync("..");
			Issue34975SecondPage.IsTracking = false;
			await Task.Delay(100);

			// Round 2: navigate again so that Mac's AXObserver (used by Appium) updates its
			// tracked accessibility elements from Round 1 → Round 2, releasing the native
			// hold on Round 1's views and allowing GC to collect Round 1's page.
			await Shell.Current.GoToAsync("Issue34975_second");
			await Shell.Current.GoToAsync("..");
			await Task.Delay(300);

			checkButton.IsVisible = true;
			statusLabel.Text = "Now tap Check Memory";
		};

		checkButton.Clicked += async (s, e) =>
		{
			statusLabel.Text = "Checking...";

			try
			{
				await GarbageCollectionHelper.WaitForGC(10000, Issue34975SecondPage.Instances.ToArray());
			}
			catch { }

			var alive = Issue34975SecondPage.Instances.Count(wr => wr.IsAlive);
			statusLabel.Text = $"Still alive: {alive}";
		};

		var mainPage = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 15,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					statusLabel,
					navigateButton,
					checkButton,
				}
			}
		};

		Items.Add(new ShellContent
		{
			Content = mainPage,
			Route = "Issue34975_main",
		});
	}
}
