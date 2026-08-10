#nullable disable
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	// Adjusts the accessibility (TalkBack) information reported for a CollectionView's RecyclerView
	// so that synthetic rows -- Header, Footer, and EmptyView -- are not counted as part of the
	// list. Without this, TalkBack announces an inflated item count (e.g. "in list, 3 items" for a
	// list with only a Header and Footer) and announces the Header/Footer/EmptyView as if they were
	// selectable list rows. See https://github.com/dotnet/maui/issues/35681.
	internal class CollectionViewAccessibilityDelegate : RecyclerViewAccessibilityDelegate
	{
		readonly RecyclerView _recyclerView;

		public CollectionViewAccessibilityDelegate(RecyclerView recyclerView) : base(recyclerView)
		{
			_recyclerView = recyclerView;
		}

		public override void OnInitializeAccessibilityNodeInfo(AView host, AccessibilityNodeInfoCompat info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);

			var excludedCount = GetExcludedFromAccessibilityCollectionCount();

			if (excludedCount <= 0)
			{
				return;
			}

			var collectionInfo = info.CollectionInfo;

			if (collectionInfo == null)
			{
				return;
			}

			var rowCount = collectionInfo.RowCount;
			var columnCount = collectionInfo.ColumnCount;

			// LinearLayoutManager reports the item count via RowCount (vertical) or ColumnCount
			// (horizontal); GridLayoutManager reports both. Only adjust whichever dimension is
			// actually tracking the adapter's item count.
			if (rowCount > 1)
			{
				rowCount = System.Math.Max(0, rowCount - excludedCount);
			}
			else if (columnCount > 1)
			{
				columnCount = System.Math.Max(0, columnCount - excludedCount);
			}

			info.SetCollectionInfo(AccessibilityNodeInfoCompat.CollectionInfoCompat.Obtain(
				rowCount, columnCount, collectionInfo.IsHierarchical, collectionInfo.SelectionMode));
		}

		public override AccessibilityDelegateCompat GetItemDelegate()
		{
			return new CollectionViewItemAccessibilityDelegate(this, _recyclerView);
		}

		int GetExcludedFromAccessibilityCollectionCount()
		{
			if (_recyclerView?.GetAdapter() is not IAccessibilityCollectionAdapter adapter)
			{
				return 0;
			}

			var itemCount = _recyclerView.GetAdapter().ItemCount;
			var excluded = 0;

			for (var position = 0; position < itemCount; position++)
			{
				if (adapter.IsExcludedFromAccessibilityCollection(position))
				{
					excluded++;
				}
			}

			return excluded;
		}

		class CollectionViewItemAccessibilityDelegate : ItemDelegate
		{
			readonly RecyclerView _recyclerView;

			public CollectionViewItemAccessibilityDelegate(RecyclerViewAccessibilityDelegate accessibilityDelegate, RecyclerView recyclerView)
				: base(accessibilityDelegate)
			{
				_recyclerView = recyclerView;
			}

			public override void OnInitializeAccessibilityNodeInfo(AView host, AccessibilityNodeInfoCompat info)
			{
				base.OnInitializeAccessibilityNodeInfo(host, info);

				if (_recyclerView?.GetAdapter() is not IAccessibilityCollectionAdapter adapter)
				{
					return;
				}

				var position = _recyclerView.GetChildAdapterPosition(host);

				if (position == RecyclerView.NoPosition)
				{
					return;
				}

				if (adapter.IsExcludedFromAccessibilityCollection(position))
				{
					// Remove the "row X of Y" / list-membership semantics for this row so TalkBack
					// treats the Header/Footer/EmptyView as a standalone view, not a list item.
					info.SetCollectionItemInfo(null);
					return;
				}

				var itemInfo = info.CollectionItemInfo;

				if (itemInfo == null)
				{
					return;
				}

				// The row/column index reported by the platform is based on the raw adapter
				// position, which still includes any excluded rows (Header/EmptyView) before this
				// item. Subtract those out so real items are announced with their index within the
				// actual ItemsSource, not their raw adapter position.
				var excludedBefore = GetExcludedFromAccessibilityCollectionCountBefore(adapter, position);

				if (excludedBefore <= 0)
				{
					return;
				}

				var rowIndex = itemInfo.RowIndex;
				var columnIndex = itemInfo.ColumnIndex;

				if (rowIndex > 0)
				{
					rowIndex = System.Math.Max(0, rowIndex - excludedBefore);
				}

				if (columnIndex > 0)
				{
					columnIndex = System.Math.Max(0, columnIndex - excludedBefore);
				}

				var builder = new AccessibilityNodeInfoCompat.CollectionItemInfoCompat.Builder()
					.SetRowIndex(rowIndex)
					.SetRowSpan(itemInfo.RowSpan)
					.SetColumnIndex(columnIndex)
					.SetColumnSpan(itemInfo.ColumnSpan)
					.SetSelected(itemInfo.IsSelected);

				info.SetCollectionItemInfo(builder.Build());
			}

			static int GetExcludedFromAccessibilityCollectionCountBefore(IAccessibilityCollectionAdapter adapter, int position)
			{
				var excluded = 0;

				for (var i = 0; i < position; i++)
				{
					if (adapter.IsExcludedFromAccessibilityCollection(i))
					{
						excluded++;
					}
				}

				return excluded;
			}
		}
	}
}
