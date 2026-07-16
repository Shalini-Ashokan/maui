using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;
using WebKit;

namespace Microsoft.Maui.Platform
{
	public class MauiWKWebView : WKWebView, IWebViewDelegate, IUIViewLifeCycleEvents
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Used to persist cookies across WebView instances. Not a leak.")]
		static WKProcessPool? SharedPool;

		string? _pendingUrl;
		readonly WeakReference<WebViewHandler> _handler;

		public MauiWKWebView(WebViewHandler handler)
			: this(RectangleF.Empty, handler)
		{
		}

		public MauiWKWebView(CGRect frame, WebViewHandler handler)
			: this(frame, handler, CreateConfiguration())
		{
		}

		public MauiWKWebView(CGRect frame, WebViewHandler handler, WKWebViewConfiguration configuration)
			: base(frame, configuration)
		{
			_ = handler ?? throw new ArgumentNullException(nameof(handler));
			_handler = new WeakReference<WebViewHandler>(handler);

			BackgroundColor = UIColor.Clear;
			AutosizesSubviews = true;

			NavigationDelegate = new MauiWebViewNavigationDelegate(handler);

			// Unlike Android and Windows, WKWebView does not automatically restore focus to the
			// previously focused DOM element when it becomes the first responder again. Track the
			// last focused element in the page so it can be explicitly refocused from Focus().
			// See https://github.com/dotnet/maui/issues/36201
			configuration.UserContentController.AddUserScript(LastFocusedElementTrackerScript);
		}

