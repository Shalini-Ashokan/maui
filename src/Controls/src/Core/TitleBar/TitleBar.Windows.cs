using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class TitleBar
	{
		internal override void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			base.OnIsVisibleChanged(oldValue, newValue);

			var navRootManager = Handler?.MauiContext?.GetNavigationRootManager();
			navRootManager?.SetTitleBarVisibility(newValue);
		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();
			if (Application.Current is INotifyPropertyChanged propertyChanged)
			{
				propertyChanged.PropertyChanged += PropertyChanged_PropertyChanged;
			}
		}
		
		private void PropertyChanged_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Application.UserAppTheme))
			{ 
				BackgroundColor = Application.Current?.UserAppTheme == ApplicationModel.AppTheme.Light ?
					Colors.White : Colors.Black;
			}
		}
	}
}
