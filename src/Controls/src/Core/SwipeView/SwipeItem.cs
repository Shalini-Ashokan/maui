#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeItem']/Docs/*" />
	public partial class SwipeItem : MenuItem, Controls.ISwipeItem, Maui.ISwipeItemMenuItem
	{
		/// <summary>Bindable property for <see cref="BackgroundColor"/>.</summary>
		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SwipeItem), null, propertyChanged: OnBackgroundColorChanged);

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(SwipeItem), true);

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="//Member[@MemberName='BackgroundColor']/Docs/*" />
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="//Member[@MemberName='IsVisible']/Docs/*" />
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		public event EventHandler<EventArgs> Invoked;

		Paint ISwipeItemMenuItem.Background => new SolidPaint(BackgroundColor);

		Visibility ISwipeItemMenuItem.Visibility => this.IsVisible ? Visibility.Visible : Visibility.Collapsed;

		void Maui.ISwipeItem.OnInvoked()
		{
			if (Command != null && Command.CanExecute(CommandParameter))
				Command.Execute(CommandParameter);

			OnClicked();
			Invoked?.Invoke(this, EventArgs.Empty);
		}

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
		}

		static void OnBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SwipeItem swipeItem)
			{
				// Notify handler that Background property has changed when BackgroundColor changes
				// This ensures the handler's MapBackground method gets called for theme changes
				swipeItem.Handler?.UpdateValue(nameof(IView.Background));
			}
		}
	}
}