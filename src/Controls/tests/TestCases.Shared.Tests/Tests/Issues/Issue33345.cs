using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33345 : _IssuesUITest
{
    public override string Issue => "FlowDirection MatchParent not resolved in custom layouts";

    public Issue33345(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Layout)]
    public void FlowDirectionMatchParentResolvesInCustomLayout()
    {
        App.WaitForElement("Label1");

        App.Tap("CustomLayout");

        App.WaitForElement("Label2");
        VerifyScreenshot();
    }
}