using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		static UIColor DefaultBackgroundColor = UIColor.FromRGBA(120, 120, 128, 40);

		public static void UpdateIsOn(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.SetState(view.IsOn, true);
		}

		public static void UpdateTrackColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view is null)
			{
				return;
			}

			if (UsesNativeLiquidGlassRendering())
			{
				UpdateTrackColorForNativeLiquidGlass(uiSwitch, view);
				return;
			}

			var uIView = GetTrackSubview(uiSwitch);

			if (uIView is null)
			{
				return;
			}

			var trackColor = view.TrackColor?.ToPlatform();

			if (view.IsOn)
			{
				if (trackColor is not null)
				{
					uiSwitch.OnTintColor = trackColor;
					uIView.BackgroundColor = trackColor;
				}
				else
				{
					uiSwitch.OnTintColor = null;
					uIView.BackgroundColor = null;
				}
			}
			else
			{
				if (trackColor is not null)
				{
					uIView.BackgroundColor = trackColor;
				}
				else
				{
					// iOS 13+ uses the UIColor.SecondarySystemFill to support Light and Dark mode
					// else, use the RGBA equivalent of UIColor.SecondarySystemFill in Light mode
					var fallbackColor = OperatingSystem.IsIOSVersionAtLeast(13) ? UIColor.SecondarySystemFill : DefaultBackgroundColor;
					uIView.BackgroundColor = fallbackColor;
				}
			}
		}

		internal static void UpdateTrackColorForNativeLiquidGlass(UISwitch uiSwitch, ISwitch view)
		{
			var trackColor = view.TrackColor?.ToPlatform();

			if (trackColor is null)
			{
				ResetNativeLiquidGlassTrackAppearance(uiSwitch);
				return;
			}

			var cornerRadius = GetTrackCornerRadius(uiSwitch);
			uiSwitch.Layer.CornerRadius = cornerRadius;
			uiSwitch.ClipsToBounds = true;

			if (view.IsOn)
			{
				uiSwitch.OnTintColor = trackColor;
				uiSwitch.TintColor = null;
				uiSwitch.BackgroundColor = null;
			}
			else
			{
				uiSwitch.OnTintColor = trackColor;
				uiSwitch.TintColor = trackColor;
				uiSwitch.BackgroundColor = trackColor;
			}
		}

		internal static void ResetNativeLiquidGlassTrackAppearance(UISwitch uiSwitch)
		{
			uiSwitch.OnTintColor = null;
			uiSwitch.TintColor = null;
			uiSwitch.BackgroundColor = null;
			uiSwitch.Layer.CornerRadius = 0;
			uiSwitch.ClipsToBounds = false;
		}

		public static void UpdateThumbColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			Graphics.Color thumbColor = view.ThumbColor;
			if (thumbColor != null)
				uiSwitch.ThumbTintColor = thumbColor?.ToPlatform();
		}

		internal static UIView? GetTrackSubview(this UISwitch uISwitch)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				return uISwitch.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq();
			else
				return uISwitch.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq();
		}

		internal static UIColor? GetTrackColor(this UISwitch uISwitch)
		{
			return uISwitch.GetTrackSubview()?.BackgroundColor;
		}

		internal static bool ShouldPreserveNativeTrackAppearance(ISwitch view, bool usesNativeLiquidGlassRendering) =>
			usesNativeLiquidGlassRendering && view.TrackColor is null;

		internal static nfloat GetTrackCornerRadius(UISwitch uiSwitch)
		{
			var height = uiSwitch.Bounds.Height;

			if (height <= 0)
				height = uiSwitch.IntrinsicContentSize.Height;

			return height / 2;
		}

		internal static bool UsesNativeLiquidGlassRendering() =>
			OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26);
	}
}
