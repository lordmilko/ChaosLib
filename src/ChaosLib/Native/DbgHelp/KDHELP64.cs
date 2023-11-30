using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct KDHELP64
    {
        public long Thread;
        public int ThCallbackStack;
        public int ThCallbackBStore;
        public int NextCallback;
        public int FramePointer;
        public long KiCallUserMode;
        public long KeUserCallbackDispatcher;
        public long SystemRangeStart;
        public long KiUserExceptionDispatcher;
        public long StackBase;
        public long StackLimit;

        public int BuildVersion;
        public int RetpolineStubFunctionTableSize;
        public long RetpolineStubFunctionTable;
        public int RetpolineStubOffset;
        public int RetpolineStubSize;
        public fixed long Reserved[2];
    }
}