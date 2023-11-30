using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_RUNTIME_FUNCTION_ENTRY
    {
        [FieldOffset(0)]
        public int BeginAddress;

        [FieldOffset(4)]
        public int EndAddress;

        [FieldOffset(8)]
        public int UnwindInfoAddress;

        [FieldOffset(8)]
        public int UnwindData;
    }
}