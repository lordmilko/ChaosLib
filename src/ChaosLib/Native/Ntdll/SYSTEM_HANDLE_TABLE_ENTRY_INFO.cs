using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [Obsolete]
    [StructLayout(LayoutKind.Sequential)]
    public class SYSTEM_HANDLE_TABLE_ENTRY_INFO
    {
        public ushort UniqueProcessId;
        public ushort CreatorBackTraceIndex;
        public byte ObjectTypeIndex;
        public byte HandleAttributes;
        public ushort HandleValue;
        public IntPtr Object;
        public int GrantedAccess;
    }
}