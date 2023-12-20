using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ChaosLib.Memory;
using ClrDebug;
using static ChaosLib.Kernel32.Native;
using static ClrDebug.HRESULT;

namespace ChaosLib
{
    /// <summary>
    /// Provides access to functions defined in Kernel32.dll
    /// </summary>
    public static partial class Kernel32
    {
        public const int INFINITE = -1;
        public const int S_FALSE = 1;
        public const int MAX_PATH = 260;
        public const int PAGE_SIZE = 0x1000;
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        #region Relay

        public static bool CloseHandle(IntPtr handle) => Native.CloseHandle(handle);

        public static void FreeLibrary(IntPtr hLibModule) => Native.FreeLibrary(hLibModule);

        public static int GetCurrentThreadId() => Native.GetCurrentThreadId();

        public static void SetConsoleCtrlHandler(ConsoleCtrlHandlerRoutine HandlerRoutine, bool Add) =>
            Native.SetConsoleCtrlHandler(HandlerRoutine, Add);

        public static void VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress) => Native.VirtualFreeEx(hProcess, lpAddress, 0, AllocationType.Release);

        public static WAIT WaitForSingleObject(IntPtr hHandle, int dwMilliseconds) =>
            Native.WaitForSingleObject(hHandle, dwMilliseconds);

        public static void ZeroMemory(IntPtr dest, int size) => Native.ZeroMemory(dest, size);

        #endregion
        #region AddDllDirectory

        public static IntPtr AddDllDirectory(string NewDirectory)
        {
            var result = Native.AddDllDirectory(NewDirectory);

            if (result == IntPtr.Zero)
                ((HRESULT) Marshal.GetHRForLastWin32Error()).ThrowOnNotOK();

            return result;
        }

        #endregion
        #region CreateFileW

        public static HRESULT TryCreateFileW(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FILE_SHARE dwShareMode,
            FileMode dwCreationDisposition,
            out IntPtr hFile)
        {
            var result = Native.CreateFileW(
                lpFileName,
                (int) dwDesiredAccess,
                dwShareMode,
                IntPtr.Zero,
                dwCreationDisposition,
                0x80 //FILE_ATTRIBUTE_NORMAL
            );

            if (result == INVALID_HANDLE_VALUE)
            {
                hFile = default;
                return (HRESULT)Marshal.GetHRForLastWin32Error();
            }

            hFile = result;

            return S_OK;
        }

        #endregion
        #region CreateProcessA

        public static void CreateProcessA(
            string lpCommandLine,
            CreateProcessFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFOA lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation)
        {
            var processAttribs = new SECURITY_ATTRIBUTES();
            var threadAttribs = new SECURITY_ATTRIBUTES();

            var result = Native.CreateProcessA(
                lpApplicationName: null,
                lpCommandLine: lpCommandLine,
                ref processAttribs,
                ref threadAttribs,
                true,
                dwCreationFlags,
                lpEnvironment,
                lpCurrentDirectory,
                ref lpStartupInfo,
                out lpProcessInformation
            );

            if (!result)
            {
                var hr = (HRESULT)Marshal.GetHRForLastWin32Error();

                throw new InvalidOperationException($"Failed to start process '{lpCommandLine}': CreateProcess returned {hr}");
            }
        }

        #endregion
        #region CreateProcessW

        public static void CreateProcessW(
            string lpCommandLine,
            CreateProcessFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFOW lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation)
        {
            var processAttribs = new SECURITY_ATTRIBUTES();
            var threadAttribs = new SECURITY_ATTRIBUTES();

            var result = Native.CreateProcessW(
                lpApplicationName: null,
                lpCommandLine: lpCommandLine,
                ref processAttribs,
                ref threadAttribs,
                true,
                dwCreationFlags,
                lpEnvironment,
                lpCurrentDirectory,
                ref lpStartupInfo,
                out lpProcessInformation
            );

            if (!result)
            {
                var hr = (HRESULT)Marshal.GetHRForLastWin32Error();

                throw new InvalidOperationException($"Failed to start process '{lpCommandLine}': CreateProcess returned {hr}");
            }
        }

        #endregion
        #region CreateRemoteThread

