using System;
using Android.Animation;
using Android.Graphics.Drawables;
using Android.Nfc.CardEmulators;
using Android.Widget;
using Microsoft.Maui.Graphics;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ASwitch>
	{
		CheckedChangeListener ChangeListener { get; } = new CheckedChangeListener();
		ValueAnimator? _shadowTrackingAnimator;

		protected override ASwitch CreatePlatformView()
		{
			return new ASwitch(Context);
		}

		protected override void ConnectHandler(ASwitch platformView)
		{
			ChangeListener.Handler = this;
			platformView.SetOnCheckedChangeListener(ChangeListener);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ASwitch platformView)
		{
			ChangeListener.Handler = null;
			platformView.SetOnCheckedChangeListener(null);

			_shadowTrackingAnimator?.Cancel();
			_shadowTrackingAnimator?.Dispose();
			_shadowTrackingAnimator = null;

			base.DisconnectHandler(platformView);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			Size size = base.GetDesiredSize(widthConstraint, heightConstraint);

			if (size.Width == 0)
			{
				int width = (int)widthConstraint;

				if (widthConstraint <= 0)
					width = Context != null ? (int)Context.GetThemeAttributeDp(global::Android.Resource.Attribute.SwitchMinWidth) : 0;

				size = new Size(width, size.Height);
			}

			return size;
		}

		public static void MapIsOn(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateIsOn(view);
		}

		public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler is SwitchHandler platformHandler)
				handler.PlatformView?.UpdateTrackColor(view);
		}

		public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler is SwitchHandler platformHandler)
				handler.PlatformView?.UpdateThumbColor(view);
		}

		void OnCheckedChanged(bool isOn)
		{
			if (VirtualView is null || VirtualView.IsOn == isOn)
				return;

			VirtualView.IsOn = isOn;

			// For switches with shadow, we need to create an animation to track the thumb movement
			// and invalidate the container to update the shadow position during the animation
			if (VirtualView?.Shadow != null && ContainerView != null)
			{
				StartShadowTrackingAnimation();
			}
		}

		void StartShadowTrackingAnimation()
		{
			// Cancel any existing animation
			_shadowTrackingAnimator?.Cancel();

			// Create an animator that matches the switch thumb animation duration
			// SwitchCompat typically uses ~250ms for thumb animation
			_shadowTrackingAnimator = ValueAnimator.OfFloat(0f, 1f);
			if (_shadowTrackingAnimator != null)
			{
				_shadowTrackingAnimator.SetDuration(250);

				_shadowTrackingAnimator.Update += (sender, e) =>
				{
					// Force container redraw during the animation to update shadow
					ContainerView?.Invalidate();
				};

				_shadowTrackingAnimator.AnimationEnd += (sender, e) =>
				{
					// Final invalidation to ensure shadow is in correct position
					ContainerView?.Invalidate();
				};

				_shadowTrackingAnimator.Start();
			}
		}

		class CheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
		{
			public SwitchHandler? Handler { get; set; }

			void CompoundButton.IOnCheckedChangeListener.OnCheckedChanged(CompoundButton? buttonView, bool isToggled)
			{
				Handler?.OnCheckedChanged(isToggled);
			}
		}
	}
}