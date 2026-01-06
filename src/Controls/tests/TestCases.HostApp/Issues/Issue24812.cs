#if ANDROID
using Android.Webkit;
using Microsoft.Maui.Handlers;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24812, "WebViewHandler OnProgressChanged not called on Android", PlatformAffected.Android)]
public class Issue24812 : ContentPage
{
	private readonly Label _statusLabel;
	private readonly Issue24812CustomWebView _webView;
	public Issue24812()
	{
		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Progress: 0",
			FontSize = 16
		};

		_webView = new Issue24812CustomWebView
		{
			AutomationId = "TestWebView",
			HeightRequest = 400,
			Source = "https://www.google.com"
		};

		_webView.ProgressChanged += OnProgressChanged;
		Content = new VerticalStackLayout
		{
			Padding = 10,
			Spacing = 10,
			Children = { _statusLabel, _webView }
		};
	}
	private void OnProgressChanged(object sender, Issue24812ProgressChangedEventArgs e)
	{
		_statusLabel.Text = $"Progress: {e.Progress}";
	}
}

public class Issue24812ProgressChangedEventArgs : EventArgs
{
	public double Progress { get; }
	public Issue24812ProgressChangedEventArgs(double progress)
	{
		Progress = progress;
	}
}

public class Issue24812CustomWebView : Microsoft.Maui.Controls.WebView
{
	public event EventHandler<Issue24812ProgressChangedEventArgs> ProgressChanged;
	internal void RaiseProgressChanged(double progress)
	{
		ProgressChanged?.Invoke(this, new Issue24812ProgressChangedEventArgs(progress));
	}
}

#if ANDROID
public class Issue24812CustomWebViewHandler : WebViewHandler
{
	private Issue24812CustomWebView CustomWebView => (Issue24812CustomWebView)VirtualView;
	protected override void ConnectHandler(Android.Webkit.WebView platformView)
	{
		base.ConnectHandler(platformView);
		platformView.SetWebChromeClient(new Issue24812CustomWebChromeClient(CustomWebView));
		platformView.Settings.JavaScriptEnabled = true;
	}

	private class Issue24812CustomWebChromeClient : WebChromeClient
	{
		private readonly Issue24812CustomWebView _customWebView;
		public Issue24812CustomWebChromeClient(Issue24812CustomWebView customWebView)
		{
			_customWebView = customWebView;
		}
		public override void OnProgressChanged(Android.Webkit.WebView view, int newProgress)
		{
			base.OnProgressChanged(view, newProgress);
			_customWebView.RaiseProgressChanged(newProgress);
		}
	}
}
#endif