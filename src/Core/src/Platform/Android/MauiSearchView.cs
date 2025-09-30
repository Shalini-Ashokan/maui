using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchView : SearchView
	{
		internal MauiAppCompatEditText? _queryEditor;

		public MauiSearchView(Context context) : base(context)
		{
			SetIconifiedByDefault(false);
			MaxWidth = int.MaxValue;

			// Set up the close button width
			var image = FindViewById<ImageView>(Resource.Id.search_close_btn);
			image?.SetMinimumWidth((int?)Context?.ToPixels(44) ?? 0);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			// Replace the built-in EditText with MauiAppCompatEditText
			Post(() => ReplaceEditText());
		}

		void ReplaceEditText()
		{
			if (_queryEditor != null || Context == null)
				return;

			var originalEditText = this.GetFirstChildOfType<EditText>();
			if (originalEditText?.Parent is ViewGroup parent)
			{
				var index = parent.IndexOfChild(originalEditText);
				var layoutParams = originalEditText.LayoutParameters;

				_queryEditor = new MauiAppCompatEditText(Context)
				{
					LayoutParameters = layoutParams,
					Text = originalEditText.Text,
					Hint = originalEditText.Hint,
					InputType = originalEditText.InputType,
					ImeOptions = originalEditText.ImeOptions,
					Id = originalEditText.Id,
					Background = originalEditText.Background
				};

				parent.RemoveViewAt(index);
				parent.AddView(_queryEditor, index);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_queryEditor = null;
			}
			base.Dispose(disposing);
		}
	}
}
