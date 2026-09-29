#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38949 : _IssuesUITest
{
    public override string Issue => "Windows TitleBar handler context when navigating to modal pages";

    public Issue38949(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Navigation)]
    public void TitleBarDoesNotThrowWhenPushingModalPage()
    {
        App.WaitForElement("TitleBarPushModal");

        App.Tap("TitleBarPushModal");

        // If #38949 is present, the TitleBar handler/context
        // mismatch can throw during this navigation.
        App.WaitForElement("TitleBarPopModal");

        App.Tap("TitleBarPopModal");

        App.WaitForElement("TitleBarPushModal");
    }
}
#endif
