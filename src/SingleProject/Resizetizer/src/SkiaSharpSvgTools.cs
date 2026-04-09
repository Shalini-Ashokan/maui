using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SkiaSharp;
using Svg.Skia;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpSvgTools : SkiaSharpTools, IDisposable
	{
		// Matches commonly used relative units in SVG width/height: em, ex, rem, %, vw, vh
		static readonly Regex RelativeUnitPattern = new Regex(
			@"^\s*[-+]?\d*\.?\d+\s*(em|ex|rem|%|vw|vh)\s*$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		SKSvg svg;

		public SkiaSharpSvgTools(ResizeImageInfo info, ILogger logger)
		 : this(info.Filename, info.BaseSize, info.Color, info.TintColor, logger)
		{
		}

		public SkiaSharpSvgTools(string filename, SKSize? baseSize, SKColor? backgroundColor, SKColor? tintColor, ILogger logger)
		 : base(filename, baseSize, backgroundColor, tintColor, logger)
		{
			var sw = Stopwatch.StartNew();

			svg = new SKSvg();

			using var stream = PreprocessSvgToStream(filename);
			var pic = stream != null
			 ? svg.Load(stream)
			 : svg.Load(filename);

			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms ({filename})");

			if (pic.CullRect.Size.IsEmpty)
				Logger?.Log($"SVG picture did not have a size and will fail to generate. ({Filename})");
		}

		/// <summary>
		/// If the SVG width or height uses a common relative unit (em, ex, rem, %, vw, vh),
		/// strips those attributes so SkiaSharp can render correctly.
		/// Returns null when no preprocessing is needed.
		/// </summary>
		MemoryStream PreprocessSvgToStream(string filename)
		{
			try
			{
				var doc = XDocument.Load(filename);
				var root = doc.Root;

				if (root == null)
					return null;

				var widthAttr = root.Attribute("width");
				var heightAttr = root.Attribute("height");

				bool widthHasEm = widthAttr != null && RelativeUnitPattern.IsMatch(widthAttr.Value);
				bool heightHasEm = heightAttr != null && RelativeUnitPattern.IsMatch(heightAttr.Value);

				if (!widthHasEm && !heightHasEm)
					return null;

				if (widthHasEm)
					widthAttr.Remove();

				if (heightHasEm)
					heightAttr.Remove();

				var ms = new MemoryStream();
				doc.Save(ms);
				ms.Position = 0;

				return ms;
			}
			catch (Exception ex)
			{
				Logger?.Log($"Failed to preprocess SVG, loading original: {ex.Message}");
				return null;
			}
		}

		public override SKSize GetOriginalSize() =>
		 svg.Picture.CullRect.Size;

		public override void DrawUnscaled(SKCanvas canvas, float scale)
		{
			var size = GetOriginalSize();
			if (size.IsEmpty)
			{
				throw new InvalidOperationException(
				 $"Cannot draw SVG file '{Filename}'. The SVG has no size. " +
				 "Ensure the SVG includes a viewBox or valid width/height.");
			}

			if (scale >= 1)
			{
				// draw using default scaling
				canvas.DrawPicture(svg.Picture, Paint);
			}
			else
			{
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