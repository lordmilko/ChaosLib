using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TI_FINDCHILDREN_PARAMS
    {
        public int Count;
        public int Start;
        public fixed int ChildId[1];
    }
}