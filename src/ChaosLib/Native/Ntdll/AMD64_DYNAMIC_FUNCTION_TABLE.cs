using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AMD64_DYNAMIC_FUNCTION_TABLE
    {
        public LIST_ENTRY ListEntry;
        public long FunctionTable;
        public LARGE_INTEGER TimeStamp;
        public long MinimumAddress;
        public long MaximumAddress;
        public long BaseAddress;
        public long Callback; //This field must always be 8 bytes even in x86
        public long Context;
        public long OutOfProcessCallbackDll;
        public AMD64_FUNCTION_TABLE_TYPE Type;
        public int EntryCount;
    }
}