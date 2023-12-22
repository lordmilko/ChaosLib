using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ChaosLib.TypedData;
using ClrDebug;
using ClrDebug.DbgEng;
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
         * of using the copy of DbgHelp under system32 when you don't have the Debugging Tools for Windows installed. */

        private DbgHelpSymbolCallback callbackInstance;
        private PSYMBOL_REGISTERED_CALLBACK64 callback; //Without this it seems the delegate can get GC'd
        private PSYMBOL_FUNCENTRY_CALLBACK64 functionEntryCallbackDispatcher;
        private bool disposed;

        private HashSet<long> modules = new HashSet<long>();

        public IntPtr hProcess { get; }

        //Options are global within DbgHelp
        internal SYMOPT GlobalOptions
        {
            get => DbgHelp.SymGetOptions();
            set => DbgHelp.SymSetOptions(value);
        }

        public string[] Modules
        {
            get
            {
                var list = new List<string>();

                DbgHelp.SymEnumerateModules64(hProcess, (moduleName, baseAddress, userContext) =>
                {
                    list.Add(moduleName + " " + baseAddress.ToString("X"));
                    return true;
                });

                return list.ToArray();
            }
        }

        #region Callback

        public DbgHelpSymbolCallback Callback
        {
            get
            {
                if (callbackInstance == null)
                {
                    callbackInstance = new DbgHelpSymbolCallback();
                    callback = callbackInstance.CallbackHandler;
                    DbgHelp.SymRegisterCallback64(hProcess, callback);
                }

                return callbackInstance;
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

            var manualModLoad = false;

            if (invadeProcess && IntPtr.Size == 8 && Kernel32.IsWow64ProcessOrDefault(hProcess))
            {
                /* If we're a 64-bit process trying to debug a 32-bit process, allowing DbgHelp to enumerate all modules for us
                 * will result in Wow64 modules being included. We can tell DbgHelp to load 32-bit modules as well using
                 * SYMOPT_INCLUDE_32BIT_MODULES but this just makes things worse: if we're trying to use typed data, we need
                 * to calculate the offsets of fields based on the 32-bit PDB. The only way to do that is to guarantee that none
                 * of the 64-bit modules exist in our Dbghelp session */

                invadeProcess = false;
                manualModLoad = true;
            }

            this.hProcess = hProcess;

            DbgHelp.SymInitialize(hProcess, userSearchPath, invadeProcess);

            if (manualModLoad)
                Load32BitModulesFor64BitProcess();
        }

        private unsafe void Load32BitModulesFor64BitProcess()
        {
            /* We retrieve 32-bit modules, but now we have a new problem: if we try and get the file path using
             * GetModuleFileNameExW, it will simply show DLLs as being under system32 instead of SysWOW64.
             * Apparently GetModuleFileNameExW queries the PEB of the remote process to get the path, and
             * a 32-bit process will think its 32-bit modules are in fact under system32.
             *
             * We can resolve this by querying RtlQueryProcessDebugInformation directly. This actually how
             * DbgHelp loads its modules, however the difference here is that we ONLY load our 32-bit modules,
             * rather than every single module we come across. */

            //These are all the modules we're actually interested in
            var x86Modules = Kernel32.EnumProcessModulesEx(hProcess, LIST_MODULES.LIST_MODULES_32BIT);

            //The application itself has the same module base in both the 32-bit and 64-bit sections of the process, so we need to check
            //to see whether we've added a module before
            var addedModules = new HashSet<IntPtr>();

            var buffer = Ntdll.RtlCreateQueryDebugBuffer();

            try
            {
                //MODULES32 implies MODULES, so we need to filter out the retrieved modules for only the ones we're looking for
                Ntdll.RtlQueryProcessDebugInformation(Kernel32.GetProcessId(hProcess), RTL_QUERY_PROCESS.MODULES32 | RTL_QUERY_PROCESS.NONINVASIVE, buffer);

                var pModules = buffer->Modules;

                for (var i = 0; i < pModules->NumberOfModules; i++)
                {
                    var moduleInfo = ((RTL_PROCESS_MODULE_INFORMATION*) &pModules->Modules)[i];

                    if (!addedModules.Contains(moduleInfo.ImageBase) && x86Modules.Contains(moduleInfo.ImageBase))
                    {
                        //This is an x86 module. Load it in DbgHelp!
                        addedModules.Add(moduleInfo.ImageBase);

                        var modulePath = Marshal.PtrToStringAnsi((IntPtr) moduleInfo.FullPathName);

                        AddModule(modulePath, (long) (void*) moduleInfo.ImageBase, moduleInfo.ImageSize);
                    }
                }
            }
            finally
            {
                Ntdll.RtlDestroyQueryDebugBuffer(buffer);
            }
        }

        public void AddModule(string imageName, long baseOfDll, int dllSize)
        {
            /* You can usually get away with not specifying an image size. In dbghelp!LoadModule,
             * it will call dbghelp!modload which will result in the true image size being calculated for you.
             * However, this behavior does not occur when SYMOPT_DEFERRED_LOADS is specified...leading to any calls to
             * SymFromAddr now failing due to the bounds of each image now being unknown. Because DbgHelp's options are
             * set globally, if you load DbgEng to check something, it will overwrite all of the default DbgHelp options!
             * Thus it is safest to just always specify the image size and avoid these kinds of mixups */

            if (!modules.Add(baseOfDll))
                throw new InvalidOperationException("Module was already loaded");

            DbgHelp.SymLoadModuleEx(hProcess, imageName: imageName, baseOfDll: (ulong) baseOfDll, dllSize: dllSize);
        }

        public void RemoveModule(long baseOfDll)
        {
            modules.Remove(baseOfDll);

            DbgHelp.SymUnloadModule64(hProcess, baseOfDll);
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