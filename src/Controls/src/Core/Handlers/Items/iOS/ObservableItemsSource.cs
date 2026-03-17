#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class ObservableItemsSource : IObservableItemsViewSource
	{
		readonly WeakReference<UICollectionViewController> _collectionViewController;
		readonly bool _grouped;
		readonly int _section;
		readonly IEnumerable _itemsSource;
		readonly Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> _itemPropertyChangedHandlers = [];
		bool _disposed;

		public ObservableItemsSource(IEnumerable itemSource, UICollectionViewController collectionViewController, int group = -1)
		{
			_collectionViewController = new(collectionViewController);

			_section = group < 0 ? 0 : group;
			_grouped = group >= 0;

			_itemsSource = itemSource;

			Count = ItemsCount();

			((INotifyCollectionChanged)itemSource).CollectionChanged += CollectionChanged;
			SubscribeToItemsPropertyChanged(itemSource);
		}

		internal event NotifyCollectionChangedEventHandler CollectionViewUpdating;
		internal event NotifyCollectionChangedEventHandler CollectionViewUpdated;

		internal UICollectionView CollectionView => _collectionViewController.TryGetTarget(out var controller) ? controller.CollectionView : null;

		public int Count { get; private set; }

		public int Section => _section;

		public object this[int index] => ElementAt(index);

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					((INotifyCollectionChanged)_itemsSource).CollectionChanged -= CollectionChanged;
					UnsubscribeFromAllItemsPropertyChanged();
				}

				_disposed = true;
			}
		}

		public int ItemCountInGroup(nint group)
		{
			return Count;
		}

		public object Group(NSIndexPath indexPath)
		{
			return null;
		}

		public IItemsViewSource GroupItemsViewSource(NSIndexPath indexPath)
		{
			return null;
		}

		public NSIndexPath GetIndexForItem(object item)
		{
			for (int n = 0; n < Count; n++)
			{
				if (Equals(this[n], item))
				{
					return NSIndexPath.Create(_section, n);
				}
			}

			return NSIndexPath.Create(-1, -1);
		}

		public int GroupCount => 1;

		public int ItemCount => Count;

		public bool ObserveChanges { get; set; } = true;

		public object this[NSIndexPath indexPath]
		{
			get
			{
				if (indexPath.Section != _section)
				{
					throw new ArgumentOutOfRangeException(nameof(indexPath));
				}

				return this[(int)indexPath.Item];
			}
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (!ObserveChanges)
			{
				return;
			}

			if (!ApplicationModel.MainThread.IsMainThread)
			{
				ApplicationModel.MainThread.BeginInvokeOnMainThread(() => CollectionChanged(args));
			}
			else
			{
				CollectionChanged(args);
			}
		}

		void CollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			if (!_collectionViewController.TryGetTarget(out var controller))
				return;

			// Force UICollectionView to get the internal accounting straight
			var collectionView = controller.CollectionView;
			if (!collectionView.Hidden)
				collectionView.NumberOfItemsInSection(_section);

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					Replace(args);
					break;
				case NotifyCollectionChangedAction.Move:
					Move(args);
					break;
				case NotifyCollectionChangedAction.Reset:
					UnsubscribeFromAllItemsPropertyChanged();
					SubscribeToItemsPropertyChanged(_itemsSource);
					Reload();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void Reload()
		{
			if (!_collectionViewController.TryGetTarget(out var controller))
				return;

			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

			Count = ItemsCount();

			OnCollectionViewUpdating(args);

			var collectionView = controller.CollectionView;
			collectionView.ReloadData();
			collectionView.CollectionViewLayout.InvalidateLayout();

			OnCollectionViewUpdated(args);
		}

		void ReloadItem(object item)
		{
			if (!_collectionViewController.TryGetTarget(out var controller))
				return;

			var indexPath = GetIndexForItem(item);
			if (indexPath.Section < 0 || indexPath.Item < 0)
				return;

			var collectionView = controller.CollectionView;
			if (collectionView.Hidden)
				return;

			collectionView.ReloadItems([indexPath]);
			collectionView.CollectionViewLayout.InvalidateLayout();
		}

		protected virtual NSIndexPath[] CreateIndexesFrom(int startIndex, int count)
		{
			return IndexPathHelpers.GenerateIndexPathRange(_section, startIndex, count);
		}

		private protected virtual bool ShouldReload(NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a ReloadData()
				var startIndex = args.OldStartingIndex;
				if (startIndex < 0)
				{
					return true;
				}
			}
			return false;
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			if (ShouldReload(args))
			{
				SubscribeToItemsPropertyChanged(args.NewItems);
				Reload();
				return;
			}

			var count = args.NewItems.Count;
			Count += count;
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : IndexOf(args.NewItems[0]);
			SubscribeToItemsPropertyChanged(args.NewItems);

			// Queue up the updates to the UICollectionView
			Update(c => c.InsertItems(CreateIndexesFrom(startIndex, count)), args);
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (ShouldReload(args))
			{
				UnsubscribeFromItemsPropertyChanged(args.OldItems);
				Reload();
				return;
			}

			// If we have a start index, we can be more clever about removing the item(s) (and get the nifty animations)
			var count = args.OldItems.Count;
			Count -= count;
			UnsubscribeFromItemsPropertyChanged(args.OldItems);

			Update(c => c.DeleteItems(CreateIndexesFrom(startIndex, count)), args);
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var newCount = args.NewItems.Count;
			UnsubscribeFromItemsPropertyChanged(args.OldItems);
			SubscribeToItemsPropertyChanged(args.NewItems);

			if (newCount == args.OldItems.Count)
			{
				var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : IndexOf(args.NewItems[0]);

				// We are replacing one set of items with a set of equal size; we can do a simple item range update

				Update(c => c.ReloadItems(CreateIndexesFrom(startIndex, newCount)), args);
				return;
			}

			// The original and replacement sets are of unequal size; this means that everything currently in view will 
			// have to be updated. So we just have to use ReloadData and let the UICollectionView update everything
			Reload();
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			if (count == 1)
			{
				// For a single item, we can use MoveItem and get the animation
				var oldPath = NSIndexPath.Create(_section, args.OldStartingIndex);
				var newPath = NSIndexPath.Create(_section, args.NewStartingIndex);

				Update(c => c.MoveItem(oldPath, newPath), args);
				return;
			}

			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + count;

			Update(c => c.ReloadItems(CreateIndexesFrom(start, end)), args);
		}

		void SubscribeToItemsPropertyChanged(IEnumerable items)
		{
			if (items is null)
				return;

			foreach (var item in items)
			{
				SubscribeToItemPropertyChanged(item);
			}
		}

		void UnsubscribeFromItemsPropertyChanged(IEnumerable items)
		{
			if (items is null)
				return;

			foreach (var item in items)
			{
				UnsubscribeFromItemPropertyChanged(item);
			}
		}

		void SubscribeToItemPropertyChanged(object item)
		{
			if (item is not INotifyPropertyChanged notifyPropertyChanged || _itemPropertyChangedHandlers.ContainsKey(notifyPropertyChanged))
				return;

			PropertyChangedEventHandler handler = ItemPropertyChanged;
			_itemPropertyChangedHandlers[notifyPropertyChanged] = handler;
			notifyPropertyChanged.PropertyChanged += handler;
		}

		void UnsubscribeFromItemPropertyChanged(object item)
		{
			if (item is not INotifyPropertyChanged notifyPropertyChanged)
				return;

			if (_itemPropertyChangedHandlers.TryGetValue(notifyPropertyChanged, out var handler))
			{
				notifyPropertyChanged.PropertyChanged -= handler;
				_itemPropertyChangedHandlers.Remove(notifyPropertyChanged);
			}
		}

		void UnsubscribeFromAllItemsPropertyChanged()
		{
			foreach (var itemPropertyChangedHandler in _itemPropertyChangedHandlers)
			{
				itemPropertyChangedHandler.Key.PropertyChanged -= itemPropertyChangedHandler.Value;
			}

			_itemPropertyChangedHandlers.Clear();
		}

		void ItemPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (!ObserveChanges || _disposed)
				return;

			if (!ApplicationModel.MainThread.IsMainThread)
			{
				ApplicationModel.MainThread.BeginInvokeOnMainThread(() => ReloadItem(sender));
			}
			else
			{
				ReloadItem(sender);
			}
		}

		internal int ItemsCount()
		{
			if (_itemsSource is IList list)
				return list.Count;

			int count = 0;
			foreach (var item in _itemsSource)
				count++;
			return count;
		}

		internal object ElementAt(int index)
		{
			if (_itemsSource is IList list)
				return (index >= 0 && index < list.Count) ? list[index] : null;

			int count = 0;
			foreach (var item in _itemsSource)
			{
				if (count == index)
					return item;
				count++;
			}

			return null;
		}

		internal int IndexOf(object item)
		{
			if (_itemsSource is IList list)
				return list.IndexOf(item);

			int count = 0;
			foreach (var i in _itemsSource)
			{
				if (Equals(i, item))
					return count;
				count++;
			}

			return -1;
		}

		void Update(Action<UICollectionView> update, NotifyCollectionChangedEventArgs args)
		{
			if (!_collectionViewController.TryGetTarget(out var controller))
				return;

			var collectionView = controller.CollectionView;
			if (collectionView.Hidden)
			{
				return;
			}

			OnCollectionViewUpdating(args);
			update(collectionView);
			OnCollectionViewUpdated(args);
		}

		void OnCollectionViewUpdating(NotifyCollectionChangedEventArgs args)
		{
			CollectionViewUpdating?.Invoke(this, args);
		}

		void OnCollectionViewUpdated(NotifyCollectionChangedEventArgs args)
		{
			if (!ApplicationModel.MainThread.IsMainThread)
			{
				ApplicationModel.MainThread.BeginInvokeOnMainThread(() => CollectionViewUpdated?.Invoke(this, args));
			}
			else
			{
				CollectionViewUpdated?.Invoke(this, args);
			}
		}
	}
}
