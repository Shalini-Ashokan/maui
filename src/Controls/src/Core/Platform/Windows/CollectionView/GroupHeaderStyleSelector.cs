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

		public GroupHeaderStyleSelector(GridItemsLayout gridItemsLayout)
		{
			_panel = (ItemsPanelTemplate)UWPApp.Current.Resources["VerticalGridItemsPanel"];
			_headerContainerStyle = CreateHeaderContainerStyle(gridItemsLayout);
		}

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
