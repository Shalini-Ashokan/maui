using System;
using System.ComponentModel;
using Xunit;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class LineXReferenceBindingTests : BaseTestFixture
    {
        [Fact]
        public void LineX2UpdatesWhenReferencedElementWidthChanges()
        {
            // Arrange
            var label = new Label
            {
                Text = "Test Label",
                WidthRequest = 100
            };
            
            var line = new Line();
            
            // Simulate x:Reference binding by setting up the binding
            var binding = new Binding("Width", source: label);
            line.SetBinding(Line.X2Property, binding);
            
            // Initial state
            Assert.Equal(0, line.X2); // Initial value should be 0
            
            // Act - trigger layout to set width
            label.BatchBegin();
            label.Width = 150; // This simulates the layout system setting the width
            label.BatchCommit();
            
            // Assert
            Assert.Equal(150, line.X2);
        }
        
        [Fact]
        public void LinePropertyChangedTriggersOnX2Change()
        {
            // Arrange
            var line = new Line();
            string changedPropertyName = null;
            
            line.PropertyChanged += (sender, e) =>
            {
                changedPropertyName = e.PropertyName;
            };
            
            // Act
            line.X2 = 100;
            
            // Assert
            Assert.Equal(nameof(Line.X2), changedPropertyName);
        }
        
        [Fact]
        public void LineInvalidatesShapeOnPropertyChange()
        {
            // This test verifies that the Line properly invalidates when properties change
            // which is important for the iOS fix
            
            // Arrange
            var line = new Line();
            var shapeUpdateCalled = false;
            
            // Mock the handler to track when UpdateValue is called
            var mockHandler = new MockShapeHandler(() => shapeUpdateCalled = true);
            line.Handler = mockHandler;
            
            // Act
            line.X2 = 200;
            
            // Assert
            Assert.True(shapeUpdateCalled, "Shape should be invalidated when X2 property changes");
        }
        
        [Fact]
        public void LineBindingWorksWithMultipleProperties()
        {
            // Arrange
            var sourceElement = new MockElement();
            var line = new Line();
            
            // Set up bindings for multiple properties
            line.SetBinding(Line.X1Property, new Binding("X", source: sourceElement));
            line.SetBinding(Line.Y1Property, new Binding("Y", source: sourceElement));
            line.SetBinding(Line.X2Property, new Binding("Width", source: sourceElement));
            line.SetBinding(Line.Y2Property, new Binding("Height", source: sourceElement));
            
            // Act
            sourceElement.X = 10;
            sourceElement.Y = 20;
            sourceElement.Width = 100;
            sourceElement.Height = 50;
            
            // Assert
            Assert.Equal(10, line.X1);
            Assert.Equal(20, line.Y1);
            Assert.Equal(100, line.X2);
            Assert.Equal(50, line.Y2);
        }
        
        private class MockElement : BindableObject
        {
            public static readonly BindableProperty XProperty = BindableProperty.Create(
                nameof(X), typeof(double), typeof(MockElement), 0.0);
            
            public static readonly BindableProperty YProperty = BindableProperty.Create(
                nameof(Y), typeof(double), typeof(MockElement), 0.0);
            
            public static readonly BindableProperty WidthProperty = BindableProperty.Create(
                nameof(Width), typeof(double), typeof(MockElement), 0.0);
            
            public static readonly BindableProperty HeightProperty = BindableProperty.Create(
                nameof(Height), typeof(double), typeof(MockElement), 0.0);
            
            public double X
            {
                get => (double)GetValue(XProperty);
                set => SetValue(XProperty, value);
            }
            
            public double Y
            {
                get => (double)GetValue(YProperty);
                set => SetValue(YProperty, value);
            }
            
            public double Width
            {
                get => (double)GetValue(WidthProperty);
                set => SetValue(WidthProperty, value);
            }
            
            public double Height
            {
                get => (double)GetValue(HeightProperty);
                set => SetValue(HeightProperty, value);
            }
        }
        
        private class MockShapeHandler : IElementHandler
        {
            private readonly Action _onUpdateValue;
            
            public MockShapeHandler(Action onUpdateValue)
            {
                _onUpdateValue = onUpdateValue;
            }
            
            public object PlatformView => null;
            public IElement VirtualView { get; set; }
            public IMauiContext MauiContext { get; set; }
            
            public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;
            public void SetVirtualView(IElement view) => VirtualView = view;
            public void UpdateValue(string property) => _onUpdateValue?.Invoke();
            public void Invoke(string command, object args) { }
            public void DisconnectHandler() { }
        }
    }
}