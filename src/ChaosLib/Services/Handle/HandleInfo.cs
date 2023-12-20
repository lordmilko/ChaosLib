using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib.Handle
{
    /// <summary>
    /// Represents an operating system handle.
    /// </summary>
    [DebuggerDisplay("[{Type.ToString(),nq}] {Name,nq}")]
    public abstract class HandleInfo
    {
        #region Static

        public static IEnumerable<HandleInfo> EnumerateHandles(int? processId = null)
        {
            using var buffer = GetHandleInformation();

            //As you can't use unsafe code in an enumerator, we need to do some tricky hacks
            static unsafe int GetNumberOfHandles(IntPtr ptr) => (int) ((SYSTEM_HANDLE_INFORMATION_EX*) ptr)->NumberOfHandles;

            static unsafe SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX GetInfo(IntPtr ptr, int i) =>
                ((SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX*) &((SYSTEM_HANDLE_INFORMATION_EX*) ptr)->Handles)[i];

            var numberOfHandles = GetNumberOfHandles(buffer);

            for (var i = 0; i < numberOfHandles; i++)
            {
                var pInfo = GetInfo(buffer, i);

                if (processId == null || pInfo.UniqueProcessId == (IntPtr) processId)
                    yield return New(pInfo.HandleValue);
            }
        }

        private static MemoryBuffer GetHandleInformation()
        {
            //There's a variable length array contained in the handle information, so we need a buffer
            //way bigger than the struct we're after here
            var size = Kernel32.PAGE_SIZE;

            //We're going to be returning this to the caller, so don't dispose it!
            var buffer = new MemoryBuffer(size);

            try
            {
                NTSTATUS status;

                while (true)
                {
                    //SystemHandleInformation is limited to PIDs < 65536, thus we need to use
                    //SystemExtendedHandleInformation instead
                    status = Ntdll.Native.NtQuerySystemInformation(
                        SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation,
                        buffer,
                        size,
                        out var returnLength
                    );

                    if (status == NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        //Since the last call, even more memory could be required, so we add some breathing room
                        size = returnLength + Kernel32.PAGE_SIZE;
                        buffer.Dispose();
                        buffer = new MemoryBuffer(size);
                    }
                    else
                        break;
                }

                status.ThrowOnNotOK();
            }
            catch
            {
                buffer.Dispose();

                throw;
            }

            return buffer;
        }

        public static T New<T>(IntPtr handle) where T : HandleInfo => (T) New(handle);

        public static HandleInfo New(IntPtr handle)
        {
            var type = GetHandleType(handle);

            switch (type.Value)
            {
                case HandleType.File:
                    return new FileHandleInfo(handle);

                case HandleType.ALPCPort:
                case HandleType.Event:
                case HandleType.DebugObject:
                case HandleType.Desktop:
                case HandleType.Directory:
                case HandleType.EtwRegistration:
                case HandleType.Key:
                case HandleType.IoCompletion:
                case HandleType.IRTimer:
                case HandleType.Mutant:
                case HandleType.Process:
                case HandleType.Section:
                case HandleType.Semaphore:
                case HandleType.Thread:
                case HandleType.Timer:
                case HandleType.TpWorkerFactory:
                case HandleType.WaitCompletionPacket:
                case HandleType.WindowStation:
                    return new GenericHandleInfo(handle, type.Value.Value);

                default:
                    throw new NotImplementedException($"Don't know how to handle handle of type '{type.Value}'");
            }
        }

        private static unsafe StringEnum<HandleType> GetHandleType(IntPtr handle)
        {
            //Add extra space for storing the name
            var size = Marshal.SizeOf<OBJECT_TYPE_INFORMATION>() + 1000;
            using var buffer = new MemoryBuffer(size);

            //note that even handle.exe queries for objectinformation in a killable thread

            var status = Ntdll.Native.NtQueryObject(handle, OBJECT_INFORMATION_CLASS.ObjectTypeInformation, buffer, size, out var returnLength);

            if (status == NTSTATUS.STATUS_INVALID_HANDLE)
                return HandleType.Invalid;

            status.ThrowOnNotOK();

            var pInformation = (OBJECT_TYPE_INFORMATION*)(IntPtr)buffer;

            var name = Marshal.PtrToStringUni((IntPtr)pInformation->TypeName.Buffer);

            if (Enum.TryParse<HandleType>(name, out var type))
                return type;

            return new StringEnum<HandleType>(name, () => HandleType.Unknown);
        }

        #endregion

        public IntPtr Raw { get; }

        private string name;

        public string Name
        {
            get
            {
                if (name == null)
                    name = GetHandleName();

                return name;
            }
        }

        public HandleType Type { get; }

        protected HandleInfo(IntPtr raw, HandleType type)
        {
            Raw = raw;
            Type = type;
        }

        public static implicit operator IntPtr(HandleInfo handle) => handle.Raw;

        protected virtual unsafe string GetHandleName()
        {
            var bufferSize = 2000; //Random large value
            using var buffer = new MemoryBuffer(bufferSize);

            /* It is considered very dangerous to query for ObjectNameInformation. NtQueryObject can reportedly
             * sometimes hang while trying to pull this information. The recommended way of retrieving ObjectNameInformation
             * is to spin up a separate thread to make the call, and if it seems to be taking too long, call TerminateThread.
             * handle.exe takes this approach. Some people state that if the OBJECT_BASIC_INFORMATION.GrantedAccess == 0x0012019f
             * this means that NtQueryObject will hang. I don't know what these flags mean, and would rather get a first hand
             * citation than just copy what everyone else is doing. Similarly, everybody else always duplicates their handle
             * prior to querying it. I haven't seen a clear justification for doing this as yet either.
             *
             * Aside from the fact Raymond Chen has repeatedly emphasized that there is no valid scenario for calling TerminateThread,
             * even if this were in fact an actual valid scenario, we're in .NET here. Thread.Abort() merely asks, rather
             * than tells (and isn't compatible with .NET Core) and god forbid what would happen if you called TerminateThread
             * on a thread that was being managed by the CLR
             *
             * In conclusion: until I can investigate a specific scenario where NtQueryObject hangs, we're going to just execute
             * it and see what happens. */
            var status = Ntdll.Native.NtQueryObject(Raw, OBJECT_INFORMATION_CLASS.ObjectNameInformation, buffer, bufferSize, out _);

            if (status == NTSTATUS.STATUS_INVALID_HANDLE)
                return null;

            status.ThrowOnNotOK();

            var pStr = &((OBJECT_NAME_INFORMATION*)(IntPtr)buffer)->Name;

            if (pStr->Buffer == (ushort*)0)
                return null;

            var str = Marshal.PtrToStringUni((IntPtr) pStr->Buffer);

            return str;
        }
    }
}
