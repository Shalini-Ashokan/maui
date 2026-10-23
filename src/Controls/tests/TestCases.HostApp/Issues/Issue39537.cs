namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39537, "ToolbarItem icons Does not show in Windows", PlatformAffected.UWP)]
public class Issue39537 : TestNavigationPage
{
	protected override void Init()
	{
		BarBackground = Brush.Red;
		BarTextColor = Colors.Black;

		var page = new ContentPage
		{
			Title = "Font toolbar icons",
			Background = Brush.White,
			Content = new Label
			{
				AutomationId = "Issue39537Ready",
				Text = "The toolbar should show X, Y and Z font icons.",
				TextColor = Colors.Black,
				Margin = 20,
			}
		};

		page.ToolbarItems.Add(new ToolbarItem
		{
			AutomationId = "FontToolbarItemX",
			Text = "Letter X",
			Order = ToolbarItemOrder.Primary,
			Priority = 0,
			IconImageSource = new FontImageSource { Glyph = "X", Color = Colors.Black, Size = 24 }
		});
		page.ToolbarItems.Add(new ToolbarItem
		{
			AutomationId = "FontToolbarItemY",
			Order = ToolbarItemOrder.Primary,
			Priority = 1,
			IconImageSource = new FontImageSource { Glyph = "Y", Color = Colors.Black, Size = 24 }
		});
		page.ToolbarItems.Add(new ToolbarItem
		{
			AutomationId = "FontToolbarItemZ",
			Text = "Letter Z",
			Order = ToolbarItemOrder.Primary,
			Priority = 2,
			IconImageSource = new FontImageSource { Glyph = "Z", Color = Colors.Black, Size = 24 }
		});

		Navigation.PushAsync(page);
	}
}
