namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32506, "[iOS 26] Liquid glass hamburger and toolbar icon flashing when Shell BackgroundColor is not white", PlatformAffected.iOS)]
public class Issue32506 : Shell
{
	public Issue32506()
	{
		Shell.SetBackgroundColor(this, Colors.Red);

		var flyoutItem = new FlyoutItem { Title = "Test" };

		flyoutItem.Items.Add(new ShellContent
		{
			Title = "A",
			Route = "MainPage3",
			ContentTemplate = new DataTemplate(() => new Issue32506Page("A", "PageALabel"))
		});

		flyoutItem.Items.Add(new ShellContent
		{
			Title = "B",
			Route = "MainPage4",
			ContentTemplate = new DataTemplate(() => new Issue32506Page("B", "PageBLabel"))
		});

		Items.Add(flyoutItem);
	}

	class Issue32506Page : ContentPage
	{
		public Issue32506Page(string title, string automationId)
		{
			Title = title;
			ToolbarItems.Add(new ToolbarItem { Text = "Test", AutomationId = "ToolbarItemTest" });
			Content = new Label
			{
				Text = $"Page {title}",
				AutomationId = automationId,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}
}
