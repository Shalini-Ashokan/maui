#nullable disable
using Microsoft.UI.Xaml.Controls;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPDataTemplate = Microsoft.UI.Xaml.DataTemplate;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class GroupHeaderStyleSelector : GroupStyleSelector
	{
		readonly ItemsPanelTemplate _panel;

		public GroupHeaderStyleSelector()
		{
		}

		// When the grouped items use a GridItemsLayout, each group needs its own ItemsWrapGrid panel so
		// group headers are arranged by the outer (non-wrapping) panel instead of being counted as cells
		// inside the same wrap grid as the items - see GroupableItemsViewHandler.UpdateItemTemplate and
		// FormsGridView.IsGrouped for the full explanation of the underlying WinUI limitation.
		public GroupHeaderStyleSelector(Orientation orientation)
		{
			_panel = (ItemsPanelTemplate)UWPApp.Current.Resources[
				orientation == Orientation.Horizontal ? "HorizontalGridItemsPanel" : "VerticalGridItemsPanel"];
		}

		protected override GroupStyle SelectGroupStyleCore(object group, uint level)
		{
			return new GroupStyle
			{
				HeaderTemplate = (UWPDataTemplate)UWPApp.Current.Resources["GroupHeaderTemplate"],
				Panel = _panel
			};
		}
	}
}
