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
		// Precise match for relative units (same as v1)
		static readonly Regex NonAbsoluteUnitPattern = new Regex(
		 @"^\s*[-+]?\d*\.?\d+(?:[eE][-+]?\d+)?\s*(em|ex|rem|ch|vw|vh|vmin|vmax|cap|ic|lh|rlh|%)\s*$",
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
		/// Hybrid preprocessing:
		/// - Uses Regex for accurate detection
		/// - Uses MemoryStream (no temp files)
		/// - Only parses XML when necessary
		/// </summary>
		MemoryStream PreprocessSvgToStream(string filename)
		{
			try
			{
				var doc = XDocument.Load(filename);
				var root = doc.Root;

				if (root == null)
				{
					return null;
				}

				var widthAttr = root.Attribute("width");
				var heightAttr = root.Attribute("height");

				if (widthAttr == null && heightAttr == null)
				{
					return null;
				}

				bool widthHasRelativeUnit = widthAttr != null && NonAbsoluteUnitPattern.IsMatch(widthAttr.Value);
				bool heightHasRelativeUnit = heightAttr != null && NonAbsoluteUnitPattern.IsMatch(heightAttr.Value);

				if (!widthHasRelativeUnit && !heightHasRelativeUnit)
				{
					return null;
				}

				var viewBox = root.Attribute("viewBox");
				if (viewBox == null || string.IsNullOrWhiteSpace(viewBox.Value))
				{
					Logger?.Log($"SVG has relative units but no viewBox — cannot preprocess. ({filename})");
					return null;
				}

				Logger?.Log($"SVG has relative width/height (e.g. 'em'); stripping in favour of viewBox. ({filename})");

				if (widthHasRelativeUnit)
				{
					widthAttr.Remove();
				}

				if (heightHasRelativeUnit)
				{
					heightAttr.Remove();
				}

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