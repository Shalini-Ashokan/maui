namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		var statusLabel = new Label
		{
			Text = "No action performed",
			FontSize = 18,
			AutomationId = "StatusLabel",
			TextColor = Colors.Black
		};

		var image = new Image
		{
			Source = "dotnet_bot.png",
			HeightRequest = 150,
			WidthRequest = 150,
			AutomationId = "SwipeViewImage"
		};

		var label = new Label
		{
			Text = "Swipe the image",
			FontSize = 18
		};

		var swipeItem = new SwipeItem
		{
			Text = "Delete",
			BackgroundColor = Colors.Red,

		};

		swipeItem.Invoked += (sender, e) =>
		{
			statusLabel.Text = "Delete action invoked!";
		};

		var swipeItems = new SwipeItems
{
swipeItem
};

		var swipeView = new SwipeView
		{
			AutomationId = "TestSwipeView",
			LeftItems = swipeItems,
			Content = image,

		};

		Content = swipeView;
	}
}