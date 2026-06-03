namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14577, "Width/Height x:Reference bindings show wrong values on initial render on Windows", PlatformAffected.iOS | PlatformAffected.Windows)]
public partial class Issue14577 : ContentPage
{
	public Issue14577()
	{
		InitializeComponent();
	}
}
