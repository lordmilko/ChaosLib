using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    public static partial class Ntdll
    {
        public const string OUT_OF_PROCESS_FUNCTION_TABLE_CALLBACK_EXPORT_NAME = "OutOfProcessFunctionTableCallback";

        #region NtOpenDirectoryObject

        public static IEnumerable<string> EnumerateDirectories(string objectName)
        {
            if (TryNtOpenDirectoryObject(ACCESS_MASK.DIRECTORY_QUERY, objectName, OBJ.CASE_INSENSITIVE, out var handle) != NTSTATUS.STATUS_SUCCESS)
                yield break;

            //Setup a buffer to store search results in. NtQueryDirectoryObject will store as many items as it can
            //in the buffer, and then set "context" to the number of items that were written. This is kind of an
            //implementation detail however; the user contract is that the last entry written will be "null"

            var size = 2048;
            using var buffer = new MemoryBuffer(size);

            int context = 0;

            var infoSize = Marshal.SizeOf<OBJECT_DIRECTORY_INFORMATION>();

            unsafe IntPtr GetNameBuffer(IntPtr p) => (IntPtr) (((OBJECT_DIRECTORY_INFORMATION*) p)->Name.Buffer);

            while (true)
            {
                var status = Ntdll.Native.NtQueryDirectoryObject(handle, buffer, size, false, false, ref context, out _);

                if (status == NTSTATUS.STATUS_NO_MORE_ENTRIES)
                    yield break;

                if (status != NTSTATUS.STATUS_SUCCESS && status != NTSTATUS.STATUS_MORE_ENTRIES)
                    status.ThrowOnNotOK();

                IntPtr pInfo = buffer;

                while (true)
                {
                    var pName = GetNameBuffer(pInfo);

                    if (pName == IntPtr.Zero)
                        break;

                    var name = Marshal.PtrToStringUni(pName);

                    yield return name;

                    pInfo += infoSize;
                }
            }
        }

        public static unsafe IntPtr NtOpenDirectoryObject(ACCESS_MASK DesiredAccess, string objectName, OBJ attributes)
        {
            TryNtOpenDirectoryObject(DesiredAccess, objectName, attributes, out var DirectoryHandle).ThrowOnNotOK();

            return DirectoryHandle;
        }

        public static unsafe NTSTATUS TryNtOpenDirectoryObject(
            ACCESS_MASK DesiredAccess,
            string objectName,
            OBJ attributes,
            out IntPtr DirectoryHandle)
        {
            using var nameBuffer = new MemoryBuffer(Marshal.StringToHGlobalUni(objectName));
            var length = (short)(objectName.Length * 2);

            var unicodeString = new UNICODE_STRING
            {
                Length = length,
                MaximumLength = length,
                Buffer = (ushort*)(IntPtr)nameBuffer
            };

            using var unicodeStringBuffer = new MemoryBuffer<UNICODE_STRING>(unicodeString);

            var objectAttributes = new OBJECT_ATTRIBUTES
            {
                Length = Marshal.SizeOf<OBJECT_ATTRIBUTES>(),
                ObjectName = (UNICODE_STRING*)(IntPtr)unicodeStringBuffer,
                Attributes = attributes
            };

            var status = Native.NtOpenDirectoryObject(out DirectoryHandle, DesiredAccess, ref objectAttributes);

            return status;
        }

        #endregion

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

        public static T NtQueryObject<T>(
            IntPtr Handle,
            OBJECT_INFORMATION_CLASS ObjectInformationClass)
        {
            var size = Marshal.SizeOf<T>();
            using var buffer = new MemoryBuffer(size);

            var status = Native.NtQueryObject(
                Handle,
                ObjectInformationClass,
                buffer,
                size,
                out _
            );

            if (status != NTSTATUS.STATUS_SUCCESS)
                throw new InvalidOperationException($"{nameof(NtQueryObject)} failed with status {status}");

            return Marshal.PtrToStructure<T>(buffer);
        }

        public static T NtQuerySystemInformation<T>(
            SYSTEM_INFORMATION_CLASS SystemInformationClass)
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new MemoryBuffer(size);

            var status = Native.NtQuerySystemInformation(
                SystemInformationClass,
                buffer,
                size,
                out _
            );

            if (status != NTSTATUS.STATUS_SUCCESS)
            {
                if (status == NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
                    throw new InvalidOperationException($"Variable length data structures should be manually handled, rather than using this helper method. {nameof(NtQuerySystemInformation)} returned status: {status}");

                throw new InvalidOperationException($"{nameof(NtQuerySystemInformation)} failed with status {status}");
            }

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
