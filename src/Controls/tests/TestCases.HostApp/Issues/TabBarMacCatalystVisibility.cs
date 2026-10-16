namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 0, "TabBar should be visible on MacCatalyst 18+", PlatformAffected.macOS)]
public class TabBarMacCatalystVisibility : Shell
{
	public TabBarMacCatalystVisibility()
	{
		var tabBar = new TabBar();

		var tab1 = new ShellContent
		{
			Title = "Tab 1",
			Route = "tab1",
			ContentTemplate = new DataTemplate(() => new TabBarMacCatalystVisibilityPage("Tab 1"))
		};

		var tab2 = new ShellContent
		{
			Title = "Tab 2",
			Route = "tab2",
			ContentTemplate = new DataTemplate(() => new TabBarMacCatalystVisibilityPage("Tab 2"))
		};

		var tab3 = new ShellContent
		{
			Title = "Tab 3",
			Route = "tab3",
			ContentTemplate = new DataTemplate(() => new TabBarMacCatalystVisibilityPage("Tab 3"))
		};

		tabBar.Items.Add(tab1);
		tabBar.Items.Add(tab2);
		tabBar.Items.Add(tab3);

		Items.Add(tabBar);
	}

	class TabBarMacCatalystVisibilityPage : ContentPage
	{
		public TabBarMacCatalystVisibilityPage(string tabName)
		{
			Title = tabName;

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Children =
				{
					new Label
					{
						Text = $"Current tab: {tabName}",
						AutomationId = $"CurrentTab{tabName.Replace(" ", "")}",
						FontSize = 24,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					},
					new Label
					{
						Text = "The TabBar should be visible at the bottom of the screen on MacCatalyst",
						HorizontalOptions = LayoutOptions.Center,
						Margin = new Thickness(0, 20, 0, 0)
					}
				}
			};
		}
	}
}
