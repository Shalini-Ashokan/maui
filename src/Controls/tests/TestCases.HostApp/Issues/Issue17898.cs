namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17898, "AppThemeBinding interfering with VisualState setters for FlyoutItems", PlatformAffected.Android)]
public class Issue17898 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		// This style reproduces the bug: using AppThemeBinding in the Normal VisualState setter
		// causes the Selected state to never apply (both items appear identical/gray).
		var normalLight = Color.FromArgb("#E0E0E0");
		var normalDark = Color.FromArgb("#404040");
		var selectedRed = Color.FromArgb("#FF0000");

		var normalState = new VisualState { Name = "Normal" };
		normalState.Setters.Add(new Setter
		{
			Property = VisualElement.BackgroundProperty,
			Value = new AppThemeBinding { Light = normalLight, Dark = normalDark }
		});

		var selectedState = new VisualState { Name = "Selected" };
		selectedState.Setters.Add(new Setter
		{
			Property = VisualElement.BackgroundProperty,
			Value = selectedRed
		});

		var commonGroup = new VisualStateGroup { Name = "CommonStates" };
		commonGroup.States.Add(normalState);
		commonGroup.States.Add(selectedState);

		var groups = new VisualStateGroupList();
		groups.Add(commonGroup);

		var flyoutLayoutStyle = new Style(typeof(Layout))
		{
			Class = "FlyoutItemLayoutStyle",
			ApplyToDerivedTypes = true
		};
		flyoutLayoutStyle.Setters.Add(new Setter
		{
			Property = VisualStateManager.VisualStateGroupsProperty,
			Value = groups
		});

		Resources.Add(flyoutLayoutStyle);

		var page1 = new ContentPage { Title = "Page One" };
		page1.Content = new Label
		{
			Text = "Page One",
			AutomationId = "Issue17898Label",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		AddFlyoutItem(page1, "Page One");

		var page2 = new ContentPage { Title = "Page Two" };
		page2.Content = new Label
		{
			Text = "Page Two",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		AddFlyoutItem(page2, "Page Two");
	}
}
