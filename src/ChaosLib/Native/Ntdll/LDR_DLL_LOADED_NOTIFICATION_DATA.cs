using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LDR_DLL_LOADED_NOTIFICATION_DATA
    {
        public int Flags;
        public UNICODE_STRING* FullDllName;
        public UNICODE_STRING* BaseDllName;
        public IntPtr DllBase;
        public int SizeOfImage;
    }
}