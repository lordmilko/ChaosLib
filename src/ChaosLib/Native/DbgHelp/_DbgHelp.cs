using System;
using System.Runtime.InteropServices;
using ClrDebug;
using ClrDebug.DIA;

namespace ChaosLib
{
    //SetDllDirectory must be called, or DbgHelp must be manually loaded prior to interacting with the members on this class
    public static partial class DbgHelp
    {
        private const int MaxNameLength = 2000; //2000 characters

        #region SymCleanup

        public static void SymCleanup(IntPtr hProcess) => TrySymCleanup(hProcess).ThrowOnNotOK();

        public static HRESULT TrySymCleanup(IntPtr hProcess)
        {
            var result = Native.SymCleanup(hProcess);

            if (!result)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return HRESULT.S_OK;
        }

        #endregion
        #region SymInitialize

        public static void SymInitialize(IntPtr hProcess, string userSearchPath = null, bool invadeProcess = false) =>
            TrySymInitialize(hProcess, userSearchPath, invadeProcess).ThrowOnNotOK();

        public static HRESULT TrySymInitialize(IntPtr hProcess, string userSearchPath = null, bool invadeProcess = false)
        {
            var result = Native.SymInitializeW(hProcess, userSearchPath, invadeProcess);

            if (!result)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return HRESULT.S_OK;
        }

        #endregion
        #region SymGetDiaSession

        public static DiaSession SymGetDiaSession(IntPtr hProcess, long modBase)
        {
            TrySymGetDiaSession(hProcess, modBase, out var session).ThrowOnNotOK();
            return session;
        }

        public static HRESULT TrySymGetDiaSession(IntPtr hProcess, long modBase, out DiaSession session)
        {
            var result = Native.SymGetDiaSession(hProcess, modBase, out var raw);

            if (!result)
            {
                session = null;
                return (HRESULT) Marshal.GetHRForLastWin32Error();
            }

            session = raw != null ? new DiaSession(raw) : null;
            return HRESULT.S_OK;
        }

        #endregion
        #region SymGetModuleInfo64

        public static IMAGEHLP_MODULE64 SymGetModuleInfo64(IntPtr hProcess, long qwAddr)
        {
            TrySymGetModuleInfo64(hProcess, qwAddr, out var moduleInfo).ThrowOnNotOK();
            return moduleInfo;
        }

        public static HRESULT TrySymGetModuleInfo64(IntPtr hProcess, long qwAddr, out IMAGEHLP_MODULE64 moduleInfo)
        {
            moduleInfo = new IMAGEHLP_MODULE64
            {
                SizeOfStruct = Marshal.SizeOf<IMAGEHLP_MODULE64>()
            };

            var result = Native.SymGetModuleInfo64(hProcess, (ulong) qwAddr, ref moduleInfo);

            if (!result)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return HRESULT.S_OK;
        }

        #endregion
        #region SymGetTypeFromName

        public static SymbolInfo SymGetTypeFromName(IntPtr hProcess, long BaseOfDll, string Name)
        {
            TrySymGetTypeFromName(hProcess, BaseOfDll, Name, out var symbol).ThrowOnNotOK();
            return symbol;
        }

        public static unsafe HRESULT TrySymGetTypeFromName(IntPtr hProcess, long BaseOfDll, string Name, out SymbolInfo Symbol)
        {
            IntPtr buffer = IntPtr.Zero;

            try
            {
                buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SYMBOL_INFO)) + MaxNameLength);

                SYMBOL_INFO* pNative = (SYMBOL_INFO*) buffer;
                pNative->SizeOfStruct = Marshal.SizeOf(typeof(SYMBOL_INFO));
                pNative->MaxNameLen = MaxNameLength; // Characters, not bytes!

                var result = Native.SymGetTypeFromName(hProcess, BaseOfDll, Name, buffer);

                if (!result)
                {
                    Symbol = default;
                    return (HRESULT) Marshal.GetHRForLastWin32Error();
                }

                Symbol = new SymbolInfo(pNative);
                return HRESULT.S_OK;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion
        #region SymGetTypeInfo

        public static IntPtr SymGetTypeInfo(
            IntPtr hProcess,
            long ModBase,
            int TypeId,
            IMAGEHLP_SYMBOL_TYPE_INFO GetType)
        {
            IntPtr pInfo = IntPtr.Zero;
            TrySymGetTypeInfo(hProcess, ModBase, TypeId, GetType, ref pInfo).ThrowOnNotOK();
            return pInfo;
        }

