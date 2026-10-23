using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.DeviceTests.TestCases;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Toolbar)]
	public partial class ToolbarTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(null, false)]
		[InlineData("Letter X", false)]
		[InlineData(null, true)]
		[InlineData("Letter X", true)]
		public async Task ToolbarIconIsVisibleWithAndWithoutText(string text, bool useFontImageSource)
		{
			SetupBuilder();
			var item = new ToolbarItem
			{
				Text = text ?? string.Empty,
				IconImageSource = useFontImageSource
					? new FontImageSource { Glyph = "X", Color = Graphics.Colors.Red, Size = 24 }
					: ImageSource.FromFile("red.png")
			};
			var navPage = new NavigationPage(new ContentPage
			{
				ToolbarItems = { item }
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var commandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				var button = Assert.IsType<AppBarButton>(Assert.Single(commandBar.PrimaryCommands));
				var icon = Assert.IsType<ImageIcon>(button.Icon);

				await AssertEventually(() => icon.Source != null && icon.ActualWidth > 0 && icon.ActualHeight > 0);
				Assert.Equal(text ?? string.Empty, button.Label);

				item.Text = "Updated label";
				Assert.Equal("Updated label", button.Label);
				Assert.Same(icon, button.Icon);
				Assert.NotNull(icon.Source);
			});
		}

		[Fact(DisplayName = "Toolbar Default Label Position")]
		public async Task ToolbarDefaultLabelPositionWithNoImages()
		{
			SetupBuilder();
			var item1 = new ToolbarItem() { Text = "Toolbar Item 1" };
			var item2 = new ToolbarItem() { Text = "Toolbar Item 2" };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var platformCommandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				Assert.True(platformCommandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Right);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Default Label Position with Image and Text")]
		public async Task ToolbarDefaultLabelPositionWithTextAndImages()
		{
			SetupBuilder();
			var item1 = new ToolbarItem() { IconImageSource = "red.png" };
			var item2 = new ToolbarItem() { Text = "Toolbar Item 2" };
			var item3 = new ToolbarItem() { Text = "Toolbar Item 2", IconImageSource = "red.png" };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2,
					item3
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var platformCommandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				Assert.True(platformCommandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Right);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Default Label Position With Images")]
		public async Task ToolbarDefaultLabelPositionOnlyImages()
		{
			SetupBuilder();
			var item1 = new ToolbarItem() { IconImageSource = "red.png" };
			var item2 = new ToolbarItem() { IconImageSource = "red.png" };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var platformCommandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				Assert.True(platformCommandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Collapsed);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Default Label Position Add Text to Images")]
		public async Task ToolbarDefaultLabelPositionAddText()
		{
			SetupBuilder();

			var item1 = new ToolbarItem() { IconImageSource = "red.png" };
			var item2 = new ToolbarItem() { IconImageSource = "red.png" };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				navPage.CurrentPage.ToolbarItems[0].Text = "Toolbar Item 1";

				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var platformCommandBar = ((MauiToolbar)toolbar.Handler.PlatformView).CommandBar;
				Assert.True(platformCommandBar.DefaultLabelPosition == CommandBarDefaultLabelPosition.Right);
				return Task.CompletedTask;
			});
		}
	}
}
