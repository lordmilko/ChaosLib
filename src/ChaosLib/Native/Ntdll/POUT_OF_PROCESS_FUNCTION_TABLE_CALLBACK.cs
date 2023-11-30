using System;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    public unsafe delegate NTSTATUS POUT_OF_PROCESS_FUNCTION_TABLE_CALLBACK(
        [In] IntPtr Process,
        [In] IntPtr TableAddress,
        [Out] out int Entries,
        [Out] out RUNTIME_FUNCTION* Functions);
}