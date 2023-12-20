using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_HANDLE_INFORMATION_EX
    {
        public IntPtr NumberOfHandles;
        public IntPtr Reserved;

        //SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Handles[1]
        public IntPtr Handles;
    }
}