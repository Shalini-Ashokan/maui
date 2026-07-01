using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36201 : _IssuesUITest
{
	public Issue36201(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "WebView.Focus() & Unfocus() behaves inconsistently across platforms";

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewFocus_AfterUnfocus_RestoresFocusToEditor()
	{
		// Wait for WebView to finish loading
		App.WaitForElement("EditorWebView");
		// Step 1: Tap the WebView content to manually focus the editor (simulates user interaction)
		App.Tap("EditorWebView");

#if ANDROID || IOS
       App.DismissKeyboard();
#endif

		// Step 2: Unfocus via WebView.Unfocus()
		App.Tap("UnfocusButton");
		App.WaitForElement("CheckStatusButton");
		App.Tap("CheckStatusButton");

		var statusAfterUnfocus = App.FindElement("FocusStatusLabel").GetText();
		Assert.That(statusAfterUnfocus, Is.EqualTo("unfocused"),
			"WebView.Unfocus() should remove focus from the contenteditable element.");

		// Step 3: Call WebView.Focus() — should restore focus to the contenteditable element
		// on all platforms (iOS and MacCatalyst fixed via MapFocus override in WebViewHandler.iOS.cs).
		App.Tap("FocusButton");
		App.WaitForElement("CheckStatusButton");
		App.Tap("CheckStatusButton");

		var statusAfterFocus = App.FindElement("FocusStatusLabel").GetText();
		Assert.That(statusAfterFocus, Is.EqualTo("focused"),
			"WebView.Focus() should restore focus to the contenteditable element on all platforms.");
	}
}
