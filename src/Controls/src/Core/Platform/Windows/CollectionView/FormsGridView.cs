#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPControls = Microsoft.UI.Xaml.Controls;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class FormsGridView : GridView, IEmptyView
	{
		int _span;
		// When the CollectionView is grouped, WinUI realizes one ItemsWrapGrid per group (each group hosts
		// its own copy of the ItemsPanelTemplate). Tracking all of them - not just the first one found -
		// ensures every group's grid gets the correct MaximumRowsOrColumns/ItemWidth, instead of only the
		// first group being sized correctly while later groups fall back to WinUI's default wrapping.
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
		}

		public int Span
		{
			get => _span;
			set
			{
				_span = value;
				if (_groupWrapGrids.Count > 0)
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

		// When true, the actual grid wrapping is delegated per-group to GroupStyle.Panel (see
		// GroupHeaderStyleSelector), so the outer panel here must NOT be an ItemsWrapGrid. If it were,
		// WinUI would treat each group's header AND its items-panel as individual cells and wrap them
		// together (e.g. "5 slots per row" would count a header as slot 1, throwing off every item's
		// column for that row) - this is the root cause of the well-known "first item in a grouped grid
		// renders with the wrong size" ItemsWrapGrid bug. Using a plain (non-wrapping) outer panel here
		// means headers and their group content simply stack vertically, and each group's own
		// ItemsWrapGrid (from GroupStyle.Panel) handles the actual grid wrapping in isolation.
		public bool IsGrouped { get; set; }

		public Orientation Orientation
		{
			get => _orientation;
			set
			{
				_orientation = value;

				if (IsGrouped)
				{
					// Groups always stack top-to-bottom regardless of the inner grid's orientation;
					// the real wrapping happens inside each group's own panel (GroupStyle.Panel).
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["GroupedGridItemsPanel"];
				}
				else if (_orientation == Orientation.Horizontal)
				{
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["HorizontalGridItemsPanel"];
				}
				else
				{
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["VerticalGridItemsPanel"];
				}

				if (_orientation == Orientation.Horizontal)
				{
					ScrollViewer.SetHorizontalScrollMode(this, WScrollMode.Auto);
					ScrollViewer.SetHorizontalScrollBarVisibility(this, UWPControls.ScrollBarVisibility.Auto);
				}
			}
		}

		void FindItemsWrapGrid()
		{
			// When grouping is enabled, WinUI realizes a separate ItemsWrapGrid instance for each group
			// (the ItemsPanelTemplate is applied per-group), not a single shared panel. Only patching the
			// first one found (as GetFirstDescendant did previously) leaves every other group's grid
			// unconfigured, causing its row/column layout (span, item spacing) to look wrong. Track every
			// realized wrap grid so each one gets sized consistently.
			foreach (var wrapGrid in _groupWrapGrids)
			{
				wrapGrid.SizeChanged -= WrapGridSizeChanged;
			}

			_groupWrapGrids.Clear();
			_groupWrapGrids.AddRange(this.GetDescendants<ItemsWrapGrid>());

			if (_groupWrapGrids.Count == 0)
			{
				return;
			}

			foreach (var wrapGrid in _groupWrapGrids)
			{
				wrapGrid.SizeChanged += WrapGridSizeChanged;
			}

			UpdateItemSize();
		}

		void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
		{
			FindItemsWrapGrid();
		}

		void WrapGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateItemSize();
		}

		void UpdateItemSize()
		{
			foreach (var wrapGrid in _groupWrapGrids)
			{
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
						wrapGrid.ItemWidth = Math.Floor(wrapGrid.ActualWidth / Span);
					}
					else
					{
						wrapGrid.ClearValue(ItemsWrapGrid.ItemWidthProperty);
					}
				}
			}
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
