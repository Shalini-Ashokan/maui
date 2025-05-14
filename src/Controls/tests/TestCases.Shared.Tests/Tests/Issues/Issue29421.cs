using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29421 : _IssuesUITest
{
	public override string Issue => "KeepScrollOffset Not Working as Expected in CarouselView";

	public Issue29421(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewKeepScrollOffsetAdd()
	{
		App.WaitForElement("carouselview");
		App.Tap("AddButton");
		var text = App.FindElement("positionLabel").GetText();
		Assert.That(text, Is.EqualTo("Current Item: Item 1, Position: 2"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewKeepScrollOffsetRemove()
	{
		App.WaitForElement("carouselview");
		App.Tap("RemoveButton");
		var text = App.FindElement("positionLabel").GetText();
		Assert.That(text, Is.EqualTo("Current Item: Item 1, Position: 1"));
	}
}