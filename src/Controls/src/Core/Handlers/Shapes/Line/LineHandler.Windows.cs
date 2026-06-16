#nullable disable
using System;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LineHandler
	{
		public static void MapX1(IShapeViewHandler handler, Line line)
		{
			UpdateLineSize(handler.PlatformView, line);
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapY1(IShapeViewHandler handler, Line line)
		{
			UpdateLineSize(handler.PlatformView, line);
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapX2(IShapeViewHandler handler, Line line)
		{
			UpdateLineSize(handler.PlatformView, line);
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapY2(IShapeViewHandler handler, Line line)
		{
			UpdateLineSize(handler.PlatformView, line);
			handler.PlatformView?.InvalidateShape(line);
		}
		static void UpdateLineSize(W2DGraphicsView platformView, Line line)
		{
			if (platformView == null)
				return;

			// Compute the line's correct size from its current geometry (e.g. X2 just updated).
			// We must call Arrange directly on the platform view — setting Width/Height alone only
			// schedules a layout hint; it does not update ActualWidth/ActualHeight. WinUI's
			// Arrange immediately commits the layout slot, which resizes the inner CanvasControl,
			// which causes Win2D to auto-fire Draw at the correct ActualWidth/ActualHeight.
			var desiredSize = ((IView)line).Measure(double.PositiveInfinity, double.PositiveInfinity);
			var width = Math.Max(1, desiredSize.Width);
			var height = Math.Max(1, desiredSize.Height);

			// ActualOffset is the element's current position relative to its parent after layout.
			var offset = platformView.ActualOffset;
			platformView.Arrange(new global::Windows.Foundation.Rect(offset.X, offset.Y, width, height));
		}
	}
}
