#if ANDROID // The issue is specific to Android, so restricting other platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24812 : _IssuesUITest
{
    public override string Issue => "WebViewHandler OnProgressChanged not called on Android";

    public Issue24812(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.WebView)]
    public void WebViewProgressCallbacksShouldFire()
    {
        App.WaitForElement("StatusLabel");
        var statusText = App.FindElement("StatusLabel").GetText();
        Assert.That(statusText, Is.EqualTo("Progress: 100"));
    }
}
#endif