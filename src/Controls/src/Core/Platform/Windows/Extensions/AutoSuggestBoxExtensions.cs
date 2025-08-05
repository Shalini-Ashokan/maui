using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class AutoSuggestBoxExtensions
	{
		public static void UpdateText(this AutoSuggestBox platformControl, InputView inputView)
		{
			platformControl.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}

		internal static void SyncCursorPositionFromPlatformToVirtual(this AutoSuggestBox platformView, ISearchBar virtualView)
		{
			if (virtualView == null || platformView == null)
				return;

			var textBox = platformView.GetFirstDescendant<TextBox>();
			if (textBox != null)
			{
				var cursorPosition = textBox.GetCursorPosition();
				if (virtualView.CursorPosition != cursorPosition)
				{
					virtualView.CursorPosition = cursorPosition;
				}
			}
		}
	}
}
