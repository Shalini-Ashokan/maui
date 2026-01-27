#nullable disable
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() == null)
				radioButton.ControlTemplate = DefaultTemplate;

			radioButton.EnsureTapGestureRecognizerForIOS();
			RadioButtonHandler.MapContent(handler, radioButton);
		}

		void EnsureTapGestureRecognizerForIOS()
		{
			if (ResolveControlTemplate() == null || !IsEnabled)
				return;

			var compositeGestures = ((IGestureController)this).CompositeGestureRecognizers;
			if (_tapGestureRecognizer != null && !compositeGestures.Contains(_tapGestureRecognizer))
			{
				compositeGestures.Add(_tapGestureRecognizer);
			}
		}
	}
}