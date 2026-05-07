namespace Maui.Controls.Sample.Issues;

// x:Name="issue34975SecondPage" in the XAML is the key trigger for the memory leak.
// It causes the page to register itself in its own XAML NameScope, which combined with
// Shell.TitleView creates a retain cycle on iOS that prevents GC collection.
public partial class Issue34975SecondPage : ContentPage
{
	// Only tracks instances when IsTracking is true.
	// On Mac under Appium automation, the Mac2 AXObserver holds strong native references
	// to any page that is visible during the session. A second navigation round forces the
	// AXObserver to update its tracked elements, releasing the first round's native refs.
	// We track only the first-round instances (IsTracking = true) so we can verify that
	// those are collected after the second round flushes the accessibility cache.
	public static List<WeakReference> Instances { get; } = [];
	public static bool IsTracking { get; set; }

	public Issue34975SecondPage()
	{
		InitializeComponent();
		if (IsTracking)
			Instances.Add(new WeakReference(this));
	}
}
