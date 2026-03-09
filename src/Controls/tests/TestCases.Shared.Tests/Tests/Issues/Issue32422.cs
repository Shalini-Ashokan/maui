using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32422 : _IssuesUITest
{
	public Issue32422(TestDevice device) : base(device) { }

	public override string Issue => "WebView Height Continuously Increases when webview inside Border layout";

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewHeightShouldNotGrowInsideBorderGridLayout()
	{
		// Wait for the page to load and the WebView to render
		App.WaitForElement("HeightLabel");
		VerifyScreenshot();
	}
}
