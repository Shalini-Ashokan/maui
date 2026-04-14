using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34925, "Shell flyout items scroll behind FlyoutFooter on iOS", PlatformAffected.iOS)]
public class Issue34925 : TestShell
{
	protected override void Init()
	{
		FlyoutFooterTemplate = new DataTemplate(() =>
		{
			var footer = new Grid
			{
				HeightRequest = 80,
				BackgroundColor = Color.FromArgb("#80993333"),
				AutomationId = "Issue34925FlyoutFooter",
			};
			footer.Children.Add(new Label
			{
				Text = "Flyout Footer",
				TextColor = Colors.White,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			});
			return footer;
		});

		for (int i = 1; i <= 20; i++)
		{
			var page = new ContentPage { Title = $"Page {i}" };
			if (i == 1)
			{
				page.Content = new Label { Text = "Page 1", AutomationId = "Issue34925Page1Label" };
			}
			AddFlyoutItem(page, $"Page {i}");
		}

		IncreaseFlyoutItemsHeightSoUITestsCanClickOnThem();
	}
}
