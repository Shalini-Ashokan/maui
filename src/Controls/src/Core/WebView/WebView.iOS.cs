namespace Microsoft.Maui.Controls
{
	public partial class WebView
	{
		public new void Unfocus() => Handler?.Invoke(nameof(IView.Unfocus));
	}
}