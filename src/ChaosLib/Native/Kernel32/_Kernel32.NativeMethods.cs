using System;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    public delegate bool ConsoleCtrlHandlerRoutine(int controlType);

    public static partial class Kernel32
    {
        internal static class Native
        {
            private const string kernel32 = "kernel32.dll";

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool CloseHandle(IntPtr handle);

            [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern bool CreateProcessA(
                string lpApplicationName,
                string lpCommandLine,
                ref SECURITY_ATTRIBUTES lpProcessAttributes,
                ref SECURITY_ATTRIBUTES lpThreadAttributes,
                bool bInheritHandles,
                CreateProcessFlags dwCreationFlags,
                IntPtr lpEnvironment,
                string lpCurrentDirectory,
                [In] ref STARTUPINFOA lpStartupInfo,
                out PROCESS_INFORMATION lpProcessInformation);

            [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool CreateProcessW(
                string lpApplicationName,
                string lpCommandLine,
                ref SECURITY_ATTRIBUTES lpProcessAttributes,
                ref SECURITY_ATTRIBUTES lpThreadAttributes,
                bool bInheritHandles,
                CreateProcessFlags dwCreationFlags,
                IntPtr lpEnvironment,
                string lpCurrentDirectory,
                [In] ref STARTUPINFOW lpStartupInfo,
                out PROCESS_INFORMATION lpProcessInformation);

            [DllImport(kernel32)]
            public static extern IntPtr CreateRemoteThread(
                IntPtr hProcess,
                IntPtr lpThreadAttributes,
                int dwStackSize,
                IntPtr lpStartAddress,
                IntPtr lpParameter,
                int dwCreationFlags,
                out IntPtr lpThreadId);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool FreeLibrary(IntPtr hLibModule);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool GetExitCodeThread(IntPtr hThread, out int lpExitCode);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool GetThreadContext(IntPtr hThread, IntPtr lpContext);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool IsWow64Process(
                [In] IntPtr hProcess,
                [Out, MarshalAs(UnmanagedType.Bool)] out bool Wow64Process);

            [DllImport(kernel32, EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr LoadLibrary(string lpLibFileName);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr OpenProcess(
                ProcessAccessFlags dwDesiredAccess,
                bool bInheritHandle,
                int dwProcessId);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool ReadProcessMemory(
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                [Out] IntPtr lpBuffer,
                IntPtr dwSize,
                out IntPtr lpNumberOfBytesRead);

            [DllImport(kernel32, SetLastError = true)]
            public static extern int ResumeThread(IntPtr hThread);

            [DllImport(kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool SetDllDirectory(string lpPathName);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool SetEvent([In] IntPtr hEvent);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerRoutine HandlerRoutine, bool Add);

            [DllImport(kernel32, SetLastError = true)]
            public static extern int SuspendThread(IntPtr hThread);

            [DllImport(kernel32, SetLastError = true, ExactSpelling = true)]
            public static extern IntPtr VirtualAllocEx(
                IntPtr hProcess,
                IntPtr lpAddress,
                int dwSize,
                AllocationType flAllocationType,
                MemoryProtection flProtect);

            [DllImport(kernel32, SetLastError = true, ExactSpelling = true)]
            public static extern bool VirtualFreeEx(
                IntPtr hProcess,
                IntPtr lpAddress,
                int dwSize,
                AllocationType dwFreeType);

            [DllImport(kernel32, SetLastError = true)]
            public static extern uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool WriteProcessMemory(
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                IntPtr lpBuffer,
                IntPtr nSize,
                out IntPtr lpNumberOfBytesWritten);

            [DllImport(kernel32, EntryPoint = "RtlZeroMemory", SetLastError = false)]
            public static extern void ZeroMemory(IntPtr dest, int size);
        }
    }
}
