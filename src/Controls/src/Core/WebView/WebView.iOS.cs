namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		// On iOS/MacCatalyst, WKWebView delegates first-responder to the private WKContentView
		// subview. MAUI never learns when web content is focused, so IsFocused stays false and
		// VisualElement.Unfocus() returns early without calling MapUnfocus. Hiding Unfocus()
		// here bypasses that guard and directly invokes EndEditing(true) in the handler,
		// which recursively walks all subviews to dismiss the keyboard (issue #36201).
		public new void Unfocus() => Handler?.Invoke(nameof(IView.Unfocus));
	}
}