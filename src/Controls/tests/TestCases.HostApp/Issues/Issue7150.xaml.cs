#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 7150, "EmptyView using Template displayed at the same time as the content", PlatformAffected.UWP)]
public partial class Issue7150 : ContentPage
{
	public Issue7150()
	{
		InitializeComponent();
		BindingContext = new Issue7150ViewModel();
	}

	internal class Issue7150ViewModel
	{
		public ObservableCollection<Monkey>? Monkeys { get; private set; }
		public ICommand FilterCommand => new Command<string>(FilterItems);
		readonly IList<Monkey> source;

		public Issue7150ViewModel()
		{
			source = new List<Monkey>();
			source.Add(new Monkey
			{
				Name = "Baboon",
				Location = "Africa & Asia",
				Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae."
			});

			source.Add(new Monkey
			{
				Name = "Blue Monkey",
				Location = "Central and East Africa",
				Details = "The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia",
			});

			source.Add(new Monkey
			{
				Name = "Capuchin Monkey",
				Location = "Central and South America",
				Details = "Capuchin monkeys are New World monkeys of the subfamily Cebinae, found in Central and South America."
			});

			Monkeys = new ObservableCollection<Monkey>(source);
		}

		private void FilterItems(string filter)
		{
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

public class FilterData : BindableObject
{
	public static readonly BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(string), typeof(FilterData), null);

	public string Filter
	{
		get { return (string)GetValue(FilterProperty); }
		set { SetValue(FilterProperty, value); }
	}
}