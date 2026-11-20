#nullable enable

using System.Reflection;
using Android.Graphics;
using Android.Graphics.Drawables;
using Microsoft.Maui.Maps.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public class MapIconTests
	{
		[Theory]
		[InlineData(200, 200, 100, 100, 100)]
		[InlineData(200, 100, 100, 100, 50)]
		[InlineData(40, 80, 100, 50, 100)]
		[InlineData(100, 100, 100, 100, 100)]
		[InlineData(200, 100, 64, 64, 32)]
		[InlineData(40, 80, 250, 125, 250)]
		[InlineData(250, 250, 250, 250, 250)]
		[InlineData(1, 100, 64, 1, 64)]
		[InlineData(100, 1, 64, 64, 1)]
		public void BitmapDrawableIsScaledToMarkerSize(int width, int height, int iconSize, int expectedWidth, int expectedHeight)
		{
			using var source = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888!);
			using var drawable = new BitmapDrawable(global::Android.App.Application.Context.Resources, source);
			using var result = ConvertDrawable(drawable, iconSize);

			Assert.Equal(expectedWidth, result.Width);
			Assert.Equal(expectedHeight, result.Height);
			Assert.NotSame(source, result);
		}

		[Theory]
		[InlineData(64, 64, 32)]
		[InlineData(100, 100, 50)]
		[InlineData(250, 250, 125)]
		public void DrawableWithIntrinsicSizePreservesAspectRatio(int iconSize, int expectedWidth, int expectedHeight)
		{
			using var drawable = new GradientDrawable();
			drawable.SetSize(200, 100);
			using var result = ConvertDrawable(drawable, iconSize);

			Assert.Equal(expectedWidth, result.Width);
			Assert.Equal(expectedHeight, result.Height);
		}

		[Theory]
		[InlineData(64)]
		[InlineData(100)]
		[InlineData(250)]
		public void DrawableWithoutIntrinsicSizeUsesMarkerSize(int iconSize)
		{
			using var drawable = new ColorDrawable(Color.Red);
			using var result = ConvertDrawable(drawable, iconSize);

			Assert.Equal(iconSize, result.Width);
			Assert.Equal(iconSize, result.Height);
		}

		static Bitmap ConvertDrawable(Drawable drawable, int iconSize)
		{
			var method = typeof(MapHandler).GetMethod("DrawableToBitmap", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(method);
			return Assert.IsType<Bitmap>(method.Invoke(null, new object[] { drawable, iconSize }));
		}
	}
}
