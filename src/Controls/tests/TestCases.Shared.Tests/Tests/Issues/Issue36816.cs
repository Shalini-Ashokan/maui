using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36816 : _IssuesUITest
{
	public Issue36816(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Android: Clicks being passed through to controls under other Views";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapOnOpaqueOverlayShouldNotClickButtonUnderneath()
	{
		App.WaitForElement("OverlayView");

		// Tap on the green overlay that fully covers the Button beneath it.
		// The tap should be consumed by the overlay and must NOT reach the
		// Button, so the label text must remain unchanged.
		App.Tap("OverlayView");

		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(resultText, Is.EqualTo("Not Clicked"),
			"Tapping the opaque overlay view should not pass the click through to the Button underneath it.");
	}
}
