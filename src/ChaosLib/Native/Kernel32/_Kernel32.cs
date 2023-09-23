﻿using System;
using System.Runtime.InteropServices;
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

        public static void CloseHandle(IntPtr handle) => Native.CloseHandle(handle);

        public static void FreeLibrary(IntPtr hLibModule) => Native.FreeLibrary(hLibModule);

        public static void SetConsoleCtrlHandler(ConsoleCtrlHandlerRoutine HandlerRoutine, bool Add) =>
            Native.SetConsoleCtrlHandler(HandlerRoutine, Add);

        public static void SetDllDirectory(string lpPathName) => Native.SetDllDirectory(lpPathName);

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

        public static void CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            bool waitForFinish = false)
        {
            TryCreateRemoteThread(hProcess, lpStartAddress, lpParameter, waitForFinish).ThrowOnNotOK();
        }

        public static HRESULT TryCreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            bool waitForFinish = false)
        {
            var result = Native.CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0,
                lpStartAddress,
                lpParameter,
                0,
                out var lpThreadId
            );

            if (result == IntPtr.Zero)
                return (HRESULT)Marshal.GetHRForLastWin32Error();

            if (waitForFinish)
                WaitForSingleObject(lpThreadId, INFINITE);

            CloseHandle(lpThreadId);

            return S_OK;
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
        #region GetThreadContext

        public static void GetThreadContext(IntPtr hThread, IntPtr lpContext) =>
            TryGetThreadContext(hThread, lpContext).ThrowOnNotOK();

        public static HRESULT TryGetThreadContext(IntPtr hThread, IntPtr lpContext)
        {
            var result = Native.GetThreadContext(hThread, lpContext);

            return result ? S_OK : (HRESULT) Marshal.GetHRForLastWin32Error();
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
        #region SuspendThread

        public static void SuspendThread(IntPtr hThread)
        {
            var result = Native.SuspendThread(hThread);

            if (result == -1)
                throw new DebugException((HRESULT) Marshal.GetHRForLastWin32Error());
        }

        #endregion
    }
}