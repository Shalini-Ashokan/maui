namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37440, "Editor auto expands wrongly to MaximumHeightRequest", PlatformAffected.All)]
public class Issue37440 : ContentPage
{
    public Issue37440()
    {
        Content = new VerticalStackLayout
        {
            Padding = new Thickness(16, 40, 16, 16),
            Spacing = 12,
            Children =
            {
                new Label
                {
                    AutomationId = "WaitForLabel",
                    FontSize = 18,
                    Text = "Blue border must be visible above/below the yellow editor.",
                },
                new Border
                {
                    HeightRequest = 200,
                    Background = Colors.Green,
                    Content = new Border
                    {
                        HeightRequest = 150,
                        Background = Colors.Blue,
                        Content = new Editor
                        {
                            Text = "Type here",
                            AutomationId = "Issue37440Editor",
                            MinimumHeightRequest = 50,
                            MaximumHeightRequest = 150,
                            AutoSize = EditorAutoSizeOption.TextChanges,
                            Background = Colors.Yellow,
                            TextColor = Colors.Black,
                        },
                    },
                },
            },
        };
    }
}
