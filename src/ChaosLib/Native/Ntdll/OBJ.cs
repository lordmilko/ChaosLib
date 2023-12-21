using System;

namespace ChaosLib
{
    [Flags]
    public enum OBJ
    {
        INHERIT = 0x00000002,
        PERMANENT = 0x00000010,
        EXCLUSIVE = 0x00000020,
        CASE_INSENSITIVE = 0x00000040,
        OPENIF = 0x00000080,
        OPENLINK = 0x00000100,
        KERNEL_HANDLE = 0x00000200,
        FORCE_ACCESS_CHECK = 0x00000400,
        VALID_ATTRIBUTES = 0x000007f2
    }
}