        public static int CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            bool waitForFinish = false)
        {
            TryCreateRemoteThread(hProcess, lpStartAddress, lpParameter, waitForFinish, out var exitCode).ThrowOnNotOK();
            return exitCode;
        }

        public static HRESULT TryCreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            bool waitForFinish,
            out int exitCode)
        {
            exitCode = default;

            var hThread = Native.CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0,
                lpStartAddress,
                lpParameter,
                0,
                out _ //lpThreadId
            );

            if (hThread == IntPtr.Zero)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            if (waitForFinish)
                WaitForSingleObject(hThread, INFINITE);

            if (!GetExitCodeThread(hThread, out exitCode))
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            CloseHandle(hThread);

            return S_OK;
        }

        #endregion
        #region CreateToolhelp32Snapshot

        public static IntPtr CreateToolhelp32Snapshot(TH32CS dwFlags, int th32ProcessID)
        {
            TryCreateToolhelp32Snapshot(dwFlags, th32ProcessID, out var hSnapshot).ThrowOnNotOK();
            return hSnapshot;
        }

        public static HRESULT TryCreateToolhelp32Snapshot(TH32CS dwFlags, int th32ProcessID, out IntPtr hSnapshot)
        {
            hSnapshot = Native.CreateToolhelp32Snapshot(dwFlags, th32ProcessID);

            if (hSnapshot == INVALID_HANDLE_VALUE)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return S_OK;
        }

        #endregion
        #region DebugActiveProcessStop

        public static void DebugActiveProcessStop(int dwProcessId)
        {
            var result = Native.DebugActiveProcessStop(dwProcessId);

            if (!result)
                ((HRESULT) Marshal.GetHRForLastWin32Error()).ThrowOnNotOK();
        }

        #endregion
        #region EnumProcessModulesEx

        public static IntPtr[] EnumProcessModulesEx(IntPtr hProcess, LIST_MODULES dwFilterFlag)
        {
            TryEnumProcessModulesEx(hProcess, dwFilterFlag, out var modules).ThrowOnNotOK();
            return modules;
        }

        public static HRESULT TryEnumProcessModulesEx(IntPtr hProcess, LIST_MODULES dwFilterFlag, out IntPtr[] modules)
        {
            var result = Native.EnumProcessModulesEx(hProcess, IntPtr.Zero, 0, out var lpcbNeeded, dwFilterFlag);

            if (result)
            {
                using var buffer = new MemoryBuffer(lpcbNeeded);

                result = Native.EnumProcessModulesEx(hProcess, buffer, lpcbNeeded, out lpcbNeeded, dwFilterFlag);

                if (result)
                {
                    var length = lpcbNeeded / IntPtr.Size;
                    var results = new IntPtr[length];

                    for (var i = 0; i < length; i++)
                        results[i] = Marshal.PtrToStructure<IntPtr>((IntPtr) buffer + i * IntPtr.Size);

                    modules = results;
                    return S_OK;
                }
            }

            modules = null;
            return (HRESULT) Marshal.GetHRForLastWin32Error();
        }

        #endregion
        #region FindVolumes

        public static IEnumerable<string> FindVolumes()
        {
            TryFindFirstVolumeW(out var name, out var hFindVolume).ThrowOnNotOK();

            yield return name;

            try
            {
                while (TryFindNextVolumeW(hFindVolume, out name) != ERROR_NO_MORE_FILES)
                {
                    yield return name;
                }
            }
            finally
            {
                TryFindVolumeClose(hFindVolume).ThrowOnNotOK();
            }
        }

        public static HRESULT TryFindFirstVolumeW(out string lpszVolumeName, out IntPtr hFindVolume)
        {
            var size = MAX_PATH * 2; //WCHAR
            using var buffer = new MemoryBuffer(size);
            var result = FindFirstVolumeW(buffer, size);

            if (result == INVALID_HANDLE_VALUE)
            {
                lpszVolumeName = default;
                hFindVolume = default;

                return (HRESULT) Marshal.GetHRForLastWin32Error();
            }

            lpszVolumeName = Marshal.PtrToStringUni(buffer);
            hFindVolume = result;
            return S_OK;
        }

        public static HRESULT TryFindNextVolumeW(IntPtr hFindVolume, out string lpszVolumeName)
        {
            var size = MAX_PATH * 2; //WCHAR
            using var buffer = new MemoryBuffer(size);

            var result = FindNextVolumeW(hFindVolume, buffer, size);

            if (!result)
            {
                lpszVolumeName = null;
                return (HRESULT) Marshal.GetHRForLastWin32Error();
            }

            lpszVolumeName = Marshal.PtrToStringUni(buffer);
            return S_OK;
        }

        public static HRESULT TryFindVolumeClose(IntPtr hFindVolume)
        {
            var result = Native.FindVolumeClose(hFindVolume);

            if (!result)
                return (HRESULT)Marshal.GetHRForLastWin32Error();

            return S_OK;
        }

        #endregion
        #region GetModuleFileNameExW

        public static string GetModuleFileNameExW(IntPtr hProcess, IntPtr hModule)
        {
            var buffer = new char[MAX_PATH];

            var result = Native.GetModuleFileNameExW(hProcess, hModule, buffer, MAX_PATH);

            if (result == 0)
                ((HRESULT) Marshal.GetHRForLastWin32Error()).ThrowOnNotOK();

            return new string(buffer, 0, result);
        }

        #endregion
        #region GetModuleHandleW

        public static IntPtr GetModuleHandleW(string lpModuleName)
        {
            GetModuleHandleW(lpModuleName, out var hModule).ThrowOnNotOK();
            return hModule;
        }

        public static HRESULT GetModuleHandleW(string lpModuleName, out IntPtr hModule)
        {
            hModule = Native.GetModuleHandleW(lpModuleName);

            if (hModule == IntPtr.Zero)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return HRESULT.S_OK;
        }

        #endregion
        #region GetProcAddress

        /// <summary>
        /// Gets the address of the specified procedure from the specified module.
        /// </summary>
        /// <param name="hModule">A handle to the module to get the procedure address from.</param>
        /// <param name="lpProcName">The name of the procedure to retrieve.</param>
        /// <returns>The address of the specified procedure</returns>
        /// <exception cref="EntryPointNotFoundException">The specified procedure was not found.</exception>
        public static IntPtr GetProcAddress(IntPtr hModule, string lpProcName)
        {
            var result = Native.GetProcAddress(hModule, lpProcName);

            if (result == IntPtr.Zero)
                throw new EntryPointNotFoundException($"Unable to find entry point named '{lpProcName}' in DLL: {(HRESULT)Marshal.GetHRForLastWin32Error()}");

            return result;
        }

        #endregion
        #region GetProcessHeap

        public static IntPtr GetProcessHeap()
        {
            TryGetProcessHeap(out var hHeap).ThrowOnNotOK();
            return hHeap;
        }

        public static HRESULT TryGetProcessHeap(out IntPtr hHeap)
        {
            var result = Native.GetProcessHeap();

            if (result == IntPtr.Zero)
            {
                hHeap = IntPtr.Zero;
                return (HRESULT) Marshal.GetHRForLastWin32Error();
            }

            hHeap = result;
            return S_OK;
        }

        #endregion
        #region GetProcessId

        public static int GetProcessId(IntPtr Process)
        {
            var result = Native.GetProcessId(Process);

            if (result == 0)
                ((HRESULT) Marshal.GetHRForLastWin32Error()).ThrowOnNotOK();

            return result;
        }

        #endregion
        #region GetThreadContext

        public static void GetThreadContext(IntPtr hThread, IntPtr lpContext) =>
            TryGetThreadContext(hThread, lpContext).ThrowOnNotOK();

        public static HRESULT TryGetThreadContext(IntPtr hThread, IntPtr lpContext)
        {
            var result = Native.GetThreadContext(hThread, lpContext);

            return result ? S_OK : (HRESULT) Marshal.GetHRForLastWin32Error();
        }

        #endregion
        #region GetThreadDescription

        public static string GetThreadDescription(IntPtr hThread)
        {
            var hr = Native.GetThreadDescription(hThread, out var ppszThreadDescription);

            if (hr == STATUS_SUCCESS)
            {
                try
                {
                    var str = Marshal.PtrToStringUni(ppszThreadDescription);

                    if (str == string.Empty)
                        return null;

                    return str;
                }
                finally
                {
                    LocalFree(ppszThreadDescription);
                }
            }

            throw new DebugException(hr);
        }

        #endregion
        #region GetVolumePathNamesForVolumeNameW

        public static string[] GetVolumePathNamesForVolumeNameW(string lpszVolumeName)
        {
            var size = MAX_PATH;
            var buffer = new char[size];

            var result = Native.GetVolumePathNamesForVolumeNameW(lpszVolumeName, buffer, size, out size);

            if (!result)
            {
                var hr = (HRESULT) Marshal.GetHRForLastWin32Error();

                if (hr != ERROR_MORE_DATA)
                    hr.ThrowOnNotOK();

                buffer = new char[size];

                result = Native.GetVolumePathNamesForVolumeNameW(lpszVolumeName, buffer, size, out size);

                if (!result)
                    ((HRESULT)Marshal.GetHRForLastWin32Error()).ThrowOnNotOK();
            }

            int start = 0;

            var results = new List<string>();

            for (var i = 0; i < size; i++)
            {
                if (buffer[i] == '\0')
                {
                    var length = i - start;

                    if (length == 0)
                        break;

                    results.Add(new string(buffer, start, length));
                    start = i + 1;
                }
            }

            return results.ToArray();
        }

        #endregion
        #region HeapFree

        public static void HeapFree(IntPtr hHeap, int dwFlags, IntPtr lpMem) =>
            TryHeapFree(hHeap, dwFlags, lpMem).ThrowOnNotOK();

        public static HRESULT TryHeapFree(IntPtr hHeap, int dwFlags, IntPtr lpMem)
        {
            var result = Native.HeapFree(hHeap, dwFlags, lpMem);

            if (!result)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return S_OK;
        }

        #endregion
        #region IsWow64Process

        public static bool IsWow64ProcessOrDefault(IntPtr hProcess)
        {
            if (!Native.IsWow64Process(hProcess, out var isWow64))
                return false;

            return isWow64;
        }

        public static bool IsWow64Process(IntPtr hProcess)
        {
            if (!Native.IsWow64Process(hProcess, out var isWow64))
                throw new InvalidOperationException($"Failed to query {nameof(Native.IsWow64Process)}: {(HRESULT)Marshal.GetHRForLastWin32Error()}");

            return isWow64;
        }

        #endregion
        #region LoadLibrary

        public static IntPtr LoadLibrary(string lpLibFileName)
        {
            var hModule = Native.LoadLibrary(lpLibFileName);

            if (hModule != IntPtr.Zero)
                return hModule;

            var hr = (HRESULT) Marshal.GetHRForLastWin32Error();

            if (hr == ERROR_BAD_EXE_FORMAT)
                throw new BadImageFormatException($"Failed to load module '{lpLibFileName}'. Module may target an architecture different from the current process.");

            var ex = Marshal.GetExceptionForHR((int)hr);

            throw new DllNotFoundException($"Unable to load DLL '{lpLibFileName}' or one of its dependencies: {ex.Message}");
        }

        #endregion
        #region OpenProcess

        public static SafeProcessHandle OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId)
        {
            var result = Native.OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);

            if (result == IntPtr.Zero)
                throw new DebugException((HRESULT) Marshal.GetHRForLastWin32Error());

            return result;
        }

        #endregion
        #region OpenThread

        public static SafeThreadHandle OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId)
        {
            TryOpenThread(dwDesiredAccess, bInheritHandle, dwThreadId, out var hThread).ThrowOnNotOK();
            return hThread;
        }

        public static HRESULT TryOpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId, out SafeThreadHandle hThread)
        {
            hThread = Native.OpenThread(dwDesiredAccess, bInheritHandle, dwThreadId);

            return hThread == IntPtr.Zero ? (HRESULT) Marshal.GetHRForLastWin32Error() : S_OK;
        }

        #endregion
        #region QueryDosDeviceW

        public static string[] QueryDosDeviceW(string lpDeviceName = null)
        {
            if (lpDeviceName != null)
            {
                if (lpDeviceName.StartsWith("\\\\?\\"))
                    lpDeviceName = lpDeviceName.Substring(4);

                lpDeviceName = lpDeviceName.TrimEnd('\\');
            }

            //Note that if no device name is specified, this will list EVERY SINGLE device - not just hard disk drives!

            var size = MAX_PATH;
            var buffer = new char[size];

            while (true)
            {
                //The device name cannot have a trailing slash
                var result = Native.QueryDosDeviceW(lpDeviceName, buffer, size);

                if (result == 0)
                {
                    var hr = (HRESULT) Marshal.GetHRForLastWin32Error();

                    if (hr == ERROR_INSUFFICIENT_BUFFER)
                    {
                        size *= 2;
                        buffer = new char[size];
                    }
                    else
                        hr.ThrowOnNotOK();
                }
                else
                {
                    break;
                }
            }

            int start = 0;

            var results = new List<string>();

            for (var i = 0; i < size; i++)
            {
                if (buffer[i] == '\0')
                {
                    var length = i - start;

                    if (length == 0)
                        break;

                    results.Add(new string(buffer, start, length));
                    start = i + 1;
                }
            }

            return results.ToArray();
        }

        #endregion
        #region ReadProcessMemory

        public static bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead)
        {
            var result = Native.ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, new IntPtr(dwSize), out var read);

            lpNumberOfBytesRead = (int)read;

            return result;
        }

        public static byte[] ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            int dwSize)
        {
            byte[] buffer;
            TryReadProcessMemory(hProcess, lpBaseAddress, dwSize, out buffer).ThrowOnNotOK();
            return buffer;
        }

        public static HRESULT TryReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            int dwSize,
            out byte[] buffer)
        {
            var buff = Marshal.AllocHGlobal(dwSize);

            try
            {
                var result = Native.ReadProcessMemory(
                    hProcess,
                    lpBaseAddress,
                    buff,
                    new IntPtr(dwSize),
                    out var lpNumberOfBytesRead
                );

                if (!result)
                {
                    buffer = null;
                    return (HRESULT) Marshal.GetHRForLastWin32Error();
                }

                buffer = new byte[(int) lpNumberOfBytesRead];
                Marshal.Copy(buff, buffer, 0, (int) lpNumberOfBytesRead);
                return S_OK;
            }
            finally
            {
                Marshal.FreeHGlobal(buff);
            }
        }

        #endregion
        #region RemoveDllDirectory

        public static void RemoveDllDirectory(IntPtr Cookie)
        {
            var result = Native.RemoveDllDirectory(Cookie);

            if (!result)
                ((HRESULT)Marshal.GetHRForLastWin32Error()).ThrowOnNotOK();
        }

        #endregion
        #region ResumeThread

        public static void ResumeThread(IntPtr hThread)
        {
            var result = Native.ResumeThread(hThread);

            if (result == -1)
                throw new DebugException((HRESULT) Marshal.GetHRForLastWin32Error());
        }

        #endregion
        #region SetDllDirectory

        public static void SetDllDirectory(string lpPathName)
        {
            var result = Native.SetDllDirectory(lpPathName);

            if (!result)
                ((HRESULT)Marshal.GetHRForLastWin32Error()).ThrowOnNotOK();
        }

        #endregion
        #region SetEvent

        public static void SetEvent(IntPtr hEvent) => TrySetEvent(hEvent).ThrowOnNotOK();

        public static HRESULT TrySetEvent(IntPtr hEvent)
        {
            var result = Native.SetEvent(hEvent);

            return result ? S_OK : (HRESULT) Marshal.GetHRForLastWin32Error();
        }

        #endregion
        #region SuspendThread

        public static void SuspendThread(IntPtr hThread)
        {
            var result = Native.SuspendThread(hThread);

            if (result == -1)
                throw new DebugException((HRESULT) Marshal.GetHRForLastWin32Error());
        }

        #endregion
        #region VirtualAllocEx

        public static IntPtr VirtualAllocEx(IntPtr hProcess, int dwSize, AllocationType flAllocationType, MemoryProtection flProtect)
        {
            var result = Native.VirtualAllocEx(hProcess, IntPtr.Zero, dwSize, flAllocationType, flProtect);

            if (result == IntPtr.Zero)
                throw new DebugException((HRESULT) Marshal.GetHRForLastWin32Error());

            return result;
        }

        #endregion
        #region WriteProcessMemory

        public static bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            out int lpNumberOfBytesWritten)
        {
            var result = Native.WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, new IntPtr(nSize), out var written);

            lpNumberOfBytesWritten = (int) written;

            return result;
        }

        #endregion
    }
}
