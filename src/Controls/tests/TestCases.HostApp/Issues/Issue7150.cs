#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 7150, "EmptyView using Template displayed at the same time as the content", PlatformAffected.UWP)]
public class Issue7150 : TestContentPage
{
	public Issue7150()
	{
		BindingContext = new Issue7150ViewModel();
	}

	protected override void Init()
	{
		var filterButton = new Button
		{
			Text = "Filter",
			AutomationId = "EmptyViewButton"
		};
		filterButton.SetBinding(Button.CommandProperty, "FilterCommand");

		var emptyViewContent = new StackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Children =
				{
					new Label
					{
						Text = "No results matched your filter.",
						Margin = new Thickness(10, 25, 10, 10),
						FontAttributes = FontAttributes.Bold,
						FontSize = 18,
						HorizontalOptions = LayoutOptions.Fill,
						HorizontalTextAlignment = TextAlignment.Center
					},
					new Label
					{
						Text = "Try a broader filter?",
						FontAttributes = FontAttributes.Italic,
						FontSize = 12,
						HorizontalOptions = LayoutOptions.Fill,
						HorizontalTextAlignment = TextAlignment.Center
					}
				}
		};

		var emptyView = new ContentView { Content = emptyViewContent };

		var carouselView = new CarouselView
		{
			EmptyView = emptyView,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontAttributes = FontAttributes.Bold,
					FontSize = 20,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, "Name");

				return new StackLayout { Children = { label } };
			})
		};
		carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Monkeys");

		Content = new StackLayout
		{
			Children =
				{
					filterButton,
					carouselView
				}
		};
	}

	internal class Issue7150ViewModel
	{
		public ObservableCollection<Monkey>? Monkeys { get; private set; }
		public ICommand FilterCommand => new Command(FilterItems);
		readonly IList<Monkey> source;

		public Issue7150ViewModel()
		{
			source = new List<Monkey>();
			source.Add(new Monkey
			{
				Name = "Baboon",
			});

			source.Add(new Monkey
			{
				Name = "Blue Monkey",
			});

			source.Add(new Monkey
			{
				Name = "Capuchin Monkey",
			});

			Monkeys = new ObservableCollection<Monkey>(source);
		}

		private void FilterItems()
		{
			var filter = "xamarin";
			var filteredItems = source.Where(monkey => monkey.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
			foreach (var monkey in source)
			{
				if (!filteredItems.Contains(monkey))
				{
					Monkeys?.Remove(monkey);
				}
				else
				{
					if (Monkeys != null && !Monkeys.Contains(monkey))
					{
						Monkeys.Add(monkey);
					}
				}
			}
		}
	}
}