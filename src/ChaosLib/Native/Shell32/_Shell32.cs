using System;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    public static partial class Shell32
    {
        public static string[] CommandLineToArgvW(string lpCmdLine)
        {
            TryCommandLineToArgvW(lpCmdLine, out var args).ThrowOnNotOK();
            return args;
        }

        public static HRESULT TryCommandLineToArgvW(string lpCmdLine, out string[] args)
        {
            var ptr = Native.CommandLineToArgvW(lpCmdLine, out var pNumArgs);

            if (ptr == IntPtr.Zero)
            {
                args = null;
                return (HRESULT)Marshal.GetHRForLastWin32Error();
            }

            var results = new string[pNumArgs];

            try
            {
                for (var i = 0; i < pNumArgs; i++)
                {
                    var strPtr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);

                    var str = Marshal.PtrToStringUni(strPtr);

                    results[i] = str;
                }
            }
            finally
            {
                Kernel32.Native.LocalFree(ptr);
            }

            args = results;
            return HRESULT.S_OK;
        }
    }
}
