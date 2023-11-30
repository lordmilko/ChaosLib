using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct STACKFRAME64
    {
        public ADDRESS64 AddrPC;
        public ADDRESS64 AddrReturn;
        public ADDRESS64 AddrFrame;
        public ADDRESS64 AddrStack;
        public ADDRESS64 AddrBStore;
        
        public IntPtr FuncTableEntry;
        
        public fixed long Params[4];

        public bool Far;
        public bool Virtual;

        public fixed long Reserved[3];

        public KDHELP64 KdHelp;
    }
}