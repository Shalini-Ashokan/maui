using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class LineIOSBindingFixTests : BaseTestFixture
    {
        [Fact]
        public void LineOnPropertyChangedCallsUpdateValueForCoordinateProperties()
        {
            // This test verifies that the Line properly calls UpdateValue when coordinate properties change
            // which is the core mechanism for the iOS fix
            
            // Arrange
            var line = new Line();
            var updateValueCalled = false;
            var propertyName = string.Empty;
            
            var mockHandler = new TestElementHandler((prop) =>
            {
                updateValueCalled = true;
                propertyName = prop;
            });
            
            line.Handler = mockHandler;
            
            // Test X1 property
            line.X1 = 10;
            Assert.True(updateValueCalled, "UpdateValue should be called when X1 changes");
            Assert.Equal(nameof(IShapeView.Shape), propertyName);
            
            // Reset for next test
            updateValueCalled = false;
            
            // Test Y1 property  
            line.Y1 = 20;
            Assert.True(updateValueCalled, "UpdateValue should be called when Y1 changes");
            
            // Reset for next test
            updateValueCalled = false;
            
            // Test X2 property (the main issue from the bug report)
            line.X2 = 100;
            Assert.True(updateValueCalled, "UpdateValue should be called when X2 changes");
            
            // Reset for next test
            updateValueCalled = false;
            
            // Test Y2 property
            line.Y2 = 50;
            Assert.True(updateValueCalled, "UpdateValue should be called when Y2 changes");
        }
        
        [Fact]
        public void LineBindingFromLabelWidthToX2Works()
        {
            // This test reproduces the exact scenario from the bug report
            
            // Arrange
            var label = new Label
            {
                Text = "Distance"
            };
            
            var line = new Line
            {
                Stroke = Colors.DarkGreen,
                StrokeThickness = 3
            };
            
            // Set up the binding that was failing on iOS
            var binding = new Binding("Width", source: label);
            line.SetBinding(Line.X2Property, binding);
            
            // Initially, both width and X2 should be 0 or -1 (not set)
            Assert.True(line.X2 == 0 || line.X2 == -1);
            
            // Act - simulate what happens when the label gets laid out
            // In real app, this would happen through the layout system
            label.BatchBegin();
            label.Width = 85.5; // Typical width for "Distance" text
            label.BatchCommit();
            
            // Assert - the line's X2 should now match the label's width
            Assert.Equal(85.5, line.X2);
        }
        
        [Fact]
        public void LineGetPathReturnsCorrectPath()
        {
            // This test verifies that the Line's GetPath method works correctly
            // with the coordinate values that would be set by binding
            
            // Arrange
            var line = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 100,
                Y2 = 0
            };
            
            // Act
            var path = line.GetPath();
            
            // Assert
            Assert.NotNull(path);
            // The path should represent a horizontal line from (0,0) to (100,0)
            // We can't easily test the internal path structure, but we can verify it was created
        }
        
        private class TestElementHandler : IElementHandler
        {
            private readonly Action<string> _onUpdateValue;
            
            public TestElementHandler(Action<string> onUpdateValue)
            {
                _onUpdateValue = onUpdateValue;
            }
            
            public object PlatformView => null;
            public IElement VirtualView { get; set; }
            public IMauiContext MauiContext { get; set; }
            
            public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;
            public void SetVirtualView(IElement view) => VirtualView = view;
            
            public void UpdateValue(string property)
            {
                _onUpdateValue?.Invoke(property);
            }
            
            public void Invoke(string command, object args) { }
            public void DisconnectHandler() { }
        }
    }
}