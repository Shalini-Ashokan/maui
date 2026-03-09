#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateText(this UILabel platformLabel, Label label)
		{
			var text = TextTransformUtilities.GetTransformedText(label.Text, label.TextTransform);

			switch (label.TextType)
			{
				case TextType.Html:
					if (Foundation.NSThread.IsMain)
					{
						// We are already on the main thread: set HTML text synchronously so that
						// the first Measure() call returns the correct height.
						// https://github.com/dotnet/maui/issues/31674
						platformLabel.UpdateTextHtml(text);
						// MapFormatting will be applied by the caller (Label.MapText) once we return.
					}
					else
					{
						// NSAttributedString with HTML cannot be created on a background thread.
						// https://github.com/dotnet/maui/issues/25946
						// Dispatch to the main thread asynchronously.
						CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
						{
							platformLabel.UpdateTextHtml(text);

							if (label.Handler is LabelHandler labelHandler)
								Label.MapFormatting(labelHandler, label);

							// NOTE: Because we are updating text outside the normal layout
							// pass, we need to invalidate the measure for the next pass.
							label.InvalidateMeasure();
						});
					}
					break;

				default:
					if (label.FormattedText != null)
						platformLabel.AttributedText = label.ToNSAttributedString();
					else
					{
						if (platformLabel.AttributedText is not null)
						{
							platformLabel.AttributedText = null;
						}

						platformLabel.Text = text;
					}
					break;
			}
		}
	}
}
