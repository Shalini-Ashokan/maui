using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12345 : _IssuesUITest
	{
		public Issue12345(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Control backgrounds remain visible after being set to null";

		[Test]
		[Category(UITestCategories.Label)]
		public void ControlBackgroundsClearWhenSetToNull()
		{
			App.WaitForElement("SetBackgroundButton");
			App.Tap("SetBackgroundButton");
			VerifyScreenshot();
		}
	}
}
