using System.Diagnostics;
using System.Text;

namespace Maui.Controls.Sample;

public class Issue35613NavigationReproPageBase : ContentPage
{
    static readonly List<string> s_logEntries = new();
    static event Action? LogChanged;

    string _pageName;
    Editor? _logEditor;

    protected Issue35613NavigationReproPageBase()
    {
        _pageName = "Page";
        Title = "Issue 35613";
    }

    protected Issue35613NavigationReproPageBase(string pageName)
    {
        _pageName = pageName;
        Title = $"Issue 35613 - {pageName}";
    }

    protected string PageName => _pageName;

    protected void SetPageName(string pageName)
    {
        _pageName = pageName;
        Title = $"Issue 35613 - {pageName}";
    }

    protected void SetLogEditor(Editor logEditor)
    {
        _logEditor = logEditor;
        RefreshLogEditor();
    }

    protected static void ClearLog()
    {
        s_logEntries.Clear();
        RaiseLogChanged();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LogChanged += OnLogChanged;
        RefreshLogEditor();
    }

    protected override void OnDisappearing()
    {
        LogChanged -= OnLogChanged;
        base.OnDisappearing();
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        AppendLog($"OnNavigatingFrom: {_pageName} [{args.NavigationType}], DestinationPage={ResolvePageName(args.DestinationPage)}");
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        AppendLog($"OnNavigatedFrom: {_pageName} [{args.NavigationType}], DestinationPage={ResolvePageName(args.DestinationPage)}");
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        AppendLog($"OnNavigatedTo: {_pageName} [{args.NavigationType}], PreviousPage={ResolvePageName(args.PreviousPage)}");
    }

    protected Button CreatePushButton(string nextPageName, Func<Page> destinationFactory)
    {
        var normalizedPageName = nextPageName.Replace(".", string.Empty, StringComparison.Ordinal);

        return new Button
        {
            Text = $"Goto {nextPageName}",
            AutomationId = $"Issue35613_Goto{normalizedPageName}Button",
            Command = new Command(async () => await Navigation.PushAsync(destinationFactory()))
        };
    }

    protected Button CreatePopButton()
    {
        var normalizedPageName = _pageName.Replace(".", string.Empty, StringComparison.Ordinal);

        return new Button
        {
            Text = "Go Back (Pop)",
            AutomationId = $"Issue35613_GoBack{normalizedPageName}Button",
            Command = new Command(async () => await Navigation.PopAsync())
        };
    }

    protected Editor CreateLogEditor(string automationId)
    {
        var editor = new Editor
        {
            AutomationId = automationId,
            IsReadOnly = true,
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 220,
            FontFamily = "Courier New",
            FontSize = 12
        };

        SetLogEditor(editor);
        return editor;
    }

    protected View CreateLayout(string steps, params View[] actionViews)
    {
        var layout = new VerticalStackLayout();
        layout.Padding = new Thickness(16);
        layout.Spacing = 12;

        layout.Children.Add(new Label
        {
            Text = "Issue #35613 Repro",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold
        });

        layout.Children.Add(new Label { Text = $"Current page: {_pageName}" });
        layout.Children.Add(new Label { Text = steps });
        layout.Children.Add(new Label
        {
            Text = "Check OnNavigatingFrom lines. DestinationPage should match the next page on push and previous page on pop.",
            FontAttributes = FontAttributes.Italic
        });

        foreach (var actionView in actionViews)
        {
            layout.Children.Add(actionView);
        }

        return new ScrollView { Content = layout };
    }

    void AppendLog(string message)
    {
        s_logEntries.Add(message);
        Debug.WriteLine($"ISSUE35613: {message}");
        Console.WriteLine($"ISSUE35613: {message}");
        RaiseLogChanged();
    }

    static string ResolvePageName(Page? page)
    {
        if (page is Issue35613NavigationReproPageBase reproPage)
            return reproPage.PageName;

        if (page is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(page.Title))
            return page.Title;

        return page.GetType().Name;
    }

    void OnLogChanged()
    {
        RefreshLogEditor();
    }

    void RefreshLogEditor()
    {
        if (_logEditor is null)
            return;

        var builder = new StringBuilder();
        foreach (var entry in s_logEntries)
        {
            builder.AppendLine(entry);
        }

        _logEditor.Text = builder.ToString();
    }

    static void RaiseLogChanged()
    {
        LogChanged?.Invoke();
    }
}

public sealed class Issue35613Page2 : Issue35613NavigationReproPageBase
{
    public Issue35613Page2() : base("Page2")
    {
        var logEditor = CreateLogEditor("Issue35613_Page2LogEditor");
        Content = CreateLayout(
            steps: "Step 2: Click Goto Page2.1.",
            CreatePushButton("Page2.1", static () => new Issue35613Page21()),
            CreatePopButton(),
            new Label { Text = "Event Log:", FontAttributes = FontAttributes.Bold },
            logEditor);
    }
}

public sealed class Issue35613Page21 : Issue35613NavigationReproPageBase
{
    public Issue35613Page21() : base("Page2.1")
    {
        var logEditor = CreateLogEditor("Issue35613_Page21LogEditor");
        Content = CreateLayout(
            steps: "Step 3: Click Goto Page2.1.1, then go back.",
            CreatePushButton("Page2.1.1", static () => new Issue35613Page211()),
            CreatePopButton(),
            new Label { Text = "Event Log:", FontAttributes = FontAttributes.Bold },
            logEditor);
    }
}

public sealed class Issue35613Page211 : Issue35613NavigationReproPageBase
{
    public Issue35613Page211() : base("Page2.1.1")
    {
        var logEditor = CreateLogEditor("Issue35613_Page211LogEditor");
        Content = CreateLayout(
            steps: "Step 4 and 5: Click Go Back (Pop) twice overall to return toward Page2.",
            CreatePopButton(),
            new Label { Text = "Event Log:", FontAttributes = FontAttributes.Bold },
            logEditor);
    }
}