using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UNICODE_STRING
    {
        public short Length;
        public short MaximumLength;

        public ushort* Buffer;
    }
}