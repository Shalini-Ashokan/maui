#nullable disable
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WSetter = Microsoft.UI.Xaml.Setter;
using WStyle = Microsoft.UI.Xaml.Style;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class GroupHeaderStyleSelector : GroupStyleSelector
	{
		readonly ItemsPanelTemplate _panel;
		readonly WStyle _headerContainerStyle;

		public GroupHeaderStyleSelector()
		{
		}

		// When the grouped items use a GridItemsLayout, each group needs its own ItemsWrapGrid panel so
		// group headers are arranged by the outer (non-wrapping) panel instead of being counted as cells
		// inside the same wrap grid as the items - see GroupableItemsViewHandler.UpdateItemTemplate and
		// FormsGridView.IsGrouped for the full explanation of the underlying WinUI limitation.
		public GroupHeaderStyleSelector(GridItemsLayout gridItemsLayout)
		{
			var orientation = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
				? Orientation.Horizontal
				: Orientation.Vertical;

			_panel = (ItemsPanelTemplate)UWPApp.Current.Resources[
				orientation == Orientation.Horizontal ? "HorizontalGridItemsPanel" : "VerticalGridItemsPanel"];

			_headerContainerStyle = CreateHeaderContainerStyle(gridItemsLayout);
		}

		// GetItemContainerStyle (StructuredItemsViewHandler.Windows.cs) gives every GridViewItem a
		// margin of (h, v, h, v) - i.e. each item contributes v/h on ALL of its own edges. That means
		// the gap between two adjacent items is v (bottom margin of the first) + v (top margin of the
		// second) = 2v. The default GridViewHeaderItem style hardcodes Margin="0,0,0,4", which is
		// completely unrelated to VerticalItemSpacing/HorizontalItemSpacing, so the gap between a group
		// header and the first item ends up as only ~4 + v instead of 2v - i.e. roughly half the
		// spacing seen between ordinary items. Building the header's margin from the same h/v values
		// used for items keeps header-to-item spacing consistent with item-to-item spacing.
		static WStyle CreateHeaderContainerStyle(GridItemsLayout gridItemsLayout)
		{
			var h = gridItemsLayout?.HorizontalItemSpacing ?? 0;
			var v = gridItemsLayout?.VerticalItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(h, v, h, v);

			var baseStyle = UWPApp.Current.Resources[typeof(GridViewHeaderItem)] as WStyle;

			var style = new WStyle(typeof(GridViewHeaderItem));
			if (baseStyle is not null)
			{
				style.BasedOn = baseStyle;
			}

			style.Setters.Add(new WSetter(FrameworkElement.MarginProperty, margin));

			return style;
		}

		protected override GroupStyle SelectGroupStyleCore(object group, uint level)
		{
			return new GroupStyle
			{
				HeaderTemplate = (UWPDataTemplate)UWPApp.Current.Resources["GroupHeaderTemplate"],
				Panel = _panel,
				HeaderContainerStyle = _headerContainerStyle
			};
		}
	}
}
