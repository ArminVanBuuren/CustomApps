using System.Runtime.InteropServices;

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
