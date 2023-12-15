using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RTL_DEBUG_INFORMATION
    {
        public IntPtr SectionHandleClient;
        public IntPtr ViewBaseClient;
        public IntPtr ViewBaseTarget;
        public IntPtr ViewBaseDelta;
        public IntPtr EventPairClient;
        public IntPtr EventPairTarget;
        public IntPtr TargetProcessId;
        public IntPtr TargetThreadHandle;
        public int Flags;
        public IntPtr OffsetFree;
        public IntPtr CommitSize;
        public IntPtr ViewSize;
        public RTL_PROCESS_MODULES* Modules;
        public IntPtr BackTraces; //RTL_PROCESS_BACKTRACES*
        public IntPtr Heaps; //RTL_PROCESS_HEAPS*
        public IntPtr Locks; //RTL_PROCESS_LOCKS*
        public IntPtr SpecificHeap;
        public IntPtr TargetProcessHandle;

        public IntPtr Reserved1;
        public IntPtr Reserved2;
        public IntPtr Reserved3;
        public IntPtr Reserved4;
        public IntPtr Reserved5;
        public IntPtr Reserved6;
    }
}