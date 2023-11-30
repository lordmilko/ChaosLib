using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    public static partial class Shell32
    {
        public static class Native
        {
            private const string shell32 = "shell32.dll";

            [DllImport(shell32, SetLastError = true)]
            public static extern IntPtr CommandLineToArgvW(
                [In, MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine,
                [Out] out int pNumArgs);
        }
    }
}
