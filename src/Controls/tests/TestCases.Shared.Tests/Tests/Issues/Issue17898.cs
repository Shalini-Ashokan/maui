using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17898 : _IssuesUITest
{
	public override string Issue => "AppThemeBinding interfering with VisualState setters for FlyoutItems";

	public Issue17898(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutSelectedItemShouldApplySelectedVisualState()
	{
		// Wait for the initial page to load (Page One is selected by default)
		App.WaitForElement("Issue17898Label");

		// Open the flyout
		App.TapShellFlyoutIcon();

		// Take a screenshot of the flyout open state.
		// Due to bug #17898, the selected item (Page One) appears gray (#E0E0E0)
		// instead of the expected red (#FF0000) defined in the Selected VisualState.
		// AppThemeBinding in the Normal state re-fires asynchronously and overwrites
		// the Selected state, so both items look identical (gray).
		// This screenshot comparison will FAIL because the selected item is gray, not red.
		VerifyScreenshot();
	}
}
