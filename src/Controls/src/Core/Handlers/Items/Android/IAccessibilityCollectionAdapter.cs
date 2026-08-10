#nullable disable
namespace Microsoft.Maui.Controls.Handlers.Items
{
	// Implemented by RecyclerView.Adapter subclasses that mix synthetic rows (Header, Footer,
	// EmptyView) into the adapter alongside the real data items. TalkBack/accessibility services
	// derive the "in list, N items" announcement and the "row X of Y" per-item announcement
	// directly from the adapter's item count and position, so these synthetic rows must be
	// excluded to avoid misleading announcements. See https://github.com/dotnet/maui/issues/35681.
	internal interface IAccessibilityCollectionAdapter
	{
		// True if the item at `position` is a Header, Footer, or EmptyView row rather than
		// a real data item, and should therefore be excluded from the collection's accessibility
		// row/column count and from having per-item CollectionItemInfo reported.
		bool IsExcludedFromAccessibilityCollection(int position);
	}
}
