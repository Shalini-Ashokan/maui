using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue7150 : _IssuesUITest
{
	public Issue7150(TestDevice device) : base(device) { }

	public override string Issue => "EmptyView using Template displayed at the same time as the content";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CheckCarouselViewEmptyView()
	{
		App.WaitForElement("SearchBar");
		App.EnterText("SearchBar", "xamarin");
		App.PressEnter();
		VerifyScreenshot();
	}
}