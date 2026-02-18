using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21068, "[Android] IView with initial Rotation has incorrect offsets of PivotX and PivotY", PlatformAffected.Android)]
public class Issue21068 : TestContentPage
{
    protected override void Init()
    {
        var vm = new Issue21068ViewModel();
        BindingContext = vm;

        var image = new Image
        {
            BackgroundColor = Colors.Red,
            Source = "dotnet_bot.png",
            WidthRequest = 100,
            HeightRequest = 100,
            Margin = new Thickness(0, 100),
            HorizontalOptions = LayoutOptions.Center,
            AutomationId = "TestBox"
        };
        image.SetBinding(Image.RotationProperty, nameof(Issue21068ViewModel.TestRotation));

        var changeBindingButton = new Button
        {
            Text = "Change Rotation via Binding",
            AutomationId = "ChangeRotationButton"
        };

        changeBindingButton.Clicked += (s, e) =>
        {
            vm.TestRotation += 90;
        };

        Content = new VerticalStackLayout
        {
            Spacing = 10,
            Padding = new Thickness(20),
            Children =
                {
                    image,
                    changeBindingButton
                }
        };
    }
}

public class Issue21068ViewModel : INotifyPropertyChanged
{
    double _testRotation = 90;
    public double TestRotation
    {
        get => _testRotation;
        set
        {
            if (_testRotation != value)
            {
                _testRotation = value;
                OnPropertyChanged();
            }
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
