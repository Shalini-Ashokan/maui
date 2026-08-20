namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37440, "Editor auto expands wrongly to MaximumHeightRequest", PlatformAffected.All)]
public class Issue37440 : ContentPage
{
    public Issue37440()
    {
        Content = new Border
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
        };
    }
}
