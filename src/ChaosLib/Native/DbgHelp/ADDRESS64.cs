using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ADDRESS64
    {
        public long Offset;
        public short Segment;
        public ADDRESS_MODE Mode;

        public override string ToString()
        {
            return $"0x{Offset:X}";
        }
    }
}