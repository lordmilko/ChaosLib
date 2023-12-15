using System;

namespace ChaosLib
{
    [Flags]
    public enum RTL_QUERY_PROCESS : uint
    {
        MODULES = 0x00000001,
        BACKTRACES = 0x00000002,
        HEAP_SUMMARY = 0x00000004,
        HEAP_TAGS = 0x00000008,
        HEAP_ENTRIES = 0x00000010,
        LOCKS = 0x00000020,
        MODULES32 = 0x00000040,
        NONINVASIVE = 0x80000000
    }
}