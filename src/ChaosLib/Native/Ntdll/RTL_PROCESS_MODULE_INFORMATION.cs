using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RTL_PROCESS_MODULE_INFORMATION
    {
        public IntPtr Section;
        public IntPtr MappedBase;
        public IntPtr ImageBase;
        public int ImageSize;
        public int Flags;
        public short LoadOrderIndex;
        public short InitOrderIndex;
        public short LoadCount;
        public short OffsetToFileName;
        public fixed byte FullPathName[256];
    }
}