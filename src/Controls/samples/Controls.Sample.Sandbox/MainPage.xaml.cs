namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public List<CarouselItem> Items { get; } = new()
	{
		new CarouselItem { Name = "Item One",   Description = "First carousel item"  },
		new CarouselItem { Name = "Item Two",   Description = "Second carousel item" },
		new CarouselItem { Name = "Item Three", Description = "Third carousel item"  },
		new CarouselItem { Name = "Item Four",  Description = "Fourth carousel item" },
		new CarouselItem { Name = "Item Five",  Description = "Fifth carousel item"  },
	};

	public MainPage()
	{
		InitializeComponent();
		BindingContext = this;
		TheCarouselView.IndicatorView = TheIndicatorView;
	}
}

public class CarouselItem
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
}
