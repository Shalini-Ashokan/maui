#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17523 : _IssuesUITest
{
	public Issue17523(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Border clip does not constrain scaled content on Windows";

	[Test]
	[Category(UITestCategories.Border)]
	public void UnscaledImageIsClippedToRoundRectangleBorder()
	{
		App.WaitForElement("RoundRectangleImage");

		// Baseline: unscaled image should already be clipped to the Border's rounded-rectangle shape.
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void ScaledImageStaysClippedToRoundRectangleBorder()
	{
		App.WaitForElement("RoundRectangleImage");

		App.Tap("IncreaseScaleButton");
		App.WaitForTextToBePresentInElement("ScaleLabel", "1.5");

		// Scaled: the image must still be clipped to the Border - it should not spill outside
		// the rounded-rectangle's edges.
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void UnscaledImageIsClippedToEllipseBorder()
	{
		App.WaitForElement("EllipseImage");

		// Baseline: unscaled image should already be clipped to the Border's ellipse shape.
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void ScaledImageStaysClippedToEllipseBorder()
	{
		App.WaitForElement("EllipseImage");

		App.Tap("IncreaseScaleButton");
		App.WaitForTextToBePresentInElement("ScaleLabel", "1.5");

		// Scaled: the image must still be clipped to the Border - it should not spill outside
		// the ellipse's edges.
		VerifyScreenshot();
	}
}
#endif
