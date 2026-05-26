namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35613, "OnNavigatingFrom DestinationPage is set to current page instead of actual destination page", PlatformAffected.All)]
public class Issue35613 : NavigationPage
{
	internal static string PushNavigatingFromDestination = string.Empty;
	internal static string PopNavigatingFromDestination = string.Empty;

	public Issue35613() : base(new Issue35613Page1())
	{
	}

	public class Issue35613Page1 : ContentPage
	{
		Label _popDestLabel;

		public Issue35613Page1()
		{
			Title = "Page1";

			_popDestLabel = new Label
			{
				AutomationId = "PopDestLabel",
				Text = "Not navigated back yet"
			};

			var navButton = new Button
			{
				Text = "Go to Page2",
				AutomationId = "GoToPage2Button",
			};
			navButton.Clicked += async (s, e) => await Navigation.PushAsync(new Issue35613Page2());

			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 10,
				Children =
				{
					navButton,
					_popDestLabel
				}
			};
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs e)
		{
			base.OnNavigatingFrom(e);
			Issue35613.PushNavigatingFromDestination = e.DestinationPage?.Title ?? "null";
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_popDestLabel.Text = Issue35613.PopNavigatingFromDestination;
		}
	}

	public class Issue35613Page2 : ContentPage
	{
		Label _pushDestLabel;

		public Issue35613Page2()
		{
			Title = "Page2";

			_pushDestLabel = new Label
			{
				AutomationId = "Page2CapturedPushDest",
				Text = "Loading..."
			};

			var backButton = new Button
			{
				Text = "Go Back",
				AutomationId = "GoBackButton",
				Command = new Command(async () => await Navigation.PopAsync())
			};

			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 10,
				Children =
				{
					_pushDestLabel,
					backButton
				}
			};
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs e)
		{
			base.OnNavigatingFrom(e);
			Issue35613.PopNavigatingFromDestination = e.DestinationPage?.Title ?? "null";
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_pushDestLabel.Text = Issue35613.PushNavigatingFromDestination;
		}
	}
}
