using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30 : _IssuesUITest
{
	public Issue30(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "Clipped image shadow should render correctly";

	[Test]
	[Category(UITestCategories.Shadow)]
	public void ClippedImageShadowRendersCorrectly()
	{
		App.WaitForElement("ClippedShadowView");
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
