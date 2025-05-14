using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29421, "KeepScrollOffset Not Working as Expected in CarouselView", PlatformAffected.UWP)]
public class Issue29421 : ContentPage
{
	public Issue29421()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var carouselItems = new ObservableCollection<string>
		{
			"Item 0",
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
		};

		CarouselView carouselView = new CarouselView
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
			HeightRequest = 300,
			Position = 1,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Padding = 10
				};

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 18,
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				grid.Children.Add(label);
				return grid;
			}),
			HorizontalOptions = LayoutOptions.Fill,
		};

		var indicatorView = new IndicatorView
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		carouselView.IndicatorView = indicatorView;

		var horizontalStack = new HorizontalStackLayout();
		var addButton = new Button
		{
			Text = "Add item",
			AutomationId = "AddButton",
			Margin = new Thickness(20),
		};

		var removeButton = new Button
		{
			Text = "Remove item",
			AutomationId = "RemoveButton",
			Margin = new Thickness(20),
		};
		horizontalStack.Children.Add(addButton);
		horizontalStack.Children.Add(removeButton);

		var label = new Label
		{
			AutomationId = "positionLabel",
			Text = $"Current Item{carouselView.CurrentItem}, Position {carouselView.Position}",
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(20),
		};

		addButton.Clicked += (sender, e) =>
		{
			carouselItems.Insert(0, "NewItem");
			label.Text = $"Current Item: {carouselView.CurrentItem}, Position: {carouselView.Position}";
		};

		removeButton.Clicked += (sender, e) =>
		{
			carouselItems.RemoveAt(0);
			label.Text = $"Current Item: {carouselView.CurrentItem}, Position: {carouselView.Position}";
		};

		verticalStackLayout.Children.Add(horizontalStack);
		verticalStackLayout.Children.Add(label);
		verticalStackLayout.Children.Add(carouselView);
		verticalStackLayout.Children.Add(indicatorView);
		Content = verticalStackLayout;
	}
}