using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ImplicitStyleInheritanceTests : BaseTestFixture
    {
        public ImplicitStyleInheritanceTests()
        {
            ApplicationExtensions.CreateAndSetMockApplication();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Application.ClearCurrent();
            }
            base.Dispose(disposing);
        }

        [Fact]
        public void ImplicitStyleInheritance_PageOverridesApp_ShouldInheritNonOverriddenProperties()
        {
            // This test verifies the main issue: when a page-level implicit style exists,
            // application-level implicit styles should still provide values for properties
            // not set by the page-level style.

            // Application-level implicit style sets both TextColor and BackgroundColor
            var appStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Blue },
                    new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Yellow }
                }
            };

            // Page-level implicit style only sets TextColor
            var pageStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Red }
                }
            };

            var app = new MockApplication();
            app.Resources.Add(appStyle);

            var page = new ContentPage
            {
                Resources = new ResourceDictionary { pageStyle },
                Content = new Label()
            };

            app.LoadPage(page);
            var label = (Label)page.Content;

            // Expected behavior: Page-level style overrides TextColor, but app-level BackgroundColor should be inherited
            Assert.Equal(Colors.Red, label.TextColor); // From page-level style
            Assert.Equal(Colors.Yellow, label.BackgroundColor); // From app-level style (inherited)
        }

        [Fact]
        public void ImplicitStyleInheritance_ThreeLevel_ShouldCascadeCorrectly()
        {
            // Test with three levels: App -> Page -> ContentView
            
            var appStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Blue },
                    new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Yellow },
                    new Setter { Property = Label.FontSizeProperty, Value = 20d }
                }
            };

            var pageStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Red }
                }
            };

            var contentViewStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.FontSizeProperty, Value = 16d }
                }
            };

            var app = new MockApplication();
            app.Resources.Add(appStyle);

            var contentView = new ContentView
            {
                Resources = new ResourceDictionary { contentViewStyle },
                Content = new Label()
            };

            var page = new ContentPage
            {
                Resources = new ResourceDictionary { pageStyle },
                Content = contentView
            };

            app.LoadPage(page);
            var label = (Label)contentView.Content;

            // Expected: ContentView overrides FontSize, Page overrides TextColor, App provides BackgroundColor
            Assert.Equal(Colors.Red, label.TextColor); // From page-level style
            Assert.Equal(Colors.Yellow, label.BackgroundColor); // From app-level style
            Assert.Equal(16d, label.FontSize); // From contentview-level style
        }

        [Fact]
        public void ImplicitStyleInheritance_WithExplicitStyle_ShouldStillInheritImplicit()
        {
            // Test that explicit styles work correctly with implicit style inheritance
            
            var appStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Yellow }
                }
            };

            var pageStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Red }
                }
            };

            var explicitStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.FontSizeProperty, Value = 18d }
                }
            };

            var app = new MockApplication();
            app.Resources.Add(appStyle);

            var page = new ContentPage
            {
                Resources = new ResourceDictionary { pageStyle },
                Content = new Label
                {
                    Style = explicitStyle
                }
            };

            app.LoadPage(page);
            var label = (Label)page.Content;

            // Expected: Explicit style sets FontSize, implicit styles should still provide other properties
            Assert.Equal(Colors.Red, label.TextColor); // From page-level implicit style
            Assert.Equal(Colors.Yellow, label.BackgroundColor); // From app-level implicit style
            Assert.Equal(18d, label.FontSize); // From explicit style
        }

        [Fact]
        public void ImplicitStyleInheritance_NoPageLevel_ShouldUseAppLevel()
        {
            // Test that when there's no page-level style, app-level still works
            
            var appStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Blue },
                    new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Yellow }
                }
            };

            var app = new MockApplication();
            app.Resources.Add(appStyle);

            var page = new ContentPage
            {
                Content = new Label()
            };

            app.LoadPage(page);
            var label = (Label)page.Content;

            // Expected: App-level style should apply both properties
            Assert.Equal(Colors.Blue, label.TextColor);
            Assert.Equal(Colors.Yellow, label.BackgroundColor);
        }
    }
}
