using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace Maui.Controls.Sample.Tests
{
    public class ImplicitStyleInheritanceTest
    {
        public void TestImplicitStyleInheritance()
        {
            // Create Application-level implicit style
            var appStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Red },
                    new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Yellow }
                }
            };

            var application = new Application();
            application.Resources = new ResourceDictionary { appStyle };

            // Create Page-level implicit style that only sets TextColor
            var pageStyle = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Green }
                }
            };

            var page = new ContentPage();
            page.Resources = new ResourceDictionary { pageStyle };

            // Create Label without explicit styling
            var label = new Label { Text = "Test Label" };
            page.Content = label;

            Application.Current = application;
            application.LoadPage(page);

            // Expected behavior:
            // - TextColor should be Green (from page-level style)  
            // - BackgroundColor should be Yellow (from application-level style)
            Console.WriteLine($"TextColor: {label.TextColor}"); // Should be Green
            Console.WriteLine($"BackgroundColor: {label.BackgroundColor}"); // Should be Yellow

            // Verify the fix
            bool isFixed = label.TextColor == Colors.Green && label.BackgroundColor == Colors.Yellow;
            Console.WriteLine($"Fix working: {isFixed}");
        }
    }
}
