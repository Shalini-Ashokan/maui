namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36201, "WebView Focus and Unfocus behaves inconsistently across platforms", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue36201 : ContentPage
{
	readonly WebView _webView;
	readonly Label _focusStatusLabel;

	const string HtmlContent = @"<!doctype html>
<html>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1' />
  <style>
    body { font-family: sans-serif; margin: 12px; }
    #editor {
      min-height: 80px;
      border: 2px solid #ccc;
      padding: 10px;
      border-radius: 4px;
      outline: none;
    }
    #editor:focus { border-color: #3b82f6; }
    #status { margin-top: 8px; font-size: 13px; color: #555; }
  </style>
</head>
<body>
  <div id='editor' contenteditable='true' tabindex='0'>Editable content area — tap Focus/Unfocus to test.</div>
</body>
</html>";

	public Issue36201()
	{
		_focusStatusLabel = new Label
		{
			Text = "Loading...",
			AutomationId = "FocusStatusLabel"
		};

		_webView = new WebView
		{
			HeightRequest = 250,
			HorizontalOptions = LayoutOptions.Fill,
			AutomationId = "EditorWebView",
			Source = new HtmlWebViewSource { Html = HtmlContent }
		};

		var focusButton = new Button
		{
			Text = "Focus WebView",
			AutomationId = "FocusButton"
		};
		focusButton.Clicked += (s, e) =>
		{
			_webView.Focus();
		};

		var unfocusButton = new Button
		{
			Text = "Unfocus WebView",
			AutomationId = "UnfocusButton"
		};
		unfocusButton.Clicked += (s, e) =>
		{
			_webView.Unfocus();
		};

		var checkStatusButton = new Button
		{
			Text = "Check Status",
			AutomationId = "CheckStatusButton"
		};
		checkStatusButton.Clicked += (s, e) => UpdateFocusStatusAsync();

		Content = new VerticalStackLayout
		{
			Padding = 12,
			Spacing = 8,
			Children =
			{
				_webView,
				_focusStatusLabel,
				focusButton,
				unfocusButton,
				checkStatusButton
			}
		};
	}

	void UpdateFocusStatusAsync()
	{
		var result = _webView.IsFocused;
		_focusStatusLabel.Text = result ? "focused" : "unfocused";
	}
}
