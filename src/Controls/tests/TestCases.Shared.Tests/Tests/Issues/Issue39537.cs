using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39537 : _IssuesUITest
{
	public Issue39537(TestDevice device) : base(device) { }

	public override string Issue => "ToolbarItem icons Does not show in Windows";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void Issue39537ToolbarFontIconsAreVisibleWithAndWithoutText()
	{
		App.WaitForElement("Issue39537Ready");
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
