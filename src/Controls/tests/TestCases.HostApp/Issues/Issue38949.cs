using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38949, "Windows TitleBar handler context when navigating to modal pages", PlatformAffected.UWP)]
public class Issue38949 : ContentPage
{
    readonly TitleBar _titleBar;

    public Issue38949()
    {
        _titleBar = new TitleBar
        {
            Title = "Test Title",
            Subtitle = "Test Subtitle"
        };

        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "TitleBar modal regression test"
                },

                new Button
                {
                    Text = "Push Modal Page",
                    AutomationId = "TitleBarPushModal",
                    Command = new Command(async () =>
                    {
                        await Navigation.PushModalAsync(
                            new Issue38949());
                    })
                },

                new Button
                {
                    Text = "Pop Modal Page",
                    AutomationId = "TitleBarPopModal",
                    Command = new Command(async () =>
                    {
                        await Navigation.PopModalAsync();
                    })
                }
            }
        };
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (Window is not null)
            Window.TitleBar = _titleBar;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        if (Window is not null &&
            Navigation.ModalStack.Count == 0)
        {
            Window.TitleBar = null;
        }
    }
}
