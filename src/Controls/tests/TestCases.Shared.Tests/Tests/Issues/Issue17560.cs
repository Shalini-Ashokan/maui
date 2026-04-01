using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17560 : _IssuesUITest
{
	public Issue17560(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[macOS/iOS] Border clipping incorrect when Content is scaled";

	[Test]
	[Category(UITestCategories.Border)]
	public void BorderShouldCorrectlyClipScaledContent()
	{
		App.WaitForElement("PageReadyLabel");
		VerifyScreenshot();
	}
}
