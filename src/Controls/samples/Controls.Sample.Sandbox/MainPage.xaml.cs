using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	private ImageViewModel _viewModel;
	public MainPage()
	{
		InitializeComponent();
		_viewModel = new ImageViewModel();
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new ImageViewModel();
		await Navigation.PushAsync(new ImageOptionsPage(_viewModel));
	}

}

public class ImageViewModel : INotifyPropertyChanged
{
	private Aspect _aspect = Aspect.AspectFit;
	// IMPORTANT: must start out null (not Colors.Transparent). A non-null Background makes
	// ImageHandler.NeedsContainer true from the very first render, so the WrapperView container
	// already exists and the crash never happens. Starting at null reproduces the real first-time
	// container-creation race that happens in ImageHandler.Windows.cs MapBackground.
	private Color? _backgroundColor = null;

	/// <summary>
	/// Gets or sets the aspect mode for the image.
	/// </summary>
	public Aspect Aspect
	{
		get => _aspect;
		set { if (_aspect != value) { _aspect = value; OnPropertyChanged(); } }
	}

	/// <summary>
	/// Gets or sets the background color for the image.
	/// </summary>
	public Color? BackgroundColorValue
	{
		get => _backgroundColor;
		set { if (_backgroundColor != value) { _backgroundColor = value; OnPropertyChanged(); } }
	}





	// ...existing code...


	// ...existing code...
	/// <summary>
	/// Gets or sets the image source path (filename or URL).
	/// </summary>

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}