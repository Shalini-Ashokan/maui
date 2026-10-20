using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31141 : _IssuesUITest
{
	public Issue31141(TestDevice device) : base(device) { }

	public override string Issue => "IndicatorView not visible when applying FlowDirection RightToLeft with IndicatorTemplate";

	[Test]
	[Category(UITestCategories.IndicatorView)]
	public void IndicatorViewShouldBeVisibleWithRTLFlowDirectionAndIndicatorTemplate()
	{
		App.WaitForElement("Issue31141Label");
		VerifyScreenshot();
	}
}
