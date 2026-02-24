using Android.OS;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ControlsAccessibilityDelegate : AccessibilityDelegateCompatWrapper
	{
		public IViewHandler Handler { get; }

		public ControlsAccessibilityDelegate(AccessibilityDelegateCompat? originalDelegate, IViewHandler viewHandler)
			: base(originalDelegate)
		{
			Handler = viewHandler;
		}

		public override void OnInitializeAccessibilityNodeInfo(PlatformView? host, AccessibilityNodeInfoCompat? info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);

			if (Handler?.VirtualView is View v)
				v.UpdateSemanticNodeInfo(info);
		}

		public override bool PerformAccessibilityAction(PlatformView? host, int action, Bundle? args)
		{
			// Handle TalkBack's ACTION_CLICK by firing the MAUI tap gesture directly.
			// When Clickable = true, TalkBack sends ACTION_CLICK (an accessibility action) instead of
			// synthesizing raw MotionEvents. We intercept it here so TapGestureRecognizer fires correctly
			// with TalkBack, while normal touch continues to go through the gesture detector pipeline.
			if (action == AccessibilityNodeInfoCompat.ActionClick &&
				Handler?.VirtualView is View view &&
				view.HasAccessibleTapGesture(out var tapGestureRecognizer))
			{
				if (host?.Enabled == true)
					tapGestureRecognizer.SendTapped(view, (v) => Point.Zero);

				return true;
			}

			return base.PerformAccessibilityAction(host, action, args);
		}
	}
}
