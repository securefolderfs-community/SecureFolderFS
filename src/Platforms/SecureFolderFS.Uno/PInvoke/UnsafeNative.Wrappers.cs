#if WINDOWS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace SecureFolderFS.Uno.PInvoke
{
    internal static partial class UnsafeNative
    {
        public static List<MONITORINFOEX> GetMonitorInfos()
        {
            var count = GetSystemMetrics(SystemMetric.SM_CMONITORS);

            var infos = new List<MONITORINFOEX>(count);
            var gcHandle = GCHandle.Alloc(infos);
            var pGcHandle = GCHandle.ToIntPtr(gcHandle);

            var success = EnumDisplayMonitors(default, null, MonitorEnumProc, pGcHandle);
            gcHandle.Free();
            if (!success)
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            return infos;
        }

        private static bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, PRECT lprcMonitor, IntPtr dwData)
        {
            var handle = GCHandle.FromIntPtr(dwData);
            if (!lprcMonitor.IsEmpty && handle is { IsAllocated: true, Target: List<MONITORINFOEX> list })
            {
                var monitorInfo = new MONITORINFOEX()
                {
                    cbSize = (uint)Marshal.SizeOf<MONITORINFOEX>()
                };

                GetMonitorInfo(hMonitor, ref monitorInfo);
                list.Add(monitorInfo);
            }

            return true;
        }
    }
}
#endif
