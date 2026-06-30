using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Maui.Controls.Sample;

internal sealed record MemorySnapshot(long ManagedBytes, long GcHeapBytes, long ResidentBytes, long WorkingSetBytes)
{
    public static MemorySnapshot Empty { get; } = new(0, 0, 0, 0);
}

internal static class MemorySampler
{
    public static async Task ForceFullCollectionAsync()
    {
        await Task.Yield();

        // Android's ART GC is concurrent and generational — objects with JNI counterparts
        // (platform views) need more passes and time to be fully released. Use more passes
        // and a longer inter-pass delay on Android to get an accurate measurement.
#if ANDROID
        const int passes = 4;
        const int delayMs = 35;
#else
        const int passes = 3;
        const int delayMs = 25;
#endif

        for (var i = 0; i < passes; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            await Task.Delay(delayMs);
        }
    }

    public static async Task<MemorySnapshot> TakeAfterCollectionAsync()
    {
        await ForceFullCollectionAsync();

        return new MemorySnapshot(
            GC.GetTotalMemory(forceFullCollection: false),
            GC.GetGCMemoryInfo().HeapSizeBytes,
            GetResidentMemoryBytes(),
            GetWorkingSetBytes());
    }

    static long GetWorkingSetBytes()
    {
        try
        {
            return Process.GetCurrentProcess().WorkingSet64;
        }
        catch
        {
            return 0;
        }
    }

    static long GetResidentMemoryBytes()
    {
#if ANDROID
		try
		{
			var memoryInfo = new Android.OS.Debug.MemoryInfo();
			Android.OS.Debug.GetMemoryInfo(memoryInfo);
			return memoryInfo.TotalPss * 1024L;
		}
		catch
		{
		}
#endif

#if IOS || MACCATALYST
        try
        {
            var info = new MachTaskBasicInfo();
            var count = (uint)(Marshal.SizeOf<MachTaskBasicInfo>() / sizeof(int));

            if (task_info(mach_task_self(), MachTaskBasicInfoFlavor, out info, ref count) == 0)
                return info.ResidentSize.ToInt64();
        }
        catch
        {
        }
#endif

        return GetWorkingSetBytes();
    }

#if IOS || MACCATALYST
    const int MachTaskBasicInfoFlavor = 20;

    [DllImport("/usr/lib/libSystem.dylib")]
    static extern IntPtr mach_task_self();

    [DllImport("/usr/lib/libSystem.dylib")]
    static extern int task_info(IntPtr targetTask, int flavor, out MachTaskBasicInfo taskInfo, ref uint taskInfoCount);

    [StructLayout(LayoutKind.Sequential)]
    struct TimeValue
    {
        public int Seconds;
        public int Microseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MachTaskBasicInfo
    {
        public IntPtr VirtualSize;
        public IntPtr ResidentSize;
        public IntPtr ResidentSizeMax;
        public TimeValue UserTime;
        public TimeValue SystemTime;
        public int Policy;
        public int SuspendCount;
    }
#endif
}
