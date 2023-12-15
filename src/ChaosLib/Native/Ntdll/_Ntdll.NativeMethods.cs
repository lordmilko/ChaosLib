﻿using System;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    public unsafe delegate void PLDR_DLL_NOTIFICATION_FUNCTION(
        [In] LDR_DLL_NOTIFICATION_REASON NotificationReason,
        [In] LDR_DLL_NOTIFICATION* NotificationData,
        [In] IntPtr Context);

    public static partial class Ntdll
    {
        private const string ntdll = "ntdll.dll";

        public static unsafe class Native
        {
            [DllImport(ntdll)]
            public static extern NTSTATUS LdrRegisterDllNotification(
                [In] int Flags,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] PLDR_DLL_NOTIFICATION_FUNCTION NotificationFunction,
                [In] IntPtr Context,
                [Out] out IntPtr Cookie);

            [DllImport(ntdll)]
            public static extern NTSTATUS LdrUnregisterDllNotification(
                [In] IntPtr Cookie);

            [DllImport(ntdll)]
            public static extern NTSTATUS NtQueryInformationProcess(
                [In] IntPtr ProcessHandle,
                [In] PROCESSINFOCLASS ProcessInformationClass,
                [Out] IntPtr ProcessInformation,
                [In] int ProcessInformationLength,
                [Out] out int ReturnLength);

            [DllImport(ntdll)]
            public static extern NTSTATUS NtQueryInformationThread(
                [In] IntPtr ThreadHandle,
                [In] THREADINFOCLASS ThreadInformationClass,
                [Out] IntPtr ThreadInformation,
                [In] int ThreadInformationLength,
                [Out] out int ReturnLength);

            [DllImport(ntdll)]
            public static extern NTSTATUS NtQuerySystemInformation(
                [In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
                [Out] IntPtr SystemInformation,
                [In] int SystemInformationLength,
                [Out] out int ReturnLength);

            //Prior to Windows 8, the return type was BOOLEAN (1 byte). Now it's LOGICAL = ULONG = 4 bytes
            [DllImport(ntdll)]
            public static extern int RtlFreeHeap(
                [In] IntPtr HeapHandle,
                [In] int Flags,
                [In] IntPtr HeapBase);

            [DllImport(ntdll)]
            public static extern IntPtr RtlGetFunctionTableListHead();

            [DllImport(ntdll)]
            public static extern RTL_DEBUG_INFORMATION* RtlCreateQueryDebugBuffer(
                [In] int MaximumCommit,
                [In, MarshalAs(UnmanagedType.U1)] bool UseEventPair);

            [DllImport(ntdll)]
            public static extern NTSTATUS RtlDestroyQueryDebugBuffer(
                [In] RTL_DEBUG_INFORMATION* Buffer);

            [DllImport(ntdll)]
            public static extern NTSTATUS RtlQueryProcessDebugInformation(
                [In] IntPtr UniqueProcessId,
                [In] RTL_QUERY_PROCESS Flags,
                [Out] RTL_DEBUG_INFORMATION* Buffer);
        }
    }
}
