using System;
using System.Runtime.InteropServices;
using ClrDebug.DIA;

namespace ChaosLib
{
    [Serializable]
    public unsafe struct SymbolInfo
    {
        public string Name { get; }

        public ulong Address { get; }

        public int TypeIndex { get; }

        public int Index { get; }

        public long ModuleBase { get; }

        public SymFlag Flags { get; }

        public SymTagEnum Tag { get; }

        public int Size { get; }

        public SymbolInfo(SYMBOL_INFO* symbolInfo)
        {
            Address = symbolInfo->Address;
            TypeIndex = symbolInfo->TypeIndex;
            Index = symbolInfo->Index;
            ModuleBase = symbolInfo->ModBase;
            Flags = symbolInfo->Flags;
            Tag = symbolInfo->Tag;
            Size = symbolInfo->Size;

            Name = Marshal.PtrToStringAnsi((IntPtr)symbolInfo->Name, symbolInfo->NameLen);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}