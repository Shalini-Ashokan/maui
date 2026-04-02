using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34783 : _IssuesUITest
{
	public Issue34783(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView dynamic item sizing resets after scrolling on Android";

	// Regression introduced by PR #34534, which added `_selectedTemplate = null` in
	// TemplatedItemViewHolder.Recycle(). That forced every re-bind after a view is
	// recycled to recreate the view from the DataTemplate, discarding any runtime
	// HeightRequest changes the user had made on the item.
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void DynamicItemSizeIsRetainedAfterScrolling()
	{
		// Wait for the first item to be visible.
		App.WaitForElement("CV_Item_0");

		// Record the initial (default) height.
		var initialHeight = App.WaitForElement("CV_Item_0").GetRect().Height;

		// Tap the first item to expand it (HeightRequest 60 → 120).
		App.Tap("CV_Item_0");

		// The item should now be taller.
		var expandedHeight = App.WaitForElement("CV_Item_0").GetRect().Height;
		Assert.That(expandedHeight, Is.GreaterThan(initialHeight),
			"Tapping the item should increase its height.");

		// Scroll down far enough so that the first item scrolls off-screen and is recycled.
		for (int i = 0; i < 6; i++)
		{
			App.ScrollDown("TestCollectionView", ScrollStrategy.Gesture, 0.9);
		}

		// Scroll back to the top to bring the first item back into view.
		for (int i = 0; i < 6; i++)
		{
			App.ScrollUp("TestCollectionView", ScrollStrategy.Gesture, 0.9);
		}

		// The first item must be visible again.
		App.WaitForElement("CV_Item_0");

		// After the fix: the item retains its expanded size.
		// With the regression: the recycled view is recreated from the template and
		// reverts to the default (smaller) height.
		var heightAfterScroll = App.WaitForElement("CV_Item_0").GetRect().Height;
		Assert.That(heightAfterScroll, Is.GreaterThanOrEqualTo(expandedHeight * 0.9),
			"Item should retain its dynamically-changed size after being scrolled off-screen and back.");
	}
}
