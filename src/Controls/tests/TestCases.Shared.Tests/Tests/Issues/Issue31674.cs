using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31674 : _IssuesUITest
{
	public override string Issue => "[iOS,Mac] Label with TextType Html is measured as height 0";

	public Issue31674(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Label)]
	public void HtmlLabelInsideCustomLayoutShouldHaveNonZeroHeight()
	{
		App.WaitForElement("PageLoaded");
		VerifyScreenshot();
	}
}
