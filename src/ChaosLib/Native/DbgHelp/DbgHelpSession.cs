using System;
using System.Collections.Generic;
using ChaosLib.TypedData;
using ClrDebug;
using ClrDebug.DIA;

namespace ChaosLib
{
    /// <summary>
    /// Provides facilities for managing a DbgHelp session for a given process,
    /// with automatic cleanup of resources on disposal.
    /// </summary>
    public class DbgHelpSession : IDisposable
    {
        /* Facts about using custom symsrv DLLs
         * -------------------------------------
         * Ostensibly, you can instruct DbgEng to use a different DLL than symsrv.dll by specifying
         * a _NT_SYMBOL_PATH to symsrv*customdll.dll*normalSymbolPaths. In practice, it's not so simple.
         * There are several limitations that limit your ability to use a custom symsrv DLL.
         *
         * - By default, DbgHelp will make its symbol search path '.;_NT_SYMBOL_PATH'. This would not be
         *   an issue, except DbgHelp will attempt to load symsrv.dll from "." prior to the DLL specified in
         *   your _NT_SYMBOL_PATH. When it fails to find symsrsv.dll in ".", it sets the global variable for
         *   storing the symsrv module handle to -1, thereby causing any subsequent attempts to load a custom
         *   symsrv DLL to error out immediately. This can be remediated by setting a custom search path to
         *   SymInitialize() to %_NT_SYMBOL_PATH%
         *
         * - Even if you do this however, dbghelp!LoadDLL won't just let you load a DLL from any random location;
         *   not only are you not allowed to specify a drive letter or directory name with your custom DLL, but
         *   DbgHelp explicitly sets the DLL path to be an absolute path in the same directory as itself. Therefore,
         *   you can't use SetDllDirectory() to trick DbgHelp to load your DLL from some other random location
         *
         * - You can set environment variable DBGHELP_DBGOUT=1 and DBGHELP_LOG=C:\someLogPath.log to get some basic
         *   logging about how DbgHelp went loading your custom DLL.
         *
         * In conclusion, in practice you can't really use a custom symsrv DLL without placing it in the same directory
         * as DbgHelp, which kind of defeats the purpose of being able to specify a custom symsrv DLL for the purposes
         * of using the copy of DbgHelp under system32 when you don't have the Debugging Tools for Windows installed.
         *
         * */

        private PSYMBOL_REGISTERED_CALLBACK64 callbackDispatcher;
        private PSYMBOL_FUNCENTRY_CALLBACK64 functionEntryCallbackDispatcher;
        private bool disposed;

        private HashSet<long> modules = new HashSet<long>();

        public IntPtr hProcess { get; }

        #region Callback

        private PSYMBOL_REGISTERED_CALLBACK64 callback;

        public PSYMBOL_REGISTERED_CALLBACK64 Callback
        {
            get => callback;
            set
            {
                if (callback == null)
                {
                    if (callbackDispatcher == null)
                    {
                        callbackDispatcher = (a, b, c, d) => Callback?.Invoke(a, b, c, d) == true;
                        DbgHelp.SymRegisterCallback64(hProcess, callbackDispatcher);
                    }

                    callback = value;
                }
                else
                {
                    //We already have a value
                    if (value != null)
                        throw new NotImplementedException("Cannot set callback: a callback has already been set.");

                    callback = null;
                }
            }
        }

        #endregion
        #region FunctionEntryCallback

        private PSYMBOL_FUNCENTRY_CALLBACK64 functionEntryCallback;

        public PSYMBOL_FUNCENTRY_CALLBACK64 FunctionEntryCallback
        {
            get => functionEntryCallback;
            set
            {
                if (functionEntryCallback == null)
                {
                    if (functionEntryCallbackDispatcher == null)
                    {
                        functionEntryCallbackDispatcher = (a, b, c) => FunctionEntryCallback?.Invoke(a, b, c) ?? IntPtr.Zero;
                        DbgHelp.SymRegisterFunctionEntryCallback64(hProcess, functionEntryCallbackDispatcher);
                    }

                    functionEntryCallback = value;
                }
                else
                {
                    //We already have a value
                    if (value != null)
                        throw new InvalidOperationException("Cannot set function entry callback: a callback has already been set.");

                    functionEntryCallback = null;
                }
            }
        }

        #endregion

        public DbgHelpSession(IntPtr hProcess, string userSearchPath = null, bool invadeProcess = true)
        {
            if (hProcess == IntPtr.Zero)
                throw new ArgumentNullException(nameof(hProcess));

            this.hProcess = hProcess;

            DbgHelp.SymInitialize(hProcess, userSearchPath, invadeProcess);
        }

        public void AddModule(string imageName, long baseOfDll)
        {
            modules.Add(baseOfDll);

            DbgHelp.SymLoadModuleEx(hProcess, imageName: imageName, baseOfDll: (ulong) baseOfDll);
        }

        public HRESULT TrySymFromAddr(long address, out SymFromAddrResult result) =>
            DbgHelp.TrySymFromAddr(hProcess, address, out result);

        public SymbolInfo SymFromIndex(long BaseOfDll, int Index) =>
            DbgHelp.SymFromIndex(hProcess, BaseOfDll, Index);

        public IMAGEHLP_MODULE64 SymGetModuleInfo64(long qwAddr) =>
            DbgHelp.SymGetModuleInfo64(hProcess, qwAddr);

        public HRESULT TrySymGetModuleInfo64(long qwAddr, out IMAGEHLP_MODULE64 moduleInfo) =>
            DbgHelp.TrySymGetModuleInfo64(hProcess, qwAddr, out moduleInfo);

        public DiaSession SymGetDiaSession(long modBase) =>
            DbgHelp.SymGetDiaSession(hProcess, modBase);

        public SymbolInfo SymGetTypeFromName(long baseOfDll, string name) =>
            DbgHelp.SymGetTypeFromName(hProcess, baseOfDll, name);

        public IntPtr SymGetTypeInfo(long ModBase, int TypeId, IMAGEHLP_SYMBOL_TYPE_INFO GetType) =>
            DbgHelp.SymGetTypeInfo(hProcess, ModBase, TypeId, GetType);

        public HRESULT TrySymGetTypeInfo(long ModBase, int TypeId, IMAGEHLP_SYMBOL_TYPE_INFO GetType, ref IntPtr pInfo) =>
            DbgHelp.TrySymGetTypeInfo(hProcess, ModBase, TypeId, GetType, ref pInfo);

        public DbgHelpTypeInfo GetTypeInfo(long moduleBase, int index) =>
            new DbgHelpTypeInfo(moduleBase, index, this);

        public void Dispose()
        {
            if (disposed)
                return;

            foreach (var module in modules)
                DbgHelp.SymUnloadModule64(hProcess, module);

            modules.Clear();

            DbgHelp.SymCleanup(hProcess);

            disposed = true;
        }
    }
}