using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Xunit;

#if IOS
using UIKit;
#endif

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("DeviceDisplay")]
	public class DeviceDisplay_Tests
	{
#if IOS
		[Theory]
		[InlineData(UIInterfaceOrientation.Portrait, DisplayOrientation.Portrait, DisplayRotation.Rotation0)]
		[InlineData(UIInterfaceOrientation.PortraitUpsideDown, DisplayOrientation.Portrait, DisplayRotation.Rotation180)]
		[InlineData(UIInterfaceOrientation.LandscapeLeft, DisplayOrientation.Landscape, DisplayRotation.Rotation270)]
		[InlineData(UIInterfaceOrientation.LandscapeRight, DisplayOrientation.Landscape, DisplayRotation.Rotation90)]
		[InlineData(UIInterfaceOrientation.Unknown, DisplayOrientation.Portrait, DisplayRotation.Unknown)]
		public void Interface_Orientation_Maps_To_Display_Info(
			UIInterfaceOrientation interfaceOrientation, DisplayOrientation orientation, DisplayRotation rotation)
		{
			Assert.Equal(orientation, DeviceDisplayImplementation.CalculateOrientation(interfaceOrientation));
			Assert.Equal(rotation, DeviceDisplayImplementation.CalculateRotation(interfaceOrientation));
		}
#endif

		[Fact]
		public Task Screen_Metrics_Are_Not_Null()
		{
			return Utils.OnMainThread(() =>
			{
				var metrics = DeviceDisplay.MainDisplayInfo;

				Assert.True(metrics.Width > 0);
				Assert.True(metrics.Height > 0);
				Assert.True(metrics.Density > 0);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Locks()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Unlocks_Without_Locking()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Locks_Only_Once()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);
				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}
	}
}
