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

            [DllImport(kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr AddDllDirectory(
                [In, MarshalAs(UnmanagedType.LPWStr)] string NewDirectory);

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

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr CreateRemoteThread(
                IntPtr hProcess,
                IntPtr lpThreadAttributes,
                int dwStackSize,
                IntPtr lpStartAddress,
                IntPtr lpParameter,
                int dwCreationFlags,
                out IntPtr lpThreadId);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr CreateToolhelp32Snapshot(
                [In] TH32CS dwFlags,
                [In] int th32ProcessID);

            //In PSAPI_VERSION 2 this function is exported from Kernel32 with a different name
            [DllImport(kernel32, EntryPoint = "K32EnumProcessModulesEx", SetLastError = true)]
            public static extern bool EnumProcessModulesEx(
                [In] IntPtr hProcess,
                [Out] IntPtr lphModule,
                [In] int cb,
                [Out] out int lpcbNeeded,
                [In] LIST_MODULES dwFilterFlag);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool FreeLibrary(IntPtr hLibModule);

            [DllImport(kernel32)]
            public static extern int GetCurrentThreadId();

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool GetExitCodeThread(IntPtr hThread, out int lpExitCode);

            [DllImport(kernel32, EntryPoint = "K32GetModuleFileNameExW", SetLastError = true)]
            public static extern int GetModuleFileNameExW(
                [In] IntPtr hProcess,
                [In, Optional] IntPtr hModule,
                [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2)] char[] lpFileName,
                [In] int nSize);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr GetModuleHandleW(
                [In, MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr GetProcessHeap();

            [DllImport(kernel32, SetLastError = true)]
            public static extern int GetProcessId([In] IntPtr Process);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool GetThreadContext(IntPtr hThread, IntPtr lpContext);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool HeapFree(
                [In] IntPtr hHeap,
                [In] int dwFlags,
                [In] IntPtr lpMem);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool IsWow64Process(
                [In] IntPtr hProcess,
                [Out, MarshalAs(UnmanagedType.Bool)] out bool Wow64Process);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr LocalFree(
                [In] IntPtr hMem);

            [DllImport(kernel32, EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr LoadLibrary(string lpLibFileName);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool Module32First(
                [In] IntPtr hSnapshot,
                [In, Out] ref MODULEENTRY32 lpme);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool Module32Next(
                [In] IntPtr hSnapshot,
                [In, Out] ref MODULEENTRY32 lpme);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr OpenProcess(
                ProcessAccessFlags dwDesiredAccess,
                bool bInheritHandle,
                int dwProcessId);

            [DllImport(kernel32, SetLastError = true)]
            public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool ReadProcessMemory(
                [In] IntPtr hProcess,
                [In] IntPtr lpBaseAddress,
                [Out] IntPtr lpBuffer,
                [In] IntPtr dwSize,
                [Out] out IntPtr lpNumberOfBytesRead);

            [DllImport(kernel32, SetLastError = true)]
            public static extern bool RemoveDllDirectory(
                [In] IntPtr Cookie);

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
            public static extern WAIT WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

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
