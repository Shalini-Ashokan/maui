#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiShapeView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiShapeView;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Graphics.Win2D.W2DGraphicsView;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiShapeView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler : IShapeViewHandler
	{
		public static IPropertyMapper<IShapeView, IShapeViewHandler> Mapper = new PropertyMapper<IShapeView, IShapeViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IShapeView.Background)] = MapBackground,
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(IShapeView.Aspect)] = MapAspect,
			[nameof(IShapeView.Fill)] = MapFill,
			[nameof(IShapeView.Stroke)] = MapStroke,
			[nameof(IShapeView.StrokeThickness)] = MapStrokeThickness,
			[nameof(IShapeView.StrokeDashPattern)] = MapStrokeDashPattern,
			[nameof(IShapeView.StrokeDashOffset)] = MapStrokeDashOffset,
			[nameof(IShapeView.StrokeLineCap)] = MapStrokeLineCap,
			[nameof(IShapeView.StrokeLineJoin)] = MapStrokeLineJoin,
			[nameof(IShapeView.StrokeMiterLimit)] = MapStrokeMiterLimit
		};

		public static CommandMapper<IShapeView, IShapeViewHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public ShapeViewHandler() : base(Mapper, CommandMapper)
		{
		}

		public ShapeViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ShapeViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IShapeView IShapeViewHandler.VirtualView => VirtualView;

		PlatformView IShapeViewHandler.PlatformView => PlatformView;

#if WINDOWS || IOS || MACCATALYST || ANDROID
		public static void MapBackground(IShapeViewHandler handler, IShapeView shapeView)
		{
			// If Fill and Background are not null, will use Fill for the Shape background
			// and Background for the ShapeView background.
			if (shapeView.Background is not null && shapeView.Fill is not null)
			{
				handler.UpdateValue(nameof(IViewHandler.ContainerView));
#if ANDROID
				// Android: When BoxView has both Color and BackgroundColor with Opacity, both colors mix together.
				// This happens because the shape draws on a transparent canvas, allowing BackgroundColor to show through.
				if (handler.GetType().Name == "BoxViewHandler" && shapeView.Fill is SolidPaint solidPaint)
				{
					handler.PlatformView?.BackgroundColor = solidPaint.Color;
				}
				else
				{
					handler.ToPlatform().UpdateBackground(shapeView);
				}
#else
				handler.ToPlatform().UpdateBackground(shapeView);
#endif
			}

			if (shapeView.Background is not null || shapeView.Fill is not null)
			{
				handler.PlatformView?.InvalidateShape(shapeView);
			}
		}
#endif
	}
}