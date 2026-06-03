#nullable disable
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;
namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LineHandler
	{
		public static void MapX1(IShapeViewHandler handler, Line line)
		{
			InvalidateShapeAndLayout(handler, line);
		}
		public static void MapY1(IShapeViewHandler handler, Line line)
		{
			InvalidateShapeAndLayout(handler, line);
		}
		public static void MapX2(IShapeViewHandler handler, Line line)
		{
			InvalidateShapeAndLayout(handler, line);
		}
		public static void MapY2(IShapeViewHandler handler, Line line)
		{
			InvalidateShapeAndLayout(handler, line);
		}
		static void InvalidateShapeAndLayout(IShapeViewHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
			line.InvalidateMeasure();
			if (line.Parent is VisualElement parent)
				parent.InvalidateMeasure();
			// Schedule one extra remeasure to handle the initial layout/binding race on Windows.
			line.Dispatcher?.Dispatch(() =>
			{
				line.InvalidateMeasure();
				if (line.Parent is VisualElement deferredParent)
					deferredParent.InvalidateMeasure();
			});
		}
	}
}
