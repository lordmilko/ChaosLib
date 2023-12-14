using System;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    public static partial class Ntdll
    {
        public const string OUT_OF_PROCESS_FUNCTION_TABLE_CALLBACK_EXPORT_NAME = "OutOfProcessFunctionTableCallback";

        public static T NtQueryInformationProcess<T>(
            IntPtr ProcessHandle,
            PROCESSINFOCLASS ProcessInformationClass)
        {
            var size = Marshal.SizeOf<T>();
            using var buffer = new MemoryBuffer(size);

            var status = Native.NtQueryInformationProcess(
                ProcessHandle,
                ProcessInformationClass,
                buffer,
                size,
                out _
            );

            if (status != NTSTATUS.STATUS_SUCCESS)
                throw new InvalidOperationException($"Query failed with status {status}");

            return Marshal.PtrToStructure<T>(buffer);
        }

        public static T NtQueryInformationThread<T>(
            IntPtr ThreadHandle,
            THREADINFOCLASS ThreadInformationClass)
        {
            var size = Marshal.SizeOf<T>();
            using var buffer = new MemoryBuffer(size);

            var status = Native.NtQueryInformationThread(
                ThreadHandle,
                ThreadInformationClass,
                buffer,
                size,
                out _
            );

            if (status != NTSTATUS.STATUS_SUCCESS)
                throw new InvalidOperationException($"Query failed with status {status}");

            return Marshal.PtrToStructure<T>(buffer);
        }
    }
}
