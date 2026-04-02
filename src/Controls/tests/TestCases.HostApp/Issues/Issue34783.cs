using System.Linq;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34783, "CollectionView dynamic item sizing resets after scrolling on Android", PlatformAffected.Android)]
public class Issue34783 : ContentPage
{
	const int DefaultSize = 60;
	const int ExpandedSize = 120;
	public Issue34783()
	{
		var items = Enumerable.Range(0, 25).Select(i => $"CV_Item_{i}").ToList();

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			ItemsSource = items,
			ItemTemplate = new DataTemplate(() =>
			{
				var boxView = new BoxView
				{
					HeightRequest = DefaultSize,
					WidthRequest = DefaultSize,
					BackgroundColor = Colors.SteelBlue
				};

				boxView.SetBinding(AutomationIdProperty, ".");
				var tap = new TapGestureRecognizer();
				tap.Tapped += (s, e) =>
				{
					var box = (BoxView)s;
					box.HeightRequest = box.HeightRequest == DefaultSize ? ExpandedSize : DefaultSize;
				};
				boxView.GestureRecognizers.Add(tap);

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");

				return new HorizontalStackLayout
				{
					Padding = new Thickness(10, 5),
					Spacing = 10,
					Children = { boxView, label }
				};
			})
		};

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star }
			},
			Children = { collectionView }
		};
	}
}
