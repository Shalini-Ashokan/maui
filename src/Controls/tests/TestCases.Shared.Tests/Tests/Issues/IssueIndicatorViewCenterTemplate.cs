using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueIndicatorViewCenterTemplate : _IssuesUITest
	{
		public override string Issue => "IndicatorView with custom template should be centered";

		public IssueIndicatorViewCenterTemplate(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.IndicatorView)]
		public void IndicatorViewWithCustomTemplateShouldBeCentered()
		{
			// Test 1: Default (no HorizontalOptions) should be centered
			App.WaitForElement("DefaultIndicator");
			var defaultIndicator = App.FindElement("DefaultIndicator");
			var defaultRect = defaultIndicator.GetRect();
			
			// Get the parent container width (approximate screen width or parent width)
			var screenWidth = App.GetDeviceInfo().Width;
			
			// Calculate expected center position (allowing some tolerance)
			var expectedCenterX = screenWidth / 2;
			var actualCenterX = defaultRect.X + (defaultRect.Width / 2);
			
			// Assert that the indicator is approximately centered (within 50 pixels tolerance)
			Assert.That(actualCenterX, Is.EqualTo(expectedCenterX).Within(50),
				$"Default indicator should be centered. Expected X: {expectedCenterX}, Actual: {actualCenterX}");

			// Test 2: Explicit Center should be centered
			var centerIndicator = App.FindElement("CenterIndicator");
			var centerRect = centerIndicator.GetRect();
			var centerActualX = centerRect.X + (centerRect.Width / 2);
			
			Assert.That(centerActualX, Is.EqualTo(expectedCenterX).Within(50),
				$"Center indicator should be centered. Expected X: {expectedCenterX}, Actual: {centerActualX}");

			// Test 3: Start should be at the start (left side)
			var startIndicator = App.FindElement("StartIndicator");
			var startRect = startIndicator.GetRect();
			
			Assert.That(startRect.X, Is.LessThan(100),
				$"Start indicator should be at the start. Actual X: {startRect.X}");

			// Test 4: RTL + Center should still be centered
			var rtlIndicator = App.FindElement("RTLCenterIndicator");
			var rtlRect = rtlIndicator.GetRect();
			var rtlActualX = rtlRect.X + (rtlRect.Width / 2);
			
			Assert.That(rtlActualX, Is.EqualTo(expectedCenterX).Within(50),
				$"RTL Center indicator should be centered. Expected X: {expectedCenterX}, Actual: {rtlActualX}");

			// Test 5: Label template should be centered
			var labelIndicator = App.FindElement("LabelTemplateIndicator");
			var labelRect = labelIndicator.GetRect();
			var labelActualX = labelRect.X + (labelRect.Width / 2);
			
			Assert.That(labelActualX, Is.EqualTo(expectedCenterX).Within(50),
				$"Label template indicator should be centered. Expected X: {expectedCenterX}, Actual: {labelActualX}");
		}
	}
}
