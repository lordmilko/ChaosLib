using System;
using System.Runtime.InteropServices;
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

        public long ReadPointer(long address)
        {
            if (IntPtr.Size == 4)
                return reader.ReadVirtual<int>(address);
            else
                return reader.ReadVirtual<long>(address);
        }

        public HRESULT TryReadVirtual(long address, int size, out byte[] value) =>
            reader.TryReadVirtual(address, size, out value);
    }
}