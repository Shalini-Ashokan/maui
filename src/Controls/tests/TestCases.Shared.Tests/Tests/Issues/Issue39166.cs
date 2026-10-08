using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39166 : _IssuesUITest
{
	public Issue39166(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "SwipeItem tap does not work with Image WidthRequest";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void Issue39166SwipeItemTapDoesWorkWithImageWidthRequest()
	{
		App.WaitForElement("TestSwipeView");
		App.SwipeLeftToRight("TestSwipeView");
		App.WaitForElement("Delete");
		App.Tap("Delete");
		App.WaitForElement("StatusLabel");
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Delete action invoked!"));

	}
}
