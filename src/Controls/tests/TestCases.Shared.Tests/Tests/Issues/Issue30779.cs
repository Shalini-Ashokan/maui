using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30779 : _IssuesUITest
{
    public Issue30779(TestDevice device)
        : base(device)
    {
    }

    public override string Issue => "SearchBar CursorPosition is not updated when the user types";

    [Test]
    [Category(UITestCategories.SearchBar)]
    public void Issue30779CursorPositionShouldUpdate()
    {
        App.WaitForElement("SearchBar");
        App.Tap("SearchBar");
        App.EnterText("SearchBar", "Hello");
        var text = App.FindElement("CursorPositionLabel").GetText();
        Assert.That(text, Is.EqualTo("SearchBar CursorPosition: 5"));
    }
}