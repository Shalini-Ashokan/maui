#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32506 : _IssuesUITest
{
	public override string Issue => "[iOS 26] Liquid glass hamburger and toolbar icon flashing when Shell BackgroundColor is not white";

	public Issue32506(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBackgroundIsRedAfterNavigatingBetweenTabs()
	{
		// Verify initial page A loaded with toolbar item visible
		App.WaitForElement("PageALabel");

		// Navigate to tab B
		App.TapTab("B");
		App.WaitForElement("PageBLabel");

		// Navigate back to tab A
		App.TapTab("A");
		App.WaitForElement("PageALabel");

		// Verify shell background is red (no white flash from liquid glass persists)
		VerifyScreenshot();
	}
}
#endif
