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
		// CSS relative length units that require a rendering context (font metrics
		// or viewport size) and cannot be resolved at build time.
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

			// Strip width/height with relative CSS units (em, rem, etc.) before loading,
			// as Svg.Skia cannot resolve them at build time and produces wrong dimensions.
			using var sanitized = SanitizeSvgRelativeUnits(filename);
			var pic = sanitized != null
				? svg.Load(sanitized)
				: svg.Load(filename);

			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms ({filename})");

			if (pic.CullRect.Size.IsEmpty)
				Logger?.Log($"SVG picture did not have a size and will fail to generate. ({Filename})");
		}

		/// <summary>
		/// If the root &lt;svg&gt; element has width/height with relative CSS units
		/// (em, rem, etc.) and a viewBox is present, returns a stream with those
		/// attributes removed. Returns <c>null</c> when no changes are needed.
		/// </summary>
		internal static MemoryStream SanitizeSvgRelativeUnits(string filename)
		{
			// Read the file once as text for both the quick pre-check and XML parsing.
			var svgContent = File.ReadAllText(filename);

			// Quick text scan: skip XML parsing entirely if no relative unit
			// string appears anywhere in the file (covers the vast majority of SVGs).
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

			// Only parse XML when we suspect relative units are present.
			var doc = XDocument.Parse(svgContent);
			var root = doc.Root;

			if (root == null || root.Name.LocalName != "svg")
				return null;

			var widthAttr = root.Attribute("width");
			var heightAttr = root.Attribute("height");

			if (widthAttr == null && heightAttr == null)
				return null;

			bool widthIsRelative = widthAttr != null && HasRelativeUnit(widthAttr.Value);
			bool heightIsRelative = heightAttr != null && HasRelativeUnit(heightAttr.Value);

			if (!widthIsRelative && !heightIsRelative)
				return null;

			// A viewBox must exist as fallback for dimensions
			var viewBoxAttr = root.Attribute("viewBox");
			if (viewBoxAttr == null || string.IsNullOrWhiteSpace(viewBoxAttr.Value))
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
