using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

// TODO: Update issue number once the GitHub issue is created
public class Issue00000 : _IssuesUITest
{
    public Issue00000(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "CollectionView ScrollTo does not work with horizontal layout";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void ProgrammaticScrollToWorksWithHorizontalLayout()
    {
        App.WaitForElement("ScrollToButton");
        App.Tap("ScrollToButton");
        var firstIndexText = App.FindElement("IndexLabel").GetText();
        Assert.That(firstIndexText, Is.EqualTo("The CollectionView is scrolled"));
    }
}