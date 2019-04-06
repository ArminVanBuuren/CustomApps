using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class SERVER
    {
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        public static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }

        }

        public static Int64 GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }
        }


        public static ProcessInfo[] ProcessList;
        const ProcessInfo PROCESS_INFO_NOT_FOUND = null;

        public static double GetCpuUsage(Process process = null)
        {
            if (process != null)
            {
                PerformanceCounter appCPUCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                appCPUCounter.NextValue();
                System.Threading.Thread.Sleep(1000);
                double.TryParse(appCPUCounter.NextValue().ToString(), out var resultApp);
                return resultApp;
            }

            PerformanceCounter totalCPUCounter = new PerformanceCounter();
            totalCPUCounter.CategoryName = "Processor";
            totalCPUCounter.CounterName = "% Processor Time";
            totalCPUCounter.InstanceName = "_Total";

            totalCPUCounter.NextValue();
            System.Threading.Thread.Sleep(1000);

            double.TryParse(totalCPUCounter.NextValue().ToString(), out var resultTotal);
            return resultTotal;
        }

        private static ProcessInfo ProcessInfoByID(int ID)
        {
            // gets the process info by it's id
            if (ProcessList == null) return PROCESS_INFO_NOT_FOUND;

            for (int i = 0; i < ProcessList.Length; i++)
                if (ProcessList[i] != PROCESS_INFO_NOT_FOUND && ProcessList[i].ID == ID)
                    return ProcessList[i];

            return PROCESS_INFO_NOT_FOUND;

        }

        public class ProcessInfo
        {
            public string Name;
            public string CpuUsage;
            public int ID;
            public long OldCpuUsage;
        }

        public static double GetMemUsage(Process process)
        {
            if (process == null)
                return default(double);

            if (IntPtr.Size == 4)
            {
                // 32-bit application
                return ((double)process.PagedMemorySize64 / 2);
            }
            else if (IntPtr.Size == 8)
            {
                // 64-bit application
                return ((double)process.PagedMemorySize64);
            }

            //var test1 = ((double) process.MinWorkingSet).ToFileSize();
            //var test2 = ((double) process.MaxWorkingSet).ToFileSize();
            //var test3 = ((double) process.NonpagedSystemMemorySize64).ToFileSize();
            //var test4 = ((double) process.PagedMemorySize64).ToFileSize();
            //var test5 = ((double) process.PagedSystemMemorySize64).ToFileSize();
            //var test6 = ((double) process.PeakPagedMemorySize64).ToFileSize();
            //var test7 = ((double) process.PeakVirtualMemorySize64).ToFileSize();
            //var test8 = ((double) process.PeakWorkingSet64).ToFileSize();
            //var test9 = ((double) process.VirtualMemorySize64).ToFileSize();
            //var test10 = ((double) process.WorkingSet64).ToFileSize();

            // The future is now!
            return default(double);
        }
    }
}
