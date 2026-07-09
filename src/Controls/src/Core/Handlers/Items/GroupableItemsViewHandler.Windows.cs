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

			// Give the group header a HeaderContainerStyle whose margin is derived from the layout's
			// HorizontalItemSpacing/VerticalItemSpacing (only meaningful for grouped GridItemsLayout) so
			// header-to-item spacing matches item-to-item spacing - see GroupHeaderStyleSelector.
			ApplyGroupHeaderStyleSelector();
		}

		// Refreshes the group header's container style (spacing) whenever HorizontalItemSpacing or
		// VerticalItemSpacing change at runtime - see GroupHeaderStyleSelector for why the header's
		// margin needs to be derived from the same values used for item margins.
		protected override void UpdateGroupHeaderContainerSpacing() => ApplyGroupHeaderStyleSelector();

		void ApplyGroupHeaderStyleSelector()
		{
			ListViewBase.GroupStyleSelector = ItemsView != null && ItemsView.IsGrouped && Layout is GridItemsLayout gridItemsLayout
				? new GroupHeaderStyleSelector(gridItemsLayout)
				: new GroupHeaderStyleSelector();
		}
	}
}
