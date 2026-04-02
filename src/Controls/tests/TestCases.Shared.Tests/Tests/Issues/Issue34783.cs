using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34783 : _IssuesUITest
{
	public Issue34783(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView dynamic item sizing resets after scrolling on Android";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void DynamicItemSizeIsRetainedAfterScrolling()
	{
		App.WaitForElement("CV_Item_0");
		var initialHeight = App.WaitForElement("CV_Item_0").GetRect().Height;
		App.Tap("CV_Item_0");

		var expandedHeight = App.WaitForElement("CV_Item_0").GetRect().Height;
		Assert.That(expandedHeight, Is.GreaterThan(initialHeight),
			"Tapping the item should increase its height.");

		for (int i = 0; i < 6; i++)
		{
			App.ScrollDown("TestCollectionView", ScrollStrategy.Gesture, 0.9);
		}

		for (int i = 0; i < 6; i++)
		{
			App.ScrollUp("TestCollectionView", ScrollStrategy.Gesture, 0.9);
		}

		App.WaitForElement("CV_Item_0");
		// With the regression: the recycled view is recreated from the template and
		// reverts to the default (smaller) height.
		var heightAfterScroll = App.WaitForElement("CV_Item_0").GetRect().Height;
		Assert.That(heightAfterScroll, Is.GreaterThanOrEqualTo(expandedHeight * 0.9),
			"Item should retain its dynamically-changed size after being scrolled off-screen and back.");
	}
}
