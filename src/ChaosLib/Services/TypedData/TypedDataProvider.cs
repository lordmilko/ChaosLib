using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClrDebug;

namespace ChaosLib.TypedData
{
    public static class TypedDataProviderExtensions
    {
        public static DbgRemoteObject CreateObject(this ITypedDataProvider provider, IntPtr address, string expr) =>
            provider.CreateObject((long) address, expr);
    }

    public interface ITypedDataProvider
    {
        DbgRemoteObject CreateObject(long address, string expr);

        DbgHelpTypeInfo GetTypeInfo(string expr);

        DbgHelpTypeInfo GetTypeInfo(long moduleBase, int index);

        DbgRemoteModule CreateModule(long moduleBase);

        byte[] ReadVirtual(long address, int size);

        long ReadPointer(long address);

        HRESULT TryReadVirtual(long address, int size, out byte[] value);
    }

    public class TypedDataProvider : ITypedDataProvider
    {
        private DbgHelpSession dbgHelpSession;
        private MemoryReader reader;

        public TypedDataProvider(DbgHelpSession dbgHelpSession)
        {
            if (dbgHelpSession == null)
                throw new ArgumentNullException(nameof(dbgHelpSession));

            //If DbgHelp was loaded from system32, this very strongly indicates a bug and that we won't
            //be able to load any symbols, so detect this and throw an error.

            var dbgHelpModule = Process.GetCurrentProcess().Modules
                .Cast<ProcessModule>()
                .FirstOrDefault(m => StringComparer.OrdinalIgnoreCase.Equals(m.ModuleName, "dbghelp.dll"));

            if (dbgHelpModule != null)
            {
                var dir = Path.GetDirectoryName(dbgHelpModule.FileName);

                var system32 = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "system32");

                //There won't be a trailing slash on dir
                if (StringComparer.OrdinalIgnoreCase.Equals(dir, system32))
                    throw new InvalidOperationException($"DbgHelp.dll has been loaded from system32; symsrv will be unavailable. Consider calling SetDllDirectory prior to creating your {nameof(DbgHelpSession)}, or explicitly load DbgHelp.dll first from a location containing symsrv.dll");
            }

            this.dbgHelpSession = dbgHelpSession;
            reader = new MemoryReader(dbgHelpSession.hProcess);
        }

        public DbgRemoteObject CreateObject(long address, string expr) =>
            new DbgRemoteObject(address, expr, this);

        public DbgHelpTypeInfo GetTypeInfo(string expr) =>
            new DbgHelpTypeInfo(expr, dbgHelpSession);

        public DbgHelpTypeInfo GetTypeInfo(long moduleBase, int typeId) =>
            dbgHelpSession.GetTypeInfo(moduleBase, typeId);

        public DbgRemoteModule CreateModule(long moduleBase)
        {
            var info = dbgHelpSession.SymGetModuleInfo64(moduleBase);

            return new DbgRemoteModule(info.ModuleName, moduleBase);
        }

        public byte[] ReadVirtual(long address, int size) =>
            reader.ReadVirtual(address, size);

        public long ReadPointer(long address) =>
            reader.ReadPointer(address);

        public HRESULT TryReadVirtual(long address, int size, out byte[] value) =>
            reader.TryReadVirtual(address, size, out value);
    }
}