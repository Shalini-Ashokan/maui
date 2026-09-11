using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class LineReferenceBinding : _IssuesUITest
    {
        public LineReferenceBinding(TestDevice device) : base(device)
        {
        }

        public override string Issue => "Line X2 binding to Label width doesn't work on iOS";

        [Test]
        [Category(UITestCategories.Shape)]
        [Category(UITestCategories.Compatibility)]
        public void LineX2ShouldUpdateWhenLabelWidthChanges()
        {
            // Wait for page to load
            App.WaitForElement("StatusLabel");

            // Wait for initial layout
            App.WaitForElement("LabelWidthLabel");
            App.WaitForElement("LineX2Label");

            // Get initial values
            var initialStatus = App.FindElement("StatusLabel").GetText();
            Assert.That(initialStatus, Is.EqualTo("Status: Initial state"));

            // Change the label width by clicking the button
            App.Tap("ChangeWidthBtn");

            // Wait for UI update
            App.WaitForElement("StatusLabel");
            var newStatus = App.FindElement("StatusLabel").GetText();
            Assert.That(newStatus, Is.EqualTo("Status: Changed to Short"));

            // The Line X2 should now match the Label width
            // Get the label width and line X2 values
            var labelWidthText = App.FindElement("LabelWidthLabel").GetText();
            var lineX2Text = App.FindElement("LineX2Label").GetText();

            // Parse the values (format: "Label Width: 123.4")
            var labelWidthValue = ExtractNumericValue(labelWidthText);
            var lineX2Value = ExtractNumericValue(lineX2Text);

            // On iOS, the bug manifests as the Line X2 staying at 0 or 1
            // while the Label width changes
            Assert.That(lineX2Value, Is.GreaterThan(0), "Line X2 should be greater than 0");
            Assert.That(lineX2Value, Is.EqualTo(labelWidthValue).Within(1), 
                $"Line X2 ({lineX2Value}) should match Label width ({labelWidthValue})");

            // Test another change
            App.Tap("ChangeWidthBtn");
            App.WaitForElement("StatusLabel");
            var mediumStatus = App.FindElement("StatusLabel").GetText();
            Assert.That(mediumStatus, Is.EqualTo("Status: Changed to Medium"));

            // Check values again
            var mediumLabelWidthText = App.FindElement("LabelWidthLabel").GetText();
            var mediumLineX2Text = App.FindElement("LineX2Label").GetText();
            
            var mediumLabelWidthValue = ExtractNumericValue(mediumLabelWidthText);
            var mediumLineX2Value = ExtractNumericValue(mediumLineX2Text);
            
            Assert.That(mediumLineX2Value, Is.GreaterThan(0), "Line X2 should be greater than 0 after second change");
            Assert.That(mediumLineX2Value, Is.EqualTo(mediumLabelWidthValue).Within(1), 
                $"Line X2 ({mediumLineX2Value}) should match Label width ({mediumLabelWidthValue}) after second change");
        }

        private double ExtractNumericValue(string text)
        {
            // Extract numeric value from text like "Label Width: 123.4"
            var colonIndex = text.IndexOf(':');
            if (colonIndex >= 0 && colonIndex < text.Length - 1)
            {
                var valueStr = text.Substring(colonIndex + 1).Trim();
                if (double.TryParse(valueStr, out double value))
                {
                    return value;
                }
            }
            return 0;
        }
    }
}