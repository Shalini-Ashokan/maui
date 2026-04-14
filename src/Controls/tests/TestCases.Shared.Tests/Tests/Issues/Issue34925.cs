using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34925 : _IssuesUITest
{
	public Issue34925(TestDevice device) : base(device) { }

	public override string Issue => "Shell flyout items scroll behind FlyoutFooter on iOS";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutItemsDoNotScrollBehindFlyoutFooter()
	{
		App.WaitForElement("Issue34925Page1Label");
		App.TapShellFlyoutIcon();

		App.WaitForElement("Issue34925FlyoutFooter");
		var footerRect = App.WaitForElement("Issue34925FlyoutFooter").GetRect();
		float footerTop = footerRect.Y;

		App.ScrollDown("Page 1", ScrollStrategy.Gesture, 0.9);

		for (int i = 18; i <= 20; i++)
		{
			var items = App.FindElements($"Page {i}");
			if (items.Count > 0)
			{
				var itemRect = items.First().GetRect();
				Assert.That(
					(double)(itemRect.Y + itemRect.Height),
					Is.LessThanOrEqualTo((double)footerTop + 2),
					$"'Page {i}' bottom edge (Y={itemRect.Y + itemRect.Height}) extends behind the footer top " +
					$"({footerTop}), indicating flyout items are scrolling behind the footer.");
			}
		}
	}
}
