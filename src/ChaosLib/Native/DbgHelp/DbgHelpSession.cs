using System;
using System.Collections.Generic;
using ClrDebug;

namespace ChaosLib
{
    /// <summary>
    /// Provides facilities for managing a DbgHelp session for a given process,
    /// with automatic cleanup of resources on disposal.
    /// </summary>
    public class DbgHelpSession : IDisposable
    {
        private IntPtr hProcess;
        private PSYMBOL_REGISTERED_CALLBACK64 callbackDispatcher;
        private PSYMBOL_FUNCENTRY_CALLBACK64 functionEntryCallbackDispatcher;
        private bool disposed;

        private HashSet<long> modules = new HashSet<long>();

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

        public DbgHelpSession(IntPtr hProcess)
        {
            if (hProcess == IntPtr.Zero)
                throw new ArgumentNullException(nameof(hProcess));

            this.hProcess = hProcess;

            DbgHelp.SymInitialize(hProcess, invadeProcess: true);
        }

        public void AddModule(string imageName, long baseOfDll)
        {
            modules.Add(baseOfDll);

            DbgHelp.SymLoadModuleEx(hProcess, imageName: imageName, baseOfDll: (ulong) baseOfDll);
        }

        public HRESULT TrySymFromAddr(long address, out SymFromAddrResult result) =>
            DbgHelp.TrySymFromAddr(hProcess, address, out result);

        public HRESULT TrySymGetModuleInfo64(long qwAddr, out IMAGEHLP_MODULE64 moduleInfo) =>
            DbgHelp.TrySymGetModuleInfo64(hProcess, qwAddr, out moduleInfo);

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