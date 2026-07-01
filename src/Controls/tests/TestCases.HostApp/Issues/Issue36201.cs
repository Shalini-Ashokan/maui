namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36201, "WebView.Focus() & Unfocus() behaves inconsistently across platforms", PlatformAffected.All)]
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
  <div id='status'>not-focused</div>
  <script>
    var editor = document.getElementById('editor');
    var status = document.getElementById('status');
    editor.addEventListener('focus', function() { status.textContent = 'focused'; });
    editor.addEventListener('blur',  function() { status.textContent = 'not-focused'; });
  </script>
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

		_webView.Navigated += async (s, e) => await UpdateFocusStatusAsync();

		var focusButton = new Button
		{
			Text = "Focus WebView",
			AutomationId = "FocusButton"
		};
		focusButton.Clicked += async (s, e) =>
		{
			_webView.Focus();
			await UpdateFocusStatusAsync();
		};

		var unfocusButton = new Button
		{
			Text = "Unfocus WebView",
			AutomationId = "UnfocusButton"
		};
		unfocusButton.Clicked += async (s, e) =>
		{
			_webView.Unfocus();
			await UpdateFocusStatusAsync();
		};

		var checkStatusButton = new Button
		{
			Text = "Check Status",
			AutomationId = "CheckStatusButton"
		};
		checkStatusButton.Clicked += async (s, e) => await UpdateFocusStatusAsync();

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

	async Task UpdateFocusStatusAsync()
	{
		var result = await _webView.EvaluateJavaScriptAsync("document.getElementById('status').textContent");
		_focusStatusLabel.Text = result ?? "unknown";
	}
}
