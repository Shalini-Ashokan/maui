#nullable disable
using System;
using Android.Views.Accessibility;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;
using Info = AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat.CollectionInfoCompat;
using ItemInfo = AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat.CollectionItemInfoCompat;

namespace Microsoft.Maui.Controls.Handlers.Items
{
    // Excludes Header/Footer/EmptyView from TalkBack's list count and per-item index. See dotnet/maui#35681.
    internal sealed class CollectionViewAccessibilityDelegate : RecyclerViewAccessibilityDelegate
    {
        readonly RecyclerView _rv;
        readonly ItemDelegateImpl _itemDelegate;

        public CollectionViewAccessibilityDelegate(RecyclerView rv) : base(rv)
            => (_rv, _itemDelegate) = (rv, new ItemDelegateImpl(this));

        public override AccessibilityDelegateCompat GetItemDelegate() => _itemDelegate;

        public override void OnInitializeAccessibilityNodeInfo(AView host, AccessibilityNodeInfoCompat info)
        {
            base.OnInitializeAccessibilityNodeInfo(host, info);

            var excluded = ExcludedCount(_rv.GetAdapter());
            var old = info?.CollectionInfo;
            if (excluded <= 0 || old is null)
                return;

            var h = IsHorizontal(_rv);
            info.SetCollectionInfo(Info.Obtain(
                h ? old.RowCount : Math.Max(0, old.RowCount - excluded),
                h ? Math.Max(0, old.ColumnCount - excluded) : old.ColumnCount,
                old.IsHierarchical, old.SelectionMode));
        }

        public override void OnInitializeAccessibilityEvent(AView host, AccessibilityEvent e)
        {
            base.OnInitializeAccessibilityEvent(host, e);
            if (e is not null)
                e.ItemCount = Math.Max(0, e.ItemCount - ExcludedCount(_rv.GetAdapter()));
        }

        static int ExcludedCount(RecyclerView.Adapter a)
        {
            if (a is null || a.ItemCount == 0)
                return 0;
            if (a is EmptyViewAdapter)
                return a.ItemCount;
            var last = a.ItemCount - 1;
            return (a.GetItemViewType(0) == ItemViewType.Header ? 1 : 0)
                 + (last > 0 && a.GetItemViewType(last) == ItemViewType.Footer ? 1 : 0);
        }

        static int HeaderOffset(RecyclerView.Adapter a) =>
            a is null or EmptyViewAdapter || a.ItemCount == 0 ? 0
                : a.GetItemViewType(0) == ItemViewType.Header ? 1 : 0;

        static bool IsHorizontal(RecyclerView rv) =>
            rv.GetLayoutManager() is LinearLayoutManager lm && lm.Orientation == LinearLayoutManager.Horizontal;

        sealed class ItemDelegateImpl : ItemDelegate
        {
            readonly CollectionViewAccessibilityDelegate _parent;
            public ItemDelegateImpl(CollectionViewAccessibilityDelegate p) : base(p) => _parent = p;

            public override void OnInitializeAccessibilityNodeInfo(AView host, AccessibilityNodeInfoCompat info)
            {
                base.OnInitializeAccessibilityNodeInfo(host, info);

                var rv = _parent._rv;
                var a = rv.GetAdapter();
                if (host is null || info is null || a is null)
                    return;

                var pos = rv.GetChildAdapterPosition(host);
                if (pos == RecyclerView.NoPosition)
                    return;

                if (a is EmptyViewAdapter || a.GetItemViewType(pos) is ItemViewType.Header or ItemViewType.Footer)
                {
                    info.SetCollectionItemInfo(null);
                    return;
                }

                var offset = HeaderOffset(a);
                var old = info.CollectionItemInfo;
                if (offset == 0 || old is null)
                    return;

                var h = IsHorizontal(rv);
                info.SetCollectionItemInfo(ItemInfo.Obtain(
                    h ? old.RowIndex : Math.Max(0, old.RowIndex - offset), old.RowSpan,
                    h ? Math.Max(0, old.ColumnIndex - offset) : old.ColumnIndex, old.ColumnSpan,
                    false, old.IsSelected));
            }
        }
    }
}