        public static unsafe HRESULT TrySymGetTypeInfo(
            IntPtr hProcess,
            long ModBase,
            int TypeId,
            IMAGEHLP_SYMBOL_TYPE_INFO GetType,
            ref IntPtr pInfo)
        {
            var innerPtr = pInfo;

            MemoryBuffer buffer = null;

            //We have a problem: generally speaking the value emitted by SymGetTypeInfo will fit into an IntPtr,
            //but in some scenarios we need to be able to pass in a complex buffer or an arbitrary size. Thus, we say that
            //if the caller specified such a complex buffer, we'll use it. Otherwise, we'll create our own IntPtr sized
            //buffer

            if (innerPtr == IntPtr.Zero)
            {
                buffer = new MemoryBuffer(IntPtr.Size);
                innerPtr = buffer;
            }

            try
            {
                var result = Native.SymGetTypeInfo(
                    hProcess,
                    ModBase,
                    TypeId,
                    GetType,
                    innerPtr
                );

                //SymGetTypeInfo erroneously sets the HRESULT from DIA to be the last error
                if (!result)
                    return (HRESULT)Marshal.GetLastWin32Error();

                if (buffer != null)
                    pInfo = *(IntPtr*) innerPtr;

                return HRESULT.S_OK;
            }
            finally
            {
                buffer?.Dispose();
            }
        }

        #endregion
        #region SymFromAddr

        public static SymFromAddrResult SymFromAddr(IntPtr hProcess, long address)
        {
            SymFromAddrResult result;
            TrySymFromAddr(hProcess, address, out result).ThrowOnNotOK();
            return result;
        }

