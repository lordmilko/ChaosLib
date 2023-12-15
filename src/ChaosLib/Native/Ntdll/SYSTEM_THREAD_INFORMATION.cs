using System;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_THREAD_INFORMATION
    {
        public LARGE_INTEGER KernelTime;
        public LARGE_INTEGER UserTime;
        public LARGE_INTEGER CreateTime;
        public IntPtr WaitTime; //has +4 bytes padding on x64
        public IntPtr StartAddress;
        public CLIENT_ID ClientId;
        public int Priority;
        public int BasePriority;
        public int ContextSwitches;
        public int ThreadState;
        public int WaitReason;
    }
}