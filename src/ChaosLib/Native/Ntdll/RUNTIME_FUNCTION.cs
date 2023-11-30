using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RUNTIME_FUNCTION
    {
        public int BeginAddress;
        public int EndAddress;
        public int UnwindData;
    }
}