		internal const string RefocusLastFocusedElementScript = @"(function() {
			var el = window.__mauiLastFocusedElement;
			if (!el || typeof el.focus !== 'function') {
				return;
			}
			el.focus();
			// Move the caret to the end of the content instead of leaving it at the
			// beginning, matching the behavior users expect when regaining focus.
			if (el.isContentEditable) {
				var range = document.createRange();
				range.selectNodeContents(el);
				range.collapse(false);
				var selection = window.getSelection();
				selection.removeAllRanges();
				selection.addRange(range);
			} else if (typeof el.setSelectionRange === 'function' && typeof el.value === 'string') {
				var end = el.value.length;
				el.setSelectionRange(end, end);
			}
		})();";

		internal const string BlurActiveElementScript = @"(function() {
			var el = document.activeElement;
			if (el && typeof el.blur === 'function') {
				el.blur();
			}
		})();";

		internal const string FocusStateMessageHandlerName = "mauiWebViewFocusState";

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Shared, immutable script instance reused across all WebView instances. Not a leak.")]
		static readonly WKUserScript LastFocusedElementTrackerScript = new WKUserScript(
			new NSString(@"(function() {
				if (window.__mauiFocusTrackerInstalled) {
					return;
				}
				window.__mauiFocusTrackerInstalled = true;
				function postFocusState(state) {
					if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.mauiWebViewFocusState) {
						window.webkit.messageHandlers.mauiWebViewFocusState.postMessage(state);
					}
				}
				document.addEventListener('focusin', function (e) {
					window.__mauiLastFocusedElement = e.target;
					postFocusState('focus');
				}, true);
				document.addEventListener('focusout', function (e) {
					postFocusState('blur');
				}, true);
			})();"),
			WKUserScriptInjectionTime.AtDocumentStart,
			false);

		public string? CurrentUrl =>
			Url?.AbsoluteUrl?.ToString();

		public override void MovedToWindow()
		{
			base.MovedToWindow();

			if (!string.IsNullOrWhiteSpace(_pendingUrl))
			{
				var closure = _pendingUrl;
				_pendingUrl = null;

				// I realize this looks like the worst hack ever but iOS 11 and cookies are super quirky
				// and this is the only way I could figure out how to get iOS 11 to inject a cookie 
				// the first time a WkWebView is used in your app. This only has to run the first time a WkWebView is used 
				// anywhere in the application. All subsequents uses of WkWebView won't hit this hack
				// Even if it's a WkWebView on a new page.
				// read through this thread https://developer.apple.com/forums/thread/99674
				// Or Bing "WkWebView and Cookies" to see the myriad of hacks that exist
				// Most of them all came down to different variations of synching the cookies before or after the
				// WebView is added to the controller. This is the only one I was able to make work
				// I think if we could delay adding the WebView to the Controller until after ViewWillAppear fires that might also work
				// But we're not really setup for that
				// If you'd like to try your hand at cleaning this up then UI Test Issue12134 and Issue3262 are your final bosses
				InvokeOnMainThread(async () =>
				{
					await Task.Delay(500);
					if (_handler.TryGetTarget(out var handler))
						await handler.FirstLoadUrlAsync(closure);
				});
			}

			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}

		[Obsolete("Use MauiWebViewNavigationDelegate.DidFinishNavigation instead.")]
		public async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
		{
			var url = CurrentUrl;

			if (url == null || url == $"file://{NSBundle.MainBundle.BundlePath}/")
				return;

			if (_handler.TryGetTarget(out var handler))
				await handler.ProcessNavigatedAsync(url);
		}

		[Export("webViewWebContentProcessDidTerminate:")]
		public void ContentProcessDidTerminate(WKWebView webView)
		{
			if (_handler.TryGetTarget(out var handler))
				handler.VirtualView.ProcessTerminated(new WebProcessTerminatedEventArgs(webView));
		}

		public void LoadHtml(string? html, string? baseUrl)
		{
			if (html != null)
				LoadHtmlString(html, baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true));
		}

		async Task LoadUrlAsync(string? url)
		{
			try
			{
				var uri = new Uri(url ?? string.Empty);
				var safeHostUri = new Uri($"{uri.Scheme}://{uri.Authority}", UriKind.Absolute);
				var safeRelativeUri = new Uri($"{uri.PathAndQuery}{uri.Fragment}", UriKind.Relative);
				var safeFullUri = new Uri(safeHostUri, safeRelativeUri);
				NSUrlRequest request = new NSUrlRequest(new NSUrl(safeFullUri.AbsoluteUri));

				if (_handler.TryGetTarget(out var handler))
				{
					if (handler.HasCookiesToLoad(safeFullUri.AbsoluteUri) &&
						!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
					{
						return;
					}

					await handler.SyncPlatformCookiesAsync(safeFullUri.AbsoluteUri);
				}

				LoadRequest(request);
			}
			catch (UriFormatException formatException)
			{
				// If we got a format exception trying to parse the URI, it might be because
				// someone is passing in a local bundled file page. If we can find a better way
				// to detect that scenario, we should use it; until then, we'll fall back to 
				// local file loading here and see if that works:
				if (!string.IsNullOrEmpty(url))
				{
					if (!LoadFile(url))
					{
						if (_handler.TryGetTarget(out var handler))
							handler.MauiContext?.CreateLogger<MauiWKWebView>()?.LogWarning($"Unable to Load Url {url}: {formatException}");
					}
				}
			}
			catch (Exception exc)
			{
				if (_handler.TryGetTarget(out var handler))
					handler.MauiContext?.CreateLogger<MauiWKWebView>()?.LogWarning($"Unable to Load Url {url}: {exc}");
			}
		}

		public void LoadUrl(string? url)
		{
			LoadUrlAsync(url).FireAndForget();
		}

		// https://developer.apple.com/forums/thread/99674
		// WKWebView and making sure cookies synchronize is really quirky
		// The main workaround I've found for ensuring that cookies synchronize 
		// is to share the Process Pool between all WkWebView instances.
		// It also has to be shared at the point you call init
		public static WKWebViewConfiguration CreateConfiguration()
		{
			// By default, setting inline media playback to allowed, including autoplay
			// and picture in picture, since these things MUST be set during the webview
			// creation, and have no effect if set afterwards.
			// A custom handler factory delegate could be set to disable these defaults
			// but if we do not set them here, they cannot be changed once the
			// handler's platform view is created, so erring on the side of wanting this
			// capability by default.
			var config = new WKWebViewConfiguration();
			if (OperatingSystem.IsMacCatalystVersionAtLeast(10) || OperatingSystem.IsIOSVersionAtLeast(10))
			{
				config.AllowsPictureInPictureMediaPlayback = true;
				config.AllowsInlineMediaPlayback = true;
				config.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
			}
			if (SharedPool == null)
				SharedPool = config.ProcessPool;
			else
				config.ProcessPool = SharedPool;

			return config;
		}

		bool LoadFile(string url)
		{
			try
			{
				var file = Path.GetFileNameWithoutExtension(url);
				var ext = Path.GetExtension(url);

				var nsUrl = NSBundle.MainBundle.GetUrlForResource(file, ext);

				if (nsUrl == null)
				{
					return false;
				}

				LoadFileUrl(nsUrl, nsUrl);

				return true;
			}
			catch (Exception ex)
			{
				if (_handler.TryGetTarget(out var handler))
					handler.MauiContext?.CreateLogger<MauiWKWebView>()?.LogWarning($"Could not load {url} as local file: {ex}");
			}

			return false;
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}
	}
}