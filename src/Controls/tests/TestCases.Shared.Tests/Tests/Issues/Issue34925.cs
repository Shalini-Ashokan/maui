using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34925 : _IssuesUITest
{
public Issue34925(TestDevice device) : base(device) { }

public override string Issue => "Shell flyout items scroll behind FlyoutHeader on iOS";

[Test]
[Category(UITestCategories.Shell)]
public void FlyoutItemsDoNotScrollBehindFlyoutHeader()
{
// Wait for initial page to confirm Shell is loaded
App.WaitForElement("Issue34925Page1Label");

// Open the flyout via the hamburger icon
App.TapShellFlyoutIcon();

// Confirm the flyout header is visible
App.WaitForElement("Issue34925FlyoutHeader");
var headerRect = App.WaitForElement("Issue34925FlyoutHeader").GetRect();
float headerBottom = headerRect.Y + headerRect.Height;

// Wait for first flyout item below the header to confirm the list loaded
App.WaitForElement("Page 1");

// Perform a small scroll on the first flyout item (NOT the header).
// A 0.08 swipe (~8% of screen ~65pt on iPhone Xs) moves items up slightly.
// With the bug: scroll view starts behind the header (Y=0) so items entering the
//               header region remain accessible at Y < headerBottom.
// With the fix: scroll view starts below the header (Y=headerBottom) so items
//               that scroll above the scroll view's frame are clipped.
App.ScrollDown("Page 1", ScrollStrategy.Gesture, 0.08);

// Check items 1-3: after a small scroll, none should be accessible above the header bottom.
// If any item IS found at Y < headerBottom, items are scrolling behind the header (the bug).
for (int i = 1; i <= 3; i++)
{
var items = App.FindElements($"Page {i}");
if (items.Count > 0)
{
var itemRect = items.FirstOrDefault()?.GetRect();
if (itemRect.HasValue)
{
Assert.That(
(double)itemRect.Value.Y,
Is.GreaterThanOrEqualTo(headerBottom - 2),
$"'Page {i}' (Y={itemRect.Value.Y}) is visible above the header bottom " +
$"({headerBottom}), indicating flyout items are scrolling behind the header.");
}
}
}
}
}
