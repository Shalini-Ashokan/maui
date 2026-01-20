using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33345, "FlowDirection MatchParent not resolved in custom layouts", PlatformAffected.Android)]
public class Issue33345 : ContentPage
{
    public Issue33345()
    {
        FlowDirection = FlowDirection.RightToLeft;
        Title = "Issue 33345 - FlowDirection MatchParent";

        var customLayout = new Issue33345CustomStackLayout
        {
            AutomationId = "CustomLayout",
            Padding = 20
        };

        var instructions = new Label
        {
            Text = "Tap the yellow label to add a second label.\nBoth labels should be right-aligned (RTL).",
            Margin = new Thickness(0, 0, 0, 10)
        };

        Content = new StackLayout
        {
            Children = { instructions, customLayout }
        };
    }
}

public abstract class Issue33345CustomLayout : Layout
{
    internal abstract Size LayoutArrangeChildren(Rect bounds);
    internal abstract Size LayoutMeasure(double widthConstraint, double heightConstraint);
}

internal class CustomLayoutManager33345 : ILayoutManager
{
    private Issue33345CustomLayout layout;

    internal CustomLayoutManager33345(Issue33345CustomLayout layout)
    {
        this.layout = layout;
    }

    public Size ArrangeChildren(Rect bounds)
    {
        return this.layout.LayoutArrangeChildren(bounds);
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        var measuredSize = this.layout.LayoutMeasure(widthConstraint, heightConstraint);
        return measuredSize;
    }
}

public class Issue33345CustomStackLayout : Issue33345CustomLayout
{
    private Label _label1 = null;
    private ContentView view1 = null;
    private ContentView view2 = null;
    private Label _label2 = null;
    private const double Spacing = 6;

    protected override ILayoutManager CreateLayoutManager()
    {
        return new CustomLayoutManager33345(this);
    }

    public Issue33345CustomStackLayout()
    {
        view1 = new ContentView();
        _label1 = new Label
        {
            Text = "Label 1 - Tap to add Label 2",
            Background = Colors.Yellow,
            TextColor = Colors.Black,
            AutomationId = "Label1",
            Padding = 10
        };
        view1.Content = _label1;

        Loaded += CustomLayout_Loaded;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, __) => AddSecondLabelIfNeeded();
        GestureRecognizers.Add(tap);
    }

    private void CustomLayout_Loaded(object sender, EventArgs e)
    {
        if (!Children.Contains(view1))
            Children.Add(view1);
    }

    private void AddSecondLabelIfNeeded()
    {
        if (_label2 != null)
            return;

        view2 = new ContentView();
        _label2 = new Label
        {
            Text = "Label 2 - Should be RTL",
            Background = Colors.LightGreen,
            TextColor = Colors.Black,
            AutomationId = "Label2",
            Padding = 10
        };
        view2.Content = _label2;
        Children.Add(view2);
        InvalidateMeasure();
    }

    internal override Size LayoutMeasure(double widthConstraint, double heightConstraint)
    {
        (view1 as IView)!.Measure(widthConstraint, heightConstraint);
        var label1Size = (view1 as IView)!.DesiredSize;

        var label2Size = Size.Zero;
        if (_label2 is View l2)
        {
            l2.Measure(widthConstraint, heightConstraint);
            label2Size = l2.DesiredSize;
        }

        if (view2 is IView v2)
        {
            v2.Measure(widthConstraint, heightConstraint);
        }

        return new Size(widthConstraint, label1Size.Height + (label2Size.Height > 0 ? label2Size.Height + Spacing : 0));
    }

    internal override Size LayoutArrangeChildren(Rect bounds)
    {
        var y = bounds.Y;

        var label1Height = (view1 as IView)!.DesiredSize.Height;
        (view1 as IView)!.Arrange(new Rect(bounds.X, y, bounds.Width, label1Height));
        y += label1Height;

        if (view2 is IView v2)
        {
            y += Spacing;
            var label2Height = _label2!.DesiredSize.Height;
            v2.Arrange(new Rect(bounds.X, y, bounds.Width, label2Height));
            y += label2Height;
        }

        return new Size(bounds.Width, y - bounds.Y);
    }
}