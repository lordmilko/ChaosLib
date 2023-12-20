using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [Obsolete]
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_HANDLE_INFORMATION
    {
        public int NumberOfHandles;

        //SYSTEM_HANDLE_TABLE_ENTRY_INFO Handles[1]
        public IntPtr Handles;
    }
}
