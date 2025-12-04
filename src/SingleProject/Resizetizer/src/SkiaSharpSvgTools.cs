using System;
using System.Diagnostics;
using SkiaSharp;
using Svg.Skia;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpSvgTools : SkiaSharpTools, IDisposable
	{
		SKSvg svg;

		public SkiaSharpSvgTools(ResizeImageInfo info, ILogger logger)
			: this(info.Filename, info.BaseSize, info.Color, info.TintColor, logger)
		{
		}

		public SkiaSharpSvgTools(string filename, SKSize? baseSize, SKColor? backgroundColor, SKColor? tintColor, ILogger logger)
			: base(filename, baseSize, backgroundColor, tintColor, logger)
		{
			var sw = new Stopwatch();
			sw.Start();

			svg = new SKSvg();
			svg.Load(filename);

			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms ({filename})");

			if (svg.Picture is null)
			{
				var errorMessage = $"Unable to load SVG file '{filename}'. The SVG file may be invalid or missing required attributes like 'viewBox'.";
				Logger?.Log($"ERROR: {errorMessage}");
				throw new InvalidOperationException(errorMessage);
			}

			if (svg.Picture.CullRect.Size.IsEmpty)
			{
				Logger?.Log($"SVG picture did not have a size and will fail to generate. ({Filename})");
			}
		}

		public override SKSize GetOriginalSize() =>
			svg.Picture?.CullRect.Size ?? SKSize.Empty;

		public override void DrawUnscaled(SKCanvas canvas, float scale)
		{
			if (svg is null || svg.Picture is null)
			{
				Logger?.Log($"ERROR: Cannot draw SVG - svg or svg.Picture is null for file '{Filename}'");
				throw new InvalidOperationException($"Cannot draw SVG file '{Filename}'. The SVG object or Picture is null.");
			}

			if (scale >= 1)
			{
				// draw using default scaling
				canvas.DrawPicture(svg.Picture, Paint);
			}
			else
			{
				// draw using raster downscaling
				var size = GetOriginalSize();

				if (size.IsEmpty)
				{
					Logger?.Log($"ERROR: Cannot draw SVG - size is empty for file '{Filename}'");
					throw new InvalidOperationException($"Cannot draw SVG file '{Filename}'. The SVG has an empty size.");
				}

				// vector scaling has rounding issues, so first draw as intended
				var info = new SKImageInfo((int)size.Width, (int)size.Height);
				using var surface = SKSurface.Create(info);

				if (surface is null)
				{
					Logger?.Log($"ERROR: Failed to create SKSurface for file '{Filename}'");
					throw new InvalidOperationException($"Failed to create drawing surface for SVG file '{Filename}'.");
				}

				var cvn = surface.Canvas;

				// draw to a larger canvas first
				cvn.Clear(SKColors.Transparent);
				cvn.DrawPicture(svg.Picture, Paint);

				// convert it all into an image
				using var img = surface.Snapshot();

				// draw to the main canvas using the correct quality settings
				canvas.DrawImage(img, 0, 0, SamplingOptions, Paint);
			}
		}

		public void Dispose()
		{
			svg?.Dispose();
			svg = null;
		}
	}
}
