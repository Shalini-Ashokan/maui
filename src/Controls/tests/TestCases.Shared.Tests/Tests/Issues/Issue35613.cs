using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35613 : _IssuesUITest
{
	public Issue35613(TestDevice device) : base(device) { }

	public override string Issue => "OnNavigatingFrom DestinationPage is set to current page instead of actual destination page";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NavigatingFromDestinationPageShouldBeActualDestinationOnPush()
	{
		// Navigate to Page2 — OnNavigatingFrom fires on Page1 with DestinationPage that should be Page2
		App.WaitForElement("GoToPage2Button");
		App.Tap("GoToPage2Button");

		// Page2 reads and displays the DestinationPage.Title captured from Page1.OnNavigatingFrom
		App.WaitForElement("Page2CapturedPushDest");
		var capturedTitle = App.FindElement("Page2CapturedPushDest").GetText();

		// With the bug: capturedTitle == "Page1" (DestinationPage is wrongly set to the source page)
		// With the fix:  capturedTitle == "Page2" (DestinationPage is correctly set to the destination)
		Assert.That(capturedTitle, Is.EqualTo("Page2"),
			$"OnNavigatingFrom.DestinationPage should be 'Page2' (destination) but was '{capturedTitle}'");
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NavigatingFromDestinationPageShouldBeActualDestinationOnPop()
	{
		// Navigate to Page2 first
		App.WaitForElement("GoToPage2Button");
		App.Tap("GoToPage2Button");

		// Navigate back — OnNavigatingFrom fires on Page2 with DestinationPage that should be Page1
		App.WaitForElement("GoBackButton");
		App.Tap("GoBackButton");

		// Page1 reads and displays the DestinationPage.Title captured from Page2.OnNavigatingFrom
		App.WaitForElement("PopDestLabel");
		var capturedTitle = App.FindElement("PopDestLabel").GetText();

		// With the bug: capturedTitle == "Page2" (DestinationPage is wrongly set to the source page)
		// With the fix:  capturedTitle == "Page1" (DestinationPage is correctly set to the destination)
		Assert.That(capturedTitle, Is.EqualTo("Page1"),
			$"OnNavigatingFrom.DestinationPage should be 'Page1' (destination) but was '{capturedTitle}'");
	}
}
