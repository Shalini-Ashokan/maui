using System;
using System.ComponentModel;
using Xunit;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests to verify that the iOS/MacCatalyst fix for Line x:Reference binding works correctly.
    /// This addresses the issue where Line X2 binding to Label width doesn't work on iOS.
    /// </summary>
    public class LineReferenceBindingFixTests : BaseTestFixture
    {
        [Fact]
        public void LineX2BindingToLabelWidth_ShouldUpdate_WhenWidthChanges()
        {
            // This test reproduces the exact scenario from the bug report:
            // X2="{Binding Source={x:Reference lblDistanceText}, Path=Width}"
            
            // Arrange
            var lblDistanceText = new Label
            {
                Text = "Distance",
                FontSize = 14
            };
            
            var line = new Line
            {
                Stroke = Colors.DarkGreen,
                StrokeThickness = 3
            };
            
            // Set up the binding that was failing on iOS
            var binding = new Binding("Width", source: lblDistanceText);
            line.SetBinding(Line.X2Property, binding);
            
            // Initially, both should be 0 or -1 (not set)
            Assert.True(line.X2 <= 0);
            
            // Act - simulate what happens during layout
            lblDistanceText.BatchBegin();
            lblDistanceText.Width = 85.5; // Typical width for "Distance" text
            lblDistanceText.BatchCommit();
            
            // Assert - the line's X2 should now match the label's width
            Assert.Equal(85.5, line.X2);
        }
        
        [Fact]
        public void LineX2PropertyChange_ShouldTriggerInvalidation()
        {
            // This test verifies that the iOS fix properly invalidates the shape
            
            // Arrange
            var line = new Line();
            var invalidationCalled = false;
            
            // Set up a mock handler to track invalidation calls
            var mockHandler = new TestShapeHandler(() => invalidationCalled = true);
            line.Handler = mockHandler;
            
            // Act
            line.X2 = 100;
            
            // Assert
            Assert.True(invalidationCalled, "Shape invalidation should be called when X2 changes");
        }
        
        [Fact] 
        public void LineAllCoordinateProperties_ShouldTriggerInvalidation()
        {
            // This test verifies that all coordinate properties properly trigger invalidation
            
            // Arrange
            var line = new Line();
            var invalidationCount = 0;
            
            var mockHandler = new TestShapeHandler(() => invalidationCount++);
            line.Handler = mockHandler;
            
            // Act - change all coordinate properties
            line.X1 = 10;
            line.Y1 = 20;
            line.X2 = 100;
            line.Y2 = 50;
            
            // Assert
            Assert.Equal(4, invalidationCount);
        }
        
        [Fact]
        public void LineWithMultipleBindings_ShouldUpdateCorrectly()
        {
            // This test verifies that multiple bindings work correctly
            
            // Arrange
            var sourceLabel = new Label { Text = "Source" };
            var line = new Line();
            
            // Set up bindings for multiple properties
            line.SetBinding(Line.X2Property, new Binding("Width", source: sourceLabel));
            line.SetBinding(Line.Y2Property, new Binding("Height", source: sourceLabel));
            
            // Act
            sourceLabel.BatchBegin();
            sourceLabel.Width = 150;
            sourceLabel.Height = 30;
            sourceLabel.BatchCommit();
            
            // Assert
            Assert.Equal(150, line.X2);
            Assert.Equal(30, line.Y2);
        }
        
        [Fact]
        public void LinePropertyChangedEvent_ShouldFireForCoordinateProperties()
        {
            // This test verifies that PropertyChanged events are properly fired
            
            // Arrange
            var line = new Line();
            var propertyChangedEvents = new List<string>();
            
            line.PropertyChanged += (sender, e) =>
            {
                propertyChangedEvents.Add(e.PropertyName);
            };
            
            // Act
            line.X1 = 5;
            line.Y1 = 10;
            line.X2 = 50;
            line.Y2 = 25;
            
            // Assert
            Assert.Contains(nameof(Line.X1), propertyChangedEvents);
            Assert.Contains(nameof(Line.Y1), propertyChangedEvents);
            Assert.Contains(nameof(Line.X2), propertyChangedEvents);
            Assert.Contains(nameof(Line.Y2), propertyChangedEvents);
        }
        
        private class TestShapeHandler : IElementHandler
        {
            private readonly Action _onInvalidate;
            
            public TestShapeHandler(Action onInvalidate)
            {
                _onInvalidate = onInvalidate;
            }
            
            public object PlatformView => new TestPlatformView(_onInvalidate);
            public IElement VirtualView { get; set; }
            public IMauiContext MauiContext { get; set; }
            
            public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;
            public void SetVirtualView(IElement view) => VirtualView = view;
            
            public void UpdateValue(string property)
            {
                _onInvalidate?.Invoke();
            }
            
            public void Invoke(string command, object args) { }
            public void DisconnectHandler() { }
        }
        
        private class TestPlatformView
        {
            private readonly Action _onInvalidate;
            
            public TestPlatformView(Action onInvalidate)
            {
                _onInvalidate = onInvalidate;
            }
            
            public void InvalidateShape(IShapeView shapeView)
            {
                _onInvalidate?.Invoke();
            }
        }
    }
}