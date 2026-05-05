using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7150, "EmptyView using Template displayed at the same time as the content", PlatformAffected.UWP)]
public class Issue7150 : ContentPage
{
	readonly ObservableCollection<string> _items = new ObservableCollection<string>
	{
		"Baboon",
		"Capuchin Monkey",
		"Blue Monkey"
	};

	public Issue7150()
	{
		Title = "Issue 7150";

		var filterButton = new Button
		{
			Margin = new Thickness(20),
			AutomationId = "FilterButton",
			Text = "Filter"
		};

		var emptyViewContent = new StackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,
			Children =
				{
					new Label
					{
						Text = "No results matched your filter.",
						Margin = new Thickness(10, 25, 10, 10),
						FontAttributes = FontAttributes.Bold,
						FontSize = 18,
						HorizontalTextAlignment = TextAlignment.Center
					},
					new Label
					{
						Text = "Try a broader filter?",
						FontAttributes = FontAttributes.Italic,
						FontSize = 12,
						HorizontalTextAlignment = TextAlignment.Center
					}
				}
		};

		var emptyView = new ContentView { Content = emptyViewContent };
		var carouselView = new CarouselView
		{
			AutomationId = "CarouselView",
			ItemsSource = _items,
			ItemTemplate = GetCarouselTemplate(),
			EmptyView = emptyView,
			Loop = true,
			HeightRequest = 300,
		};

		filterButton.Clicked += (sender, e) =>
		{
			// Filter to "Mandrill" which matches nothing, resulting in an empty view
			var filter = "Mandrill";
			var toRemove = _items.Where(name => !name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
			foreach (var item in toRemove)
				_items.Remove(item);
		};

		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};
		grid.Add(filterButton, 0, 0);
		grid.Add(carouselView, 0, 1);
		Content = grid;
	}

	DataTemplate GetCarouselTemplate()
	{
		return new DataTemplate(() =>
		{
			var grid = new Grid();
			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(6)
			};

			info.SetBinding(Label.TextProperty, new Binding("."));
			grid.Children.Add(info);
			return grid;
		});
	}
}