using System;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    public static partial class Ntdll
    {
        private const string ntdll = "ntdll.dll";

        public static class Native
        {
            [DllImport(ntdll)]
            public static extern NTSTATUS NtQueryInformationProcess(
                [In] IntPtr ProcessHandle,
                [In] PROCESSINFOCLASS ProcessInformationClass,
                [Out] IntPtr ProcessInformation,
                [In] int ProcessInformationLength,
                [Out] out int ReturnLength);

            //Prior to Windows 8, the return type was BOOLEAN (1 byte). Now it's LOGICAL = ULONG = 4 bytes
            [DllImport(ntdll)]
            public static extern int RtlFreeHeap(
                [In] IntPtr HeapHandle,
                [In] int Flags,
                [In] IntPtr HeapBase);

            [DllImport(ntdll)]
            public static extern IntPtr RtlGetFunctionTableListHead();
        }
    }
}
