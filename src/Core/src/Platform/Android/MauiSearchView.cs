using Android.Content;
using Android.Views;
using Android.Widget;
using Java.IO;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchView : SearchView
	{
		internal MauiAppCompatEditText? _queryEditor;

		public MauiSearchView(Context context) : base(context)
		{
			Initialize();
		}

		void Initialize()
		{
			SetIconifiedByDefault(false);
			MaxWidth = int.MaxValue;

			// The SearchView creates a default EditText, we need to replace it with MauiAppCompatEditText
			// to get the SelectionChanged event
			var defaultEditText = this.GetFirstChildOfType<EditText>();
			if (defaultEditText != null && !(defaultEditText is MauiAppCompatEditText))
			{
				// Get the parent view group
				var parent = defaultEditText.Parent as ViewGroup;
				if (parent != null && Context != null)
				{
					// Get the index and layout params of the default EditText
					var index = parent.IndexOfChild(defaultEditText);
					var layoutParams = defaultEditText.LayoutParameters;

					// Remove the default EditText
					parent.RemoveView(defaultEditText);

					// Create and add MauiAppCompatEditText with the same properties
					_queryEditor = new MauiAppCompatEditText(Context);
					_queryEditor.LayoutParameters = layoutParams;

					// Copy important properties from the original EditText
					_queryEditor.Id = defaultEditText.Id;
					_queryEditor.Text = defaultEditText.Text;
					_queryEditor.Hint = defaultEditText.Hint;
					_queryEditor.InputType = defaultEditText.InputType;

					parent.AddView(_queryEditor, index);
				}
			}
			else if (defaultEditText is MauiAppCompatEditText mauiEditText)
			{
				_queryEditor = mauiEditText;
			}

			if (_queryEditor?.LayoutParameters is LinearLayout.LayoutParams layoutParams2)
			{
				layoutParams2.Height = LinearLayout.LayoutParams.MatchParent;
				layoutParams2.Gravity = GravityFlags.FillVertical;
			}

			var searchCloseButtonIdentifier = Resource.Id.search_close_btn;
			if (searchCloseButtonIdentifier > 0)
			{
				var image = FindViewById<ImageView>(searchCloseButtonIdentifier);

				image?.SetMinimumWidth((int?)Context?.ToPixels(44) ?? 0);
			}
		}
	}
}
