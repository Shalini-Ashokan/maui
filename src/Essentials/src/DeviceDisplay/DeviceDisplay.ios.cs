#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;
using ObjCRuntime;

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		NSObject? observer;

#if !MACCATALYST
		readonly Dictionary<UIWindowScene, IDisposable> sceneObservers = new();
		NSObject? sceneActivatedObserver;
		NSObject? sceneDisconnectedObserver;
#endif

#if MACCATALYST
		static readonly NSString ScreenParametersChangedNotification =
			new NSString("NSApplicationDidChangeScreenParametersNotification");

		// Core Graphics P/Invoke declarations for Mac Catalyst
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern uint CGMainDisplayID();

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern IntPtr CGDisplayCopyDisplayMode(uint display);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern void CGDisplayModeRelease(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern nuint CGDisplayModeGetWidth(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern nuint CGDisplayModeGetHeight(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern double CGDisplayModeGetRefreshRate(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern double CGDisplayRotation(uint display);

		readonly object locker = new object();
		NSObject? keepScreenOnActivity;

		protected override bool GetKeepScreenOn()
		{
			lock (locker)
			{
				return keepScreenOnActivity is not null;
			}
		}

		protected override void SetKeepScreenOn(bool keepScreenOn)
		{
			lock (locker)
			{
				if ((keepScreenOnActivity is not null) == keepScreenOn)
				{
					return;
				}

				if (keepScreenOn)
				{
					keepScreenOnActivity = NSProcessInfo.ProcessInfo.BeginActivity(
						NSActivityOptions.IdleDisplaySleepDisabled | NSActivityOptions.UserInitiated,
						"KeepScreenOn");
				}
				else
				{
					NSProcessInfo.ProcessInfo.EndActivity(keepScreenOnActivity!);
					keepScreenOnActivity = null;
				}
			}
		}
#else
		protected override bool GetKeepScreenOn() => UIApplication.SharedApplication.IdleTimerDisabled;

		protected override void SetKeepScreenOn(bool keepScreenOn) => UIApplication.SharedApplication.IdleTimerDisabled = keepScreenOn;
#endif

		protected override DisplayInfo GetMainDisplayInfo()
		{
#if MACCATALYST
			// On Mac Catalyst, bypass UIScreen entirely and use Core Graphics APIs
			// This gets fresh, non-cached screen information directly from the system
			// Note: CGMainDisplayID returns the primary display (with menu bar).
			// In multi-monitor setups, this may not be the display the app window is on.
			var displayId = CGMainDisplayID();
			var mode = CGDisplayCopyDisplayMode(displayId);

			if (mode == IntPtr.Zero)
			{
				return GetFallbackDisplayInfo();
			}

			try
			{
				var width = (double)CGDisplayModeGetWidth(mode);
				var height = (double)CGDisplayModeGetHeight(mode);
				var refreshRate = CGDisplayModeGetRefreshRate(mode);

				// Get rotation from Core Graphics
				var rotationDegrees = CGDisplayRotation(displayId);
				var rotation = ConvertRotationDegreesToDisplayRotation(rotationDegrees);

				// Get scale factor from UIScreen as a fallback (this is usually stable)
				var scale = UIScreen.MainScreen.Scale;

				return new DisplayInfo(
					width: width,
					height: height,
					density: scale,
					// Orientation is intentionally hardcoded to Portrait to match Xamarin's Mac Catalyst
					// behavior. Deriving orientation from dimensions/rotation breaks existing tests and
					// Mac desktop apps don't have a meaningful orientation concept.
					orientation: DisplayOrientation.Portrait,
					rotation: rotation,
					rate: (float)refreshRate);
			}
			finally
			{
				CGDisplayModeRelease(mode);
			}
#else
			// iOS implementation
			return GetFallbackDisplayInfo();
#endif
		}

		static DisplayRotation ConvertRotationDegreesToDisplayRotation(double degrees) =>
			degrees switch
			{
				0 => DisplayRotation.Rotation0,
				90 => DisplayRotation.Rotation90,
				180 => DisplayRotation.Rotation180,
				270 => DisplayRotation.Rotation270,
				_ => DisplayRotation.Rotation0
			};

		DisplayInfo GetFallbackDisplayInfo()
		{
			var bounds = UIScreen.MainScreen.Bounds;
			var scale = UIScreen.MainScreen.Scale;
			var interfaceOrientation = GetInterfaceOrientation();

			var rate = (OperatingSystem.IsIOSVersionAtLeast(10, 3) || OperatingSystem.IsMacCatalystVersionAtLeast(10, 3) || OperatingSystem.IsTvOSVersionAtLeast(10, 3))
				? UIScreen.MainScreen.MaximumFramesPerSecond
				: 0;

			return new DisplayInfo(
				width: bounds.Width * scale,
				height: bounds.Height * scale,
				density: scale,
				orientation: CalculateOrientation(interfaceOrientation),
				rotation: CalculateRotation(interfaceOrientation),
				rate: rate);
		}

		protected override void StartScreenMetricsListeners()
		{
			var notificationCenter = NSNotificationCenter.DefaultCenter;

#if MACCATALYST
			// On Mac Catalyst, use multiple notifications to cover all display changes
			// NSApplicationDidChangeScreenParametersNotification - for resolution/refresh rate changes
			observer = notificationCenter.AddObserver(ScreenParametersChangedNotification, OnMainDisplayInfoChanged);
#else
#pragma warning disable CA1416, CA1422 // Retain notifications for older iOS and apps without scenes.
			var notification = UIApplication.DidChangeStatusBarOrientationNotification;
#pragma warning restore CA1416, CA1422
			observer = notificationCenter.AddObserver(notification, OnMainDisplayInfoChanged);

			if (OperatingSystem.IsIOSVersionAtLeast(16))
			{
				sceneActivatedObserver = notificationCenter.AddObserver(UIScene.DidActivateNotification, OnSceneActivated);
				sceneDisconnectedObserver = notificationCenter.AddObserver(UIScene.DidDisconnectNotification, OnSceneDisconnected);

				foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
				{
					if (scene is UIWindowScene windowScene)
						ObserveScene(windowScene);
				}
			}
#endif
		}

		protected override void StopScreenMetricsListeners()
		{
			observer?.Dispose();
			observer = null;

#if !MACCATALYST
			sceneActivatedObserver?.Dispose();
			sceneActivatedObserver = null;
			sceneDisconnectedObserver?.Dispose();
			sceneDisconnectedObserver = null;

			foreach (var sceneObserver in sceneObservers.Values)
				sceneObserver.Dispose();

			sceneObservers.Clear();
#endif
		}

#if !MACCATALYST
		void ObserveScene(UIWindowScene scene)
		{
			if (!sceneObservers.ContainsKey(scene))
			{
				// effectiveGeometry is KVO-compliant and tracks interface rotation, including orientation lock.
				sceneObservers.Add(scene, scene.AddObserver("effectiveGeometry", NSKeyValueObservingOptions.New,
					_ => OnMainDisplayInfoChanged()));
			}
		}

		void OnSceneActivated(NSNotification notification)
		{
			if (notification.Object is UIWindowScene scene)
			{
				ObserveScene(scene);
				OnMainDisplayInfoChanged();
			}
		}

		void OnSceneDisconnected(NSNotification notification)
		{
			if (notification.Object is UIWindowScene scene && sceneObservers.TryGetValue(scene, out var sceneObserver))
			{
				sceneObserver.Dispose();
				sceneObservers.Remove(scene);
			}
		}
#endif

		void OnMainDisplayInfoChanged(NSNotification obj) =>
			OnMainDisplayInfoChanged();

#pragma warning disable CA1416 // UIApplication.StatusBarOrientation has [UnsupportedOSPlatform("ios9.0")]. (Deprecated but still works)
#pragma warning disable CA1422 // Validate platform compatibility
		static UIInterfaceOrientation GetInterfaceOrientation()
		{
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(13) &&
				WindowStateManager.Default.GetCurrentUIWindow()?.WindowScene is UIWindowScene scene)
			{
				return OperatingSystem.IsIOSVersionAtLeast(16)
					? scene.EffectiveGeometry.InterfaceOrientation
					: scene.InterfaceOrientation;
			}
#endif
			return UIApplication.SharedApplication.StatusBarOrientation;
		}
#pragma warning restore CA1422
#pragma warning restore CA1416

		internal static DisplayOrientation CalculateOrientation(UIInterfaceOrientation orientation) =>
			orientation.IsLandscape()
				? DisplayOrientation.Landscape
				: DisplayOrientation.Portrait;

		internal static DisplayRotation CalculateRotation(UIInterfaceOrientation orientation) =>
			orientation switch
			{
				UIInterfaceOrientation.Portrait => DisplayRotation.Rotation0,
				UIInterfaceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
				UIInterfaceOrientation.LandscapeLeft => DisplayRotation.Rotation270,
				UIInterfaceOrientation.LandscapeRight => DisplayRotation.Rotation90,
				_ => DisplayRotation.Unknown,
			};
	}
}