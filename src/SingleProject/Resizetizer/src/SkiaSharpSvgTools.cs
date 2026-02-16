using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using SkiaSharp;
using Svg.Skia;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpSvgTools : SkiaSharpTools, IDisposable
	{
		static readonly string[] RelativeUnits = { "em", "ex", "rem", "ch", "vw", "vh", "vmin", "vmax" };

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

			using var sanitized = SanitizeSvgRelativeUnits(filename);
			var pic = sanitized != null
				? svg.Load(sanitized)
				: svg.Load(filename);

			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms ({filename})");

			if (pic.CullRect.Size.IsEmpty)
				Logger?.Log($"SVG picture did not have a size and will fail to generate. ({Filename})");
		}


		internal static MemoryStream SanitizeSvgRelativeUnits(string filename)
		{
			var svgContent = File.ReadAllText(filename);
			bool mayHaveRelativeUnits = false;
			foreach (var unit in RelativeUnits)
			{
				if (svgContent.IndexOf(unit, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					mayHaveRelativeUnits = true;
					break;
				}
			}

			if (!mayHaveRelativeUnits)
				return null;

			var doc = XDocument.Parse(svgContent);
			var root = doc.Root;

			if (root is null || root.Name.LocalName != "svg")
				return null;

			var widthAttr = root.Attribute("width");
			var heightAttr = root.Attribute("height");

			if (widthAttr is null && heightAttr is null)
				return null;

			bool widthIsRelative = widthAttr is not null && HasRelativeUnit(widthAttr.Value);
			bool heightIsRelative = heightAttr is not null && HasRelativeUnit(heightAttr.Value);

			if (!widthIsRelative && !heightIsRelative)
				return null;

			// A viewBox must exist as fallback for dimensions
			var viewBoxAttr = root.Attribute("viewBox");
			if (viewBoxAttr is null || string.IsNullOrWhiteSpace(viewBoxAttr.Value))
				return null;

			if (widthIsRelative)
				widthAttr.Remove();
			if (heightIsRelative)
				heightAttr.Remove();

			var ms = new MemoryStream();
			doc.Save(ms);
			ms.Position = 0;
			return ms;
		}

		static bool HasRelativeUnit(string value)
		{
			var trimmed = value.AsSpan().Trim();
			foreach (var unit in RelativeUnits)
			{
				if (trimmed.EndsWith(unit.AsSpan(), StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}

		public override SKSize GetOriginalSize() =>
			svg.Picture.CullRect.Size;

		public override void DrawUnscaled(SKCanvas canvas, float scale)
		{
			if (scale >= 1)
			{
				// draw using default scaling
				canvas.DrawPicture(svg.Picture, Paint);
			}
			else
			{
				// draw using raster downscaling
				var size = GetOriginalSize();

				// vector scaling has rounding issues, so first draw as intended
				var info = new SKImageInfo((int)size.Width, (int)size.Height);
				using var surface = SKSurface.Create(info);
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
