using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

class Issue14577 : _IssuesUITest
{
	public Issue14577(TestDevice device) : base(device) { }

	public override string Issue => "Width or Height x Reference bindings show wrong values on initial render on Windows";

	[Test]
	[Category(UITestCategories.Layout)]
	public void XReferenceWidthBindingCorrectOnInitialRender()
	{
		App.WaitForElement("ReferenceLabel");
		VerifyScreenshot();
	}
}
