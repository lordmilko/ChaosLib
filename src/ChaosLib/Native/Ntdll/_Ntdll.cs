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
                throw new InvalidOperationException($"{nameof(NtQueryInformationProcess)} failed with status {status}");

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
                throw new InvalidOperationException($"{nameof(NtQueryInformationThread)} failed with status {status}");

            return Marshal.PtrToStructure<T>(buffer);
        }

        public static unsafe RTL_DEBUG_INFORMATION* RtlCreateQueryDebugBuffer(int MaximumCommit = 0, bool UseEventPair = false)
        {
            var result = Native.RtlCreateQueryDebugBuffer(MaximumCommit, UseEventPair);

            if ((IntPtr) result == IntPtr.Zero)
                throw new InvalidOperationException($"{nameof(RtlCreateQueryDebugBuffer)} failed.");

            return result;
        }

        public static unsafe void RtlDestroyQueryDebugBuffer(RTL_DEBUG_INFORMATION* Buffer)
        {
            var status = Native.RtlDestroyQueryDebugBuffer(Buffer);

            if (status != NTSTATUS.STATUS_SUCCESS)
                throw new InvalidOperationException($"{nameof(RtlDestroyQueryDebugBuffer)} failed with status {status}");
        }

        public static unsafe void RtlQueryProcessDebugInformation(
            int UniqueProcessId,
            RTL_QUERY_PROCESS Flags,
            RTL_DEBUG_INFORMATION* Buffer)
        {
            var status = Native.RtlQueryProcessDebugInformation((IntPtr) UniqueProcessId, Flags, Buffer);

            if (status != NTSTATUS.STATUS_SUCCESS)
                throw new InvalidOperationException($"{nameof(RtlQueryProcessDebugInformation)} failed with status {status}");
        }
    }
}
