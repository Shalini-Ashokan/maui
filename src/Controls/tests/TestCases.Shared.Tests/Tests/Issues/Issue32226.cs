#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32226 : _IssuesUITest
{
	public override string Issue => "TapGestureRecognizer Tapped not called when Android Touch handler has e.Handled = false";

	public Issue32226(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapGestureRecognizerFiredWhenTouchHandlerHasHandledFalse()
	{
		App.WaitForElement("TapLabel");

		// Verify initial state
		Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Waiting"));

		// Tap the label — TapGestureRecognizer should fire even with a Touch handler that sets e.Handled = false
		App.Tap("TapLabel");

		// Verify the tap was received
		Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Tapped"),
			"TapGestureRecognizer.Tapped should fire even when an Android Touch handler sets e.Handled = false");
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapGestureRecognizerFiredViaAccessibilityAction()
	{
		App.WaitForElement("TapLabel");

		// Verify initial state
		Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Waiting"));

		// Simulate TalkBack activation: calls PerformClick() on the platform view,
		// which is the code path TalkBack ultimately takes via ACTION_CLICK.
		// With our fix, the OnClickListener forwards the click to TapGestureRecognizer.
		App.Tap("AccessibilityTapButton");

		// Verify the tap was received via the TalkBack / click listener path
		Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Tapped"),
			"TapGestureRecognizer.Tapped should fire via the TalkBack / performClick() path");
	}
}
#endif
