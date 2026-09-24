using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue30007 : _IssuesUITest
    {
        public Issue30007(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "Line Drawing Stroke Extension Issue on Windows";

        [Test]
        [Category(UITestCategories.GraphicsView)]
        public void VerifyLineDrawingWithoutStrokeExtension()
        {
            App.WaitForElement("LineTestGraphicsView");
            
            // Test with default stroke width (1)
            VerifyScreenshot();
            
            // Test with increased stroke width
            App.Click("IncreaseStrokeButton");
            App.WaitForElement("StrokeWidthLabel");
            VerifyScreenshot();
            
            App.Click("IncreaseStrokeButton");
            VerifyScreenshot();
            
            App.Click("IncreaseStrokeButton");
            VerifyScreenshot();
        }
    }
}