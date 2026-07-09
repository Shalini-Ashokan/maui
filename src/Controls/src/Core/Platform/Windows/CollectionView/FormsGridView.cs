#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPControls = Microsoft.UI.Xaml.Controls;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class FormsGridView : GridView, IEmptyView
	{
		int _span;
		ItemsWrapGrid _wrapGrid;
		readonly List<ItemsWrapGrid> _groupWrapGrids = new();
		ContentControl _emptyViewContentControl;
		ScrollViewer _scrollViewer;
		FrameworkElement _emptyView;
		View _formsEmptyView;
		Orientation _orientation;

		public FormsGridView()
		{
			// Using the full style for this control, because for some reason on 16299 we can't set the ControlTemplate
			// (it just fails silently saying it can't find the resource key)
			DefaultStyleKey = typeof(FormsGridView);

			RegisterPropertyChangedCallback(ItemsPanelProperty, ItemsPanelChanged);

			ChoosingItemContainer += OnChoosingItemContainer;
			ContainerContentChanging += OnContainerContentChanging;
		}

		public int Span
		{
			get => _span;
			set
			{
				_span = value;
				if (_wrapGrid != null)
				{
					UpdateItemSize();
				}
			}
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility),
				typeof(FormsGridView), new PropertyMetadata(WVisibility.Collapsed, EmptyViewVisibilityChanged));

		static void EmptyViewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FormsGridView gridView)
			{
				// Update this manually; normally we'd just bind this, but TemplateBinding doesn't seem to work
				// for WASDK right now.
				gridView.UpdateEmptyViewVisibility((WVisibility)e.NewValue);
			}
		}

		public WVisibility EmptyViewVisibility
		{
			get { return (WVisibility)GetValue(EmptyViewVisibilityProperty); }
			set { SetValue(EmptyViewVisibilityProperty, value); }
		}

		public Orientation Orientation
		{
			get => _orientation;
			set
			{
				_orientation = value;
				if (IsGroupedVerticalGrid)
				{
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["GroupedGridItemsPanel"];
				}
				else if (_orientation == Orientation.Horizontal)
				{
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["HorizontalGridItemsPanel"];
					ScrollViewer.SetHorizontalScrollMode(this, WScrollMode.Auto);
					ScrollViewer.SetHorizontalScrollBarVisibility(this, UWPControls.ScrollBarVisibility.Auto);
				}
				else
				{
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["VerticalGridItemsPanel"];
				}
			}
		}

		public bool IsGroupedVerticalGrid { get; set; }

		void FindItemsWrapGrid()
		{
			if (IsGroupedVerticalGrid)
			{
				FindGroupedWrapGrids();
				return;
			}

			_wrapGrid = this.GetFirstDescendant<ItemsWrapGrid>();

			if (_wrapGrid == null)
			{
				return;
			}

			_wrapGrid.SizeChanged -= WrapGridSizeChanged;
			_wrapGrid.SizeChanged += WrapGridSizeChanged;

			UpdateItemSize();
		}

		void FindGroupedWrapGrids()
		{
			_groupWrapGrids.Clear();

			CollectItemsWrapGrids(this, _groupWrapGrids);

			if (_groupWrapGrids.Count == 0)
			{
				return;
			}

			foreach (var wrapGrid in _groupWrapGrids)
			{
				wrapGrid.SizeChanged -= WrapGridSizeChanged;
				wrapGrid.SizeChanged += WrapGridSizeChanged;
			}

			_wrapGrid = _groupWrapGrids[0];
			UpdateItemSize();
		}

		static void CollectItemsWrapGrids(DependencyObject root, List<ItemsWrapGrid> result)
		{
			var count = VisualTreeHelper.GetChildrenCount(root);

			for (var i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild(root, i);

				if (child is ItemsWrapGrid wrapGrid)
				{
					result.Add(wrapGrid);
				}

				CollectItemsWrapGrids(child, result);
			}
		}

		void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
		{
			FindItemsWrapGrid();
		}

		void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (IsGroupedVerticalGrid)
			{
				FindGroupedWrapGrids();
			}
		}

		void WrapGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateItemSize();
		}

		void UpdateItemSize()
		{
			if (IsGroupedVerticalGrid)
			{
				foreach (var wrapGrid in _groupWrapGrids)
				{
					UpdateWrapGridSize(wrapGrid);
				}

				return;
			}

			UpdateWrapGridSize(_wrapGrid);
		}

		void UpdateWrapGridSize(ItemsWrapGrid wrapGrid)
		{
			if (wrapGrid == null)
			{
				return;
			}

			// Avoid the ItemWrapGrid grow beyond what this grid view is configured to
			wrapGrid.MaximumRowsOrColumns = Span;

			if (_orientation == Orientation.Horizontal)
			{
				wrapGrid.ItemHeight = Math.Floor(wrapGrid.ActualHeight / Span);
			}
			else
			{
				if (Span > 1)
				{
					// During first realization (especially grouped grids), ActualWidth can still be 0.
					// Writing ItemWidth=0 locks the first realized item into a tiny size. Use a valid
					// panel/control width when available; otherwise keep ItemWidth unset until layout settles.
					var width = wrapGrid.ActualWidth;

					if (width <= 0 || double.IsNaN(width) || double.IsInfinity(width))
					{
						width = ActualWidth;
					}

					if (width > 0 && !double.IsNaN(width) && !double.IsInfinity(width))
					{
						wrapGrid.ItemWidth = Math.Floor(width / Span);
					}
					else
					{
						wrapGrid.ClearValue(ItemsWrapGrid.ItemWidthProperty);
					}
				}
				else
				{
					wrapGrid.ClearValue(ItemsWrapGrid.ItemWidthProperty);
				}
			}

			wrapGrid.InvalidateMeasure();
			wrapGrid.InvalidateArrange();
		}

		void ItemsPanelChanged(DependencyObject sender, DependencyProperty dp)
		{
			FindItemsWrapGrid();
		}

		public void SetEmptyView(FrameworkElement emptyView, View formsEmptyView)
		{
			_emptyView = emptyView;
			_formsEmptyView = formsEmptyView;

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;

			_scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;

			if (_emptyView != null && _emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			_formsEmptyView?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

			return base.ArrangeOverride(finalSize);
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			GroupFooterItemTemplateContext.EnsureSelectionDisabled(element, item);
			base.PrepareContainerForItemOverride(element, item);
		}

		void UpdateEmptyViewVisibility(WVisibility visibility)
		{
			if (_emptyViewContentControl is null)
			{
				return;
			}

			// Adjust the ScrollViewer's hit test visibility if it exists
			if (_scrollViewer is not null)
			{
				// When the empty view is visible, disable hit testing for the ScrollViewer.
				// This ensures that interactions are directed to the empty view instead of the ScrollViewer.
				// In the template, the empty view is placed below the ScrollViewer in the visual tree.
				_scrollViewer.IsHitTestVisible = visibility != WVisibility.Visible;
			}

			_emptyViewContentControl.Visibility = visibility;
		}
	}
}
