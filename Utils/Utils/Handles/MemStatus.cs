using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Handles
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class MemStatus
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        public MemStatus()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MemStatus));
        }
    }
}
