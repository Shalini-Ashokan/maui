using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21068 : _IssuesUITest
{
    public Issue21068(TestDevice device) : base(device)
    {
    }

    public override string Issue => "[Android] IView with initial Rotation has incorrect offsets of PivotX and PivotY";

    [Test]
    [Category(UITestCategories.Image)]
    public void InitialBoundRotationShouldRotateAroundCenter()
    {
        App.WaitForElement("TestBox");
        VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Image)]
    public void ChangingRotationViaBindingShouldRotateCorrectly()
    {
        App.WaitForElement("TestBox");
        App.Tap("ChangeRotationButton");
        VerifyScreenshot();
    }
}
