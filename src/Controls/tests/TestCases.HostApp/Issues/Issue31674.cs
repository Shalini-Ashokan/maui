using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31674, "Label with TextType Html is measured as height 0", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31674 : ContentPage
{
	public Issue31674()
	{
		var htmlLabel = new Label
		{
			Text = "Hello",
			TextType = TextType.Html,
			AutomationId = "HtmlLabel"
		};

		var contentView = new Issue31674CustomContentView
		{
			Content = htmlLabel
		};

		var layout = new Issue31674CustomLayout();
		layout.Add(contentView);

		var indicator = new Label
		{
			Text = "Loaded",
			AutomationId = "PageLoaded"
		};

		Content = new VerticalStackLayout
		{
			Children = { indicator, layout }
		};
	}
}

public class Issue31674CustomLayout : Layout
{
	public Size ArrangeChildren(Rect bounds)
	{
		double yOffset = 0;
		foreach (var child in Children)
		{
			var desiredSize = child.DesiredSize;
			var rect = new Rect(bounds.Left, yOffset, desiredSize.Width, desiredSize.Height);
			child.Arrange(rect);
			yOffset += desiredSize.Height;
		}
		return new Size(bounds.Width, bounds.Height);
	}

	public new Size Measure(double widthConstraint, double heightConstraint)
	{
		double totalHeight = 0;
		foreach (var child in Children)
		{
			// Virtualization guard: only measure if not yet ensured.
			// This is what causes the bug on iOS/Mac.
			if (child != null && child is Issue31674CustomContentView customChild && !customChild.isEnsured)
			{
				child.Measure(widthConstraint, heightConstraint);
				var desiredSize = child.DesiredSize;
				totalHeight += desiredSize.Height;
			}
		}
		return new Size(widthConstraint, totalHeight);
	}

	protected override ILayoutManager CreateLayoutManager() =>
		new Issue31674CustomLayoutManager(this);
}

public class Issue31674CustomLayoutManager : LayoutManager
{
	readonly Issue31674CustomLayout _layout;
	public Issue31674CustomLayoutManager(Issue31674CustomLayout layout) : base(layout)
	{
		_layout = layout;
	}

	public override Size ArrangeChildren(Rect bounds) => _layout.ArrangeChildren(bounds);
	public override Size Measure(double widthConstraint, double heightConstraint) =>
		_layout.Measure(widthConstraint, heightConstraint);
}
public class Issue31674CustomContentView : ContentView
{
	public bool isEnsured;
	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		Size size = Content.DesiredSize;
		size = ((IView)Content).Measure(widthConstraint, double.PositiveInfinity);
		isEnsured = true;
		return base.MeasureOverride(widthConstraint, size.Height);
	}

	protected override Size ArrangeOverride(Rect bounds) => base.ArrangeOverride(bounds);
}