        public static unsafe HRESULT TrySymFromAddr(IntPtr hProcess, long address, out SymFromAddrResult result)
        {
            IntPtr buffer = IntPtr.Zero;

            try
            {
                buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SYMBOL_INFO)) + MaxNameLength);

                SYMBOL_INFO* pNative = (SYMBOL_INFO*) buffer;
                pNative->SizeOfStruct = Marshal.SizeOf(typeof(SYMBOL_INFO));
                pNative->MaxNameLen = MaxNameLength; // Characters, not bytes!

                long displacement;
                var innerResult = Native.SymFromAddr(hProcess, (ulong) address, out displacement, buffer);

                if (!innerResult)
                {
                    result = default;
                    return (HRESULT) Marshal.GetHRForLastWin32Error();
                }

                result = new SymFromAddrResult(displacement, new SymbolInfo(pNative));
                return HRESULT.S_OK;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion
        #region SymFromIndex

        public static SymbolInfo SymFromIndex(IntPtr hProcess, long BaseOfDll, int Index)
        {
            TrySymFromIndex(hProcess, BaseOfDll, Index, out var symbol).ThrowOnNotOK();
            return symbol;
        }

        public static unsafe HRESULT TrySymFromIndex(IntPtr hProcess, long BaseOfDll, int Index, out SymbolInfo Symbol)
        {
            IntPtr buffer = IntPtr.Zero;

            try
            {
                buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SYMBOL_INFO)) + MaxNameLength);

                SYMBOL_INFO* pNative = (SYMBOL_INFO*)buffer;
                pNative->SizeOfStruct = Marshal.SizeOf(typeof(SYMBOL_INFO));
                pNative->MaxNameLen = MaxNameLength; // Characters, not bytes!

                var innerResult = Native.SymFromIndex(hProcess, BaseOfDll, Index, buffer);

                if (!innerResult)
                {
                    Symbol = default;
                    return (HRESULT) Marshal.GetHRForLastWin32Error();
                }

                Symbol = new SymbolInfo(pNative);
                return HRESULT.S_OK;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion
        #region SymFromName

        public static SymbolInfo SymFromName(IntPtr hProcess, string Name)
        {
            TrySymFromName(hProcess, Name, out var Symbol).ThrowOnNotOK();
            return Symbol;
        }

        public static unsafe HRESULT TrySymFromName(IntPtr hProcess, string Name, out SymbolInfo Symbol)
        {
            IntPtr buffer = IntPtr.Zero;

            try
            {
                buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SYMBOL_INFO)) + MaxNameLength);

                SYMBOL_INFO* pNative = (SYMBOL_INFO*) buffer;
                pNative->SizeOfStruct = Marshal.SizeOf(typeof(SYMBOL_INFO));
                pNative->MaxNameLen = MaxNameLength; // Characters, not bytes!

                var result = Native.SymFromName(hProcess, Name, buffer);

                if (!result)
                {
                    Symbol = default;
                    return (HRESULT) Marshal.GetHRForLastWin32Error();
                }

                Symbol = new SymbolInfo(pNative);
                return HRESULT.S_OK;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion
        #region SymLoadModuleEx

        public static void SymLoadModuleEx(
            IntPtr hProcess,
            IntPtr hFile = default,
            string imageName = null,
            string moduleName = null,
            ulong baseOfDll = 0,
            int dllSize = 0,
            IntPtr data = default,
            int flags = 0
        ) => TrySymLoadModuleEx(hProcess, hFile, imageName, moduleName, baseOfDll, dllSize, data, flags).ThrowOnNotOK();

        public static HRESULT TrySymLoadModuleEx(
            IntPtr hProcess,
            IntPtr hFile = default,
            string imageName = null,
            string moduleName = null,
            ulong baseOfDll = 0,
            int dllSize = 0,
            IntPtr data = default,
            int flags = 0
        )
        {
            var result = Native.SymLoadModuleExW(
                hProcess,
                hFile,
                imageName,
                moduleName,
                baseOfDll,
                dllSize,
                data,
                flags
            );

            if (result == 0)
            {
                var err = Marshal.GetLastWin32Error();

                //Module was already loaded
                if (err == 0)
                    return HRESULT.S_FALSE;

                return (HRESULT) Marshal.GetHRForLastWin32Error();
            }

            return HRESULT.S_OK;
        }

        #endregion
        #region SymRegisterCallback64

        public static void SymRegisterCallback64(IntPtr hProcess, PSYMBOL_REGISTERED_CALLBACK64 CallbackFunction, long UserContext = 0) =>
            TrySymRegisterCallback64(hProcess, CallbackFunction, UserContext).ThrowOnNotOK();

        public static HRESULT TrySymRegisterCallback64(IntPtr hProcess, PSYMBOL_REGISTERED_CALLBACK64 CallbackFunction, long UserContext = 0)
        {
            var result = Native.SymRegisterCallback64(hProcess, CallbackFunction, UserContext);

            if (!result)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return HRESULT.S_OK;
        }

        #endregion
        #region SymRegisterFunctionEntryCallback64

        public static void SymRegisterFunctionEntryCallback64(IntPtr hProcess, PSYMBOL_FUNCENTRY_CALLBACK64 CallbackFunction, long UserContext = 0) =>
            TrySymRegisterFunctionEntryCallback64(hProcess, CallbackFunction, UserContext).ThrowOnNotOK();

        public static HRESULT TrySymRegisterFunctionEntryCallback64(IntPtr hProcess, PSYMBOL_FUNCENTRY_CALLBACK64 CallbackFunction, long UserContext = 0)
        {
            var result = Native.SymRegisterFunctionEntryCallback64(hProcess, CallbackFunction, UserContext);

            if (!result)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return HRESULT.S_OK;
        }

        #endregion
        #region SymSearch

        public static void SymSearch(
            IntPtr hProcess,
            ulong baseOfDll,
            PSYM_ENUMERATESYMBOLS_CALLBACK enumSymbolsCallback,
            SYMSEARCH options,
            int index = 0,
            SymTagEnum symTag = default,
            string mask = null,
            ulong address = default,
            IntPtr userContext = default
        ) => TrySymSearch(hProcess, baseOfDll, enumSymbolsCallback, options, index, symTag, mask, address, userContext).ThrowOnNotOK();

        public static HRESULT TrySymSearch(
            IntPtr hProcess,
            ulong baseOfDll,
            PSYM_ENUMERATESYMBOLS_CALLBACK enumSymbolsCallback,
            SYMSEARCH options,
            int index = 0,
            SymTagEnum symTag = default,
            string mask = null,
            ulong address = default,
            IntPtr userContext = default)
        {
            var result = Native.SymSearch(
                hProcess,
                baseOfDll,
                index,
                symTag,
                mask,
                address,
                enumSymbolsCallback,
                userContext,
                options
            );

            GC.KeepAlive(enumSymbolsCallback);

            if (!result)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return HRESULT.S_OK;
        }

        #endregion
        #region SymUnloadModule64

        public static void SymUnloadModule64(IntPtr hProcess, long baseOfDll) =>
            TrySymUnloadModule64(hProcess, baseOfDll).ThrowOnNotOK();

        public static HRESULT TrySymUnloadModule64(IntPtr hProcess, long baseOfDll)
        {
            var result = Native.SymUnloadModule64(hProcess, baseOfDll);

            if (!result)
                return (HRESULT) Marshal.GetHRForLastWin32Error();

            return HRESULT.S_OK;
        }

        #endregion
    }
}
