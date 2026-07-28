#nullable enable
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	// A minimal Panel used purely to host Border content for clipping purposes (see
	// ContentPanel.UpdateClip). It intentionally does NOT measure/arrange its child itself -
	// Content continues to be measured/arranged directly by the cross-platform layout system,
	// exactly as when it was a direct child of ContentPanel. Because this host never has a
	// RenderTransform of its own, ContentPanel can apply the Border's geometric Clip to this
	// host's visual and rely on it staying fixed to the Border's real bounds, regardless of any
	// Scale/Rotation/Translation RenderTransform applied to Content itself - and regardless of the
	// shape being clipped to (rectangle, rounded rectangle, or an arbitrary custom IShape such as a
	// Polygon/triangle). No per-shape scale-compensation math is needed at all: a parent visual's
	// Clip always bounds everything its descendants render.
	sealed partial class ContentClipHost : Panel
	{
		// Lets Content (e.g. a LayoutPanel) walk back up to the owning ContentPanel even though
		// this host - not ContentPanel itself - is now its direct visual parent. See
		// LayoutPanel.ArrangeOverride, which relies on this to avoid redundantly self-clipping
		// when ContentPanel is already clipping to the Border's shape.
		internal ContentPanel? Owner { get; set; }

		UIElementCollection? _cachedChildren;

		[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Panel.Children property is banned to enforce use of this CachedChildren property.")]
		internal UIElementCollection CachedChildren
		{
			get
			{
				_cachedChildren ??= Children;
				return _cachedChildren;
			}
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			// No-op: Content is measured externally by the cross-platform layout system.
			return availableSize;
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			// No-op: Content is arranged externally by the cross-platform layout system. Arranging
			// it again here (as the base Panel implementation would) would override the exact
			// alignment-driven rect the cross-platform layout system already assigned to Content.
			return finalSize;
		}
	}
}
