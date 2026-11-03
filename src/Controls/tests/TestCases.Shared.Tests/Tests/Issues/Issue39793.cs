#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39793 : _IssuesUITest
{
	public Issue39793(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Native title bar should not display a second MAUI title bar";

	[Test]
	[Category(UITestCategories.Window)]
	public void NativeTitleBarShouldNotShowDuplicateMauiTitleBar()
	{
		App.WaitForElement("Issue39793Content");
		VerifyScreenshot(includeTitleBar: true, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
