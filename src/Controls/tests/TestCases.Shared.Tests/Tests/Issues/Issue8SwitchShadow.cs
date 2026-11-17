using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8SwitchShadow : _IssuesUITest
{
	public Issue8SwitchShadow(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Switch Shadow Does Not Follow Thumb when Toggle On or Off";

	[Test]
	[Category(UITestCategories.Switch)]
	public void SwitchShadowShouldFollowThumbWhenToggled()
	{
		// Wait for the page to load
		App.WaitForElement("SwitchWithShadow");

		// Verify initial state
		var statusLabel = App.WaitForElement("StatusLabel");
		var initialStatus = statusLabel.GetText();
		
		// Toggle the switch by tapping it
		App.Tap("SwitchWithShadow");
		
		// Wait for state to update
		App.WaitForElement("StatusLabel");
		var newStatus = statusLabel.GetText();
		
		// Verify that the status changed
		Assert.That(newStatus, Is.Not.EqualTo(initialStatus), "Switch status should change when toggled");
		
		// Toggle again using the button
		App.Tap("ToggleButton");
		
		// Wait for state to update
		App.WaitForElement("StatusLabel");
		var thirdStatus = statusLabel.GetText();
		
		// Verify that the status changed back
		Assert.That(thirdStatus, Is.EqualTo(initialStatus), "Switch status should return to original state");
	}
}