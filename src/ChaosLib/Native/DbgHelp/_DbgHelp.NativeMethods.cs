using System;
using System.Runtime.InteropServices;
using ClrDebug;
using ClrDebug.DbgEng;
using ClrDebug.DIA;

namespace ChaosLib
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate bool PSYM_ENUMMODULES_CALLBACK64(
        [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName,
        [In] long BaseOfDll,
        [In] IntPtr UserContext);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate bool PSYM_ENUMERATESYMBOLS_CALLBACK(
        [In] IntPtr pSymInfo,
        [In] int SymbolSize,
        [In] IntPtr UserContext);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate IntPtr PSYMBOL_FUNCENTRY_CALLBACK64(
        [In] IntPtr hProcess,
        [In] long AddrBase,
        [In] long UserContext);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate bool PSYMBOL_REGISTERED_CALLBACK64(
        [In] IntPtr hProcess,
        [In] CBA ActionCode,
        [In] long CallbackData,
        [In] long UserContext);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate bool PREAD_PROCESS_MEMORY_ROUTINE64(
        [In] IntPtr hProcess,
        [In] long qwBaseAddress,
        [In] IntPtr lpBuffer,
        [In] int nSize,
        [Out] int lpNumberOfBytesRead);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate IntPtr PFUNCTION_TABLE_ACCESS_ROUTINE64(
        IntPtr ahProcess,
        long AddrBase);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate long PGET_MODULE_BASE_ROUTINE64(
        [In] IntPtr hProcess,
        [In] long Address);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate long PTRANSLATE_ADDRESS_ROUTINE64(
        [In] IntPtr hProcess,
        [In] IntPtr hThread,
        [In] ref ADDRESS64 lpaddr);

    public static partial class DbgHelp
    {
        public static class Native
        {
            private const string dbghelp = "dbghelp.dll";

            [DllImport(dbghelp)]
            public static extern bool StackWalk64(
                [In] IMAGE_FILE_MACHINE MachineType,
                [In] IntPtr hProcess,
                [In] IntPtr hThread,
                [In, Out] ref STACKFRAME64 StackFrame,
                [In, Out] IntPtr ContextRecord,
                [In, Optional] PREAD_PROCESS_MEMORY_ROUTINE64 ReadMemoryRoutine,
                [In, Optional] PFUNCTION_TABLE_ACCESS_ROUTINE64 FunctionTableAccessRoutine,
                [In, Optional] PGET_MODULE_BASE_ROUTINE64 GetModuleBaseRoutine,
                [In, Optional] PTRANSLATE_ADDRESS_ROUTINE64 TranslateAddress);

            [DllImport(dbghelp)]
            public static extern bool StackWalkEx( //StackWalkEx supersedes StackWalk64
                [In] IMAGE_FILE_MACHINE MachineType,
                [In] IntPtr hProcess,
                [In] IntPtr hThread,
                [In, Out] ref STACKFRAME_EX StackFrame,
                [In, Out] IntPtr ContextRecord,
                [In, Optional] PREAD_PROCESS_MEMORY_ROUTINE64 ReadMemoryRoutine,
                [In, Optional] PFUNCTION_TABLE_ACCESS_ROUTINE64 FunctionTableAccessRoutine,
                [In, Optional] PGET_MODULE_BASE_ROUTINE64 GetModuleBaseRoutine,
                [In, Optional] PTRANSLATE_ADDRESS_ROUTINE64 TranslateAddress,
                [In] SYM_STKWALK Flags);

            [DllImport(dbghelp)]
            internal static extern bool SymCleanup(
                [In] IntPtr hProcess);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymInitializeW(
                [In] IntPtr hProcess,
                [In, MarshalAs(UnmanagedType.LPWStr)] string UserSearchPath,
                [In] bool fInvadeProcess);

            [DllImport(dbghelp, SetLastError = true)]
            public static extern bool SymEnumerateModules64(
                [In] IntPtr hProcess,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] PSYM_ENUMMODULES_CALLBACK64 EnumModulesCallback,
                [In] IntPtr UserContext);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymFromAddr(
                [In] IntPtr hProcess,
                [In] ulong Address,
                [Out] out long Displacement,
                [Out] IntPtr Symbol);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymFromIndex(
                [In] IntPtr hProcess,
                [In] long BaseOfDll,
                [In] int Index,
                [Out] IntPtr Symbol);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymFromName(
                [In] IntPtr hProcess,
                [In, MarshalAs(UnmanagedType.LPStr)] string Name,
                [Out] IntPtr Symbol);

            [DllImport(dbghelp, SetLastError = true)]
            public static extern IntPtr SymFunctionTableAccess64(
                [In] IntPtr hProcess,
                [In] long AddrBase);

            [DllImport(dbghelp, SetLastError = true)]
            public static extern long SymGetModuleBase64(
                [In] IntPtr hProcess,
                [In] long qwAddr);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymGetModuleInfo64(
                [In] IntPtr hProcess,
                [In] ulong qwAddr,
                [In, Out] ref IMAGEHLP_MODULE64 ModuleInfo);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymGetTypeFromName(
                [In] IntPtr hProcess,
                [In] long BaseOfDll,
                [In, MarshalAs(UnmanagedType.LPStr)] string Name,
                [Out] IntPtr Symbol);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymGetTypeInfo(
                [In] IntPtr hProcess,
                [In] long ModBase,
                [In] int TypeId,
                [In] IMAGEHLP_SYMBOL_TYPE_INFO GetType,
                [Out] IntPtr pInfo);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern ulong SymLoadModuleExW(
                [In] IntPtr hProcess,
                [In, Optional] IntPtr hFile,
                [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string ImageName,
                [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string ModuleName,
                [In, Optional] ulong BaseOfDll,
                [In, Optional] int DllSize,
                [In, Optional] IntPtr Data,
                [In, Optional] int Flags);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymRegisterCallback64(
                [In] IntPtr hProcess,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] PSYMBOL_REGISTERED_CALLBACK64 CallbackFunction,
                [In] long UserContext);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymRegisterFunctionEntryCallback64(
                [In] IntPtr hProcess,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] PSYMBOL_FUNCENTRY_CALLBACK64 CallbackFunction,
                [In] long UserContext);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymSearch(
                [In] IntPtr hProcess,
                [In] ulong BaseOfDll,
                [In] int Index,
                [In] SymTagEnum SymTag,
                [In, MarshalAs(UnmanagedType.LPStr)] string Mask,
                [In] ulong Address,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] PSYM_ENUMERATESYMBOLS_CALLBACK EnumSymbolsCallback,
                [In] IntPtr UserContext,
                [In] SYMSEARCH Options);

            [DllImport(dbghelp)]
            internal static extern SYMOPT SymSetOptions(
                [In] SYMOPT SymOptions);

            [DllImport(dbghelp, SetLastError = true)]
            internal static extern bool SymUnloadModule64(
                [In] IntPtr hProcess,
                [In] long BaseOfDll);

            [DllImport(dbghelp, SetLastError = true)]
            public static extern bool SymGetDiaSession(
                [In] IntPtr hProcess,
                [In] long modBase,
                [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSession session);

            [DllImport(dbghelp, SetLastError = true)]
            public static extern bool SymGetDiaSource(
                [In] IntPtr hProcess,
                [In] long modBase,
                [Out, MarshalAs(UnmanagedType.Interface)] out IDiaDataSource dataSource);

            //stackdbg is a WinDbg extension exported by dbghelp that can be used
            //for toggling stack trace debugging on and off
            [DllImport(dbghelp)]
            public static extern void stackdbg(
                [In] IntPtr hCurrentProcess,
                [In] IntPtr hCurrentThread,
                [In] long dwCurrentPc,
                [In] int dwProcessor,
                [In, MarshalAs(UnmanagedType.LPStr)] string args);

            [DllImport(dbghelp)]
            public static extern void WinDbgExtensionDllInit(
                ref WINDBG_EXTENSION_APIS lpExtensionApis,
                short MajorVersion,
                short MinorVersion);
        }
    }
}
