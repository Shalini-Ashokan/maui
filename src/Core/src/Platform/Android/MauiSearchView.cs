using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Java.IO;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchView : SearchView
	{
		internal MauiAppCompatEditText? _queryEditor;
		internal event EventHandler? SelectionChanged;
		bool _isEditTextReplaced;

		public MauiSearchView(Context context) : base(context)
		{
			Initialize();
		}

		void Initialize()
		{
			SetIconifiedByDefault(false);
			MaxWidth = int.MaxValue;

			var searchCloseButtonIdentifier = Resource.Id.search_close_btn;
			if (searchCloseButtonIdentifier > 0)
			{
				var image = FindViewById<ImageView>(searchCloseButtonIdentifier);
				image?.SetMinimumWidth((int?)Context?.ToPixels(44) ?? 0);
			}
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			// Try to replace EditText after the SearchView is fully attached
			if (!_isEditTextReplaced)
			{
				Post(() => TryReplaceEditText());
			}
		}

		void TryReplaceEditText()
		{
			if (_isEditTextReplaced || Context == null)
				return;

			try
			{
				// Find the original EditText
				var originalEditText = this.GetFirstChildOfType<EditText>();
				if (originalEditText == null)
				{
					// If not found immediately, try again with a slight delay
					PostDelayed(() => TryReplaceEditText(), 50);
					return;
				}

				// Get the parent container
				var parent = originalEditText.Parent as ViewGroup;
				if (parent == null)
					return;

				// Get the index and layout params of the original EditText
				var index = parent.IndexOfChild(originalEditText);
				var layoutParams = originalEditText.LayoutParameters;

				// Create the new MauiAppCompatEditText
				_queryEditor = new MauiAppCompatEditText(Context)
				{
					LayoutParameters = layoutParams,
					Text = originalEditText.Text,
					Hint = originalEditText.Hint,
					InputType = originalEditText.InputType,
					ImeOptions = originalEditText.ImeOptions,
					Id = originalEditText.Id // Important: preserve the ID
				};

				// Copy other important properties
				if (originalEditText.Background != null)
					_queryEditor.Background = originalEditText.Background;

				// Copy accessibility properties to avoid null pointer exceptions
				_queryEditor.ImportantForAccessibility = originalEditText.ImportantForAccessibility;

				// Subscribe to SelectionChanged event
				_queryEditor.SelectionChanged += OnEditTextSelectionChanged;

				// Remove the original EditText and add the new one
				parent.RemoveViewAt(index);
				parent.AddView(_queryEditor, index);

				// Ensure proper layout
				if (_queryEditor.LayoutParameters is LinearLayout.LayoutParams linLayoutParams)
				{
					linLayoutParams.Height = LinearLayout.LayoutParams.MatchParent;
					linLayoutParams.Gravity = GravityFlags.FillVertical;
				}

				_isEditTextReplaced = true;
			}
			catch (Exception ex)
			{
				// Log the exception and fall back to using the original EditText
				System.Diagnostics.Debug.WriteLine($"Failed to replace SearchView EditText: {ex.Message}");
				_queryEditor = this.GetFirstChildOfType<EditText>() as MauiAppCompatEditText;
			}
		}

		void OnEditTextSelectionChanged(object? sender, EventArgs e)
		{
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _queryEditor != null)
			{
				_queryEditor.SelectionChanged -= OnEditTextSelectionChanged;
			}
			base.Dispose(disposing);
		}

		// Public method to ensure EditText is available (can be called from handler)
		public void EnsureEditTextReplaced()
		{
			if (!_isEditTextReplaced)
			{
				TryReplaceEditText();
			}
		}
	}
}
