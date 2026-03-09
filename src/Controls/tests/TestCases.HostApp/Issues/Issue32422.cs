namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32422, "WebView Height Continuously Increases when webview inside Border layout", PlatformAffected.Android)]
public class Issue32422 : ContentPage
{
	const double StableHeightThreshold = 5.0;
	public Issue32422()
	{
		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Waiting...",
		};
		var customView = new Issue32422CustomView
		{
			AutomationId = "CustomView",
			VerticalOptions = LayoutOptions.Fill,
		};
		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
			},
			Children = { statusLabel, customView },
		};
		Grid.SetRow(statusLabel, 0);
		Grid.SetRow(customView, 1);
	}
}
public class Issue32422CustomView : Grid
{
	public Issue32422CustomView()
	{
		var border = new Border() { Stroke = Colors.Red };
		var grid = new Grid();
		var webview = new WebView();
		var fallbackHtml = @"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body>
<div id='editor' contenteditable='true'>Loading editor...</div>
</body>
</html>";
		webview.Source = new HtmlWebViewSource { Html = fallbackHtml };
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
		border.Content = grid;
		grid.Children.Add(webview);
		grid.Background = new SolidColorBrush(Colors.Yellow);
		Grid.SetRow(webview, 0);
		this.Children.Add(border);
	}
}
