using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34925, "Shell flyout items scroll behind FlyoutHeader on iOS", PlatformAffected.iOS)]
public class Issue34925 : TestShell
{
	protected override void Init()
	{
		FlyoutHeaderBehavior = FlyoutHeaderBehavior.Default;

		FlyoutHeaderTemplate = new DataTemplate(() =>
		{
			var header = new Grid
			{
				HeightRequest = 150,
				BackgroundColor = Color.FromArgb("#80333399"), // semi-transparent so overlap is visible
				AutomationId = "Issue34925FlyoutHeader",
			};
			header.Children.Add(new Label
			{
				Text = "Flyout Header",
				TextColor = Colors.White,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			});
			return header;
		});

		for (int i = 1; i <= 15; i++)
		{
			var page = new ContentPage { Title = $"Page {i}" };
			if (i == 1)
			{
				// Give the first page a detectable label so the test can wait for it
				page.Content = new Label { Text = "Page 1", AutomationId = "Issue34925Page1Label" };
			}
			AddFlyoutItem(page, $"Page {i}");
		}

		IncreaseFlyoutItemsHeightSoUITestsCanClickOnThem();
	}
}
