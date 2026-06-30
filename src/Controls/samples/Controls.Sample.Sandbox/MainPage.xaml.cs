namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	CancellationTokenSource? _runCancellation;
	MemorySnapshot _baseline = MemorySnapshot.Empty;

	public MainPage()
	{
		InitializeComponent();
	}

	void OnRunControlClicked(object? sender, EventArgs e) => _ = RunAsync(ReproMode.StaticThemeControl);

	void OnRunLeakClicked(object? sender, EventArgs e) => _ = RunAsync(ReproMode.LiveThemeSwap);

	void OnRunPreloadedClicked(object? sender, EventArgs e) => _ = RunAsync(ReproMode.PreloadedThemeSwap);

	void OnStopClicked(object? sender, EventArgs e) => _runCancellation?.Cancel();

	ReproOptions ReadOptions(ReproMode mode)
	{
		return new ReproOptions(
			mode,
			ReadBoundedInt(CyclesEntry.Text, 1, 200, 40),
			ReadBoundedInt(PayloadEntry.Text, 0, 64, 3),
			ReadBoundedInt(DwellEntry.Text, 0, 5000, 100));
	}

	static int ReadBoundedInt(string? text, int min, int max, int fallback)
	{
		if (!int.TryParse(text, out var value))
			value = fallback;

		return Math.Min(max, Math.Max(min, value));
	}

	async Task RunAsync(ReproMode mode)
	{
		if (_runCancellation is not null)
			return;

		var options = ReadOptions(mode);
		_runCancellation = new CancellationTokenSource();
		var token = _runCancellation.Token;

		SetRunning(true);
		RunProgress.Progress = 0;
		SummaryLabel.Text = "Taking baseline after full GC...";

		var session = new ReproSession(options);
		ReproSession.Current = session;

		try
		{
			_baseline = await MemorySampler.TakeAfterCollectionAsync();
			SummaryLabel.Text = $"Baseline captured. Running {options.Name}.";

			for (var i = 0; i < options.Cycles; i++)
			{
				token.ThrowIfCancellationRequested();
				var cycle = session.BeginNextCycle();
				StatusLabel.Text = $"Pushing responsive page {cycle + 1}/{options.Cycles}: {options.Name}";

				await Navigation.PushAsync(new AdaptiveTriggerLeakPage(), false);
				await session.WaitForCurrentPageReadyAsync(token);

				if (options.DwellMilliseconds > 0)
					await Task.Delay(options.DwellMilliseconds, token);

				RunProgress.Progress = (i + 1d) / (options.Cycles * 2d);
			}

			for (var i = 0; i < options.Cycles; i++)
			{
				token.ThrowIfCancellationRequested();
				StatusLabel.Text = $"Popping responsive page {i + 1}/{options.Cycles}: {options.Name}";

				await Navigation.PopAsync(false);

				// Give Android's ART GC and native view hierarchy (JNI) time to release
				// platform references before measuring. 25 ms is too short on Android —
				// the fragment back-stack cleanup and handler disconnect happen asynchronously.
#if ANDROID
				await Task.Delay(150, token);
#else
				await Task.Delay(25, token);
#endif

				if ((i + 1) % 5 == 0 || i + 1 == options.Cycles)
				{
					var current = await MemorySampler.TakeAfterCollectionAsync();
					SummaryLabel.Text = session.GetStats(_baseline, current).ToSummary();
				}

				RunProgress.Progress = (options.Cycles + i + 1d) / (options.Cycles * 2d);
			}

			var finalSnapshot = await MemorySampler.TakeAfterCollectionAsync();
			SummaryLabel.Text = session.GetStats(_baseline, finalSnapshot).ToSummary();
			StatusLabel.Text = $"Completed {options.Name}.";
		}
		catch (OperationCanceledException)
		{
			StatusLabel.Text = "Run stopped.";
		}
		catch (Exception ex)
		{
			StatusLabel.Text = "Run failed.";
			SummaryLabel.Text = ex.ToString();
		}
		finally
		{
			ReproSession.Current = session;
			_runCancellation?.Dispose();
			_runCancellation = null;
			SetRunning(false);
		}
	}

	void SetRunning(bool isRunning)
	{
		RunLeakButton.IsEnabled = !isRunning;
		RunControlButton.IsEnabled = !isRunning;
		RunPreloadedThemeButton.IsEnabled = !isRunning;
		StopButton.IsEnabled = isRunning;
	}
}