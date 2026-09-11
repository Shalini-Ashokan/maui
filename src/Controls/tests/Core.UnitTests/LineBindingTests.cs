using Xunit;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class LineBindingTests : BaseTestFixture
    {
        [Fact]
        public void LineX2ShouldUpdateWhenBoundPropertyChanges()
        {
            // Arrange
            var label = new Label { Text = "Test" };
            var line = new Line();
            
            // Set up binding
            line.SetBinding(Line.X2Property, new Binding("Width", source: label));
            
            // Act - simulate width change
            label.Width = 100;
            
            // Assert
            Assert.Equal(100, line.X2);
        }
        
        [Fact]
        public void LineX2ShouldTriggerPropertyChangedOnUpdate()
        {
            // Arrange
            var line = new Line();
            bool propertyChanged = false;
            
            line.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == Line.X2Property.PropertyName)
                {
                    propertyChanged = true;
                }
            };
            
            // Act
            line.X2 = 50;
            
            // Assert
            Assert.True(propertyChanged);
        }
        
        [Fact]
        public void LineHandlerShouldBeCalledWhenX2Changes()
        {
            // Arrange
            var line = new Line();
            bool handlerCalled = false;
            
            // Mock handler
            line.Handler = new MockLineHandler(() => handlerCalled = true);
            
            // Act
            line.X2 = 75;
            
            // Assert
            Assert.True(handlerCalled);
        }
        
        private class MockLineHandler : Handlers.LineHandler
        {
            private readonly Action _onUpdateValue;
            
            public MockLineHandler(Action onUpdateValue)
            {
                _onUpdateValue = onUpdateValue;
            }
            
            public override void UpdateValue(string property)
            {
                _onUpdateValue?.Invoke();
                base.UpdateValue(property);
            }
        }
    }
}