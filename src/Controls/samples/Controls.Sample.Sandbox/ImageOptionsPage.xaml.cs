namespace Maui.Controls.Sample;

public partial class ImageOptionsPage : ContentPage
{
	private ImageViewModel _viewModel;
	public ImageOptionsPage(ImageViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

	}
	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
	// Aspect RadioButton group
	private void AspectRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked)
		{
			switch (rb.Content?.ToString())
			{
				case "AspectFit":
					_viewModel.Aspect = Aspect.AspectFit;
					break;
				case "AspectFill":
					_viewModel.Aspect = Aspect.AspectFill;
					break;
				case "Fill":
					_viewModel.Aspect = Aspect.Fill;
					break;
				case "Center":
					_viewModel.Aspect = Aspect.Center;
					break;
			}
		}
	}

	private void BgColorRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked)
		{
			_viewModel.BackgroundColorValue = rb.Content?.ToString() switch
			{
				"Red" => Colors.Red,
				"Green" => Colors.Green,
				"Blue" => Colors.Blue,
				"Yellow" => Colors.Yellow,
				_ => null,
			};
		}
	}
}
