using System;
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

        #region Relay

        public static bool CloseHandle(IntPtr handle) => Native.CloseHandle(handle);

        public static void FreeLibrary(IntPtr hLibModule) => Native.FreeLibrary(hLibModule);

        public static void SetConsoleCtrlHandler(ConsoleCtrlHandlerRoutine HandlerRoutine, bool Add) =>
            Native.SetConsoleCtrlHandler(HandlerRoutine, Add);

        public static void SetDllDirectory(string lpPathName) => Native.SetDllDirectory(lpPathName);

        public static void VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress) => Native.VirtualFreeEx(hProcess, lpAddress, 0, AllocationType.Release);

        public static void ZeroMemory(IntPtr dest, int size) => Native.ZeroMemory(dest, size);

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
        #region GetThreadContext

        public static void GetThreadContext(IntPtr hThread, IntPtr lpContext) =>
            TryGetThreadContext(hThread, lpContext).ThrowOnNotOK();

        public static HRESULT TryGetThreadContext(IntPtr hThread, IntPtr lpContext)
        {
            var result = Native.GetThreadContext(hThread, lpContext);

            return result ? S_OK : (HRESULT) Marshal.GetHRForLastWin32Error();
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

        public static IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId)
        {
            TryOpenThread(dwDesiredAccess, bInheritHandle, dwThreadId, out var hThread).ThrowOnNotOK();
            return hThread;
        }

        public static HRESULT TryOpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId, out IntPtr hThread)
        {
            hThread = Native.OpenThread(dwDesiredAccess, bInheritHandle, dwThreadId);

            return hThread == IntPtr.Zero ? (HRESULT)Marshal.GetHRForLastWin32Error() : S_OK;
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
        #region ResumeThread

        public static void ResumeThread(IntPtr hThread)
        {
            var result = Native.ResumeThread(hThread);

            if (result == -1)
                throw new DebugException((HRESULT) Marshal.GetHRForLastWin32Error());
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
        #region WaitForSingleObject

        public static WAIT WaitForSingleObject(IntPtr hHandle, int dwMilliseconds) =>
            Native.WaitForSingleObject(hHandle, dwMilliseconds);

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
