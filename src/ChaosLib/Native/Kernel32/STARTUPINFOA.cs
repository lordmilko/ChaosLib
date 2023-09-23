using System;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct STARTUPINFOA
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public STARTF dwFlags;
        public ShowWindow wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }
}