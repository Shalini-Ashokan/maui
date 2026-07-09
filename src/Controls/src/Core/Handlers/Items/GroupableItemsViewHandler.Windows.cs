#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> : SelectableItemsViewHandler<TItemsView> where TItemsView : GroupableItemsView
	{
		public static void MapIsGrouped(GroupableItemsViewHandler<TItemsView> handler, GroupableItemsView itemsView)
		{
			handler.UpdateItemsSource();
		}

		// Used by StructuredItemsViewHandler.SelectListViewBase (via CreateGridView) to decide whether the
		// outer panel needs special handling to avoid group headers being miscounted as grid cells.
		protected override bool IsGroupedGridLayout => ItemsView != null && ItemsView.IsGrouped && Layout is GridItemsLayout;

		protected override CollectionViewSource CreateCollectionViewSource()
		{
			if (ItemsView != null && ItemsView.IsGrouped)
			{
				var itemTemplate = Element.ItemTemplate;
				var itemsSource = Element.ItemsSource;

				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.CreateGrouped(itemsSource, itemTemplate,
					ItemsView.GroupHeaderTemplate, ItemsView.GroupFooterTemplate, Element, mauiContext: MauiContext),
					IsSourceGrouped = true,
					ItemsPath = new Microsoft.UI.Xaml.PropertyPath(nameof(GroupTemplateContext.Items))
				};
			}
			else
			{
				return base.CreateCollectionViewSource();
			}
		}

		protected override void UpdateItemTemplate()
		{
			base.UpdateItemTemplate();

			// For a grouped GridItemsLayout, the group's items need their own ItemsWrapGrid panel
			// (assigned here via GroupStyle.Panel) so group headers aren't laid out as cells inside the
			// same wrap grid as the items. Without this, WinUI's ItemsWrapGrid counts each header as
			// occupying one of the Span cells in its row-wrap math, which throws off the column
			// assignment for every item in that row (the well-known "first item after a header renders
			// with the wrong size" ItemsWrapGrid grouping bug).
			ListViewBase.GroupStyleSelector = Layout is GridItemsLayout gridItemsLayout
				? new GroupHeaderStyleSelector(gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? Orientation.Horizontal
					: Orientation.Vertical)
				: new GroupHeaderStyleSelector();
		}
	}
}
