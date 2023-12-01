using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ClrDebug;
using ClrDebug.DIA;

namespace ChaosLib.TypedData
{
    public class DbgHelpTypeInfo
    {
        /* SymGetTypeInfo simply defers to diaGetSymbolInfo, which then basically switches on the enum value we asked for
         * and returns the relevant property from the IDiaSymbol. Ostensibly we can just call SymGetDiaSession ourselves
         * and bypass DbgHelp entirely, however unfortunately DbgHelp's DIA implementation appears to be naughty: its BSTRs
         * seem to have an additional null terminator at the end of them (and their lengths are also different). Thus,
         * we end up with an extra null terminator at the end of our C# strings. No good! And so, we have no choice
         * but to go with Plan B: we'll define a simple wrapper type around DbgHelp that provides easy access to all
         * its typed properties */

        private Dictionary<IMAGEHLP_SYMBOL_TYPE_INFO, object> cache = new Dictionary<IMAGEHLP_SYMBOL_TYPE_INFO, object>();

#if DEBUG
        public DiaSymbol DiaSymbol
        {
            get
            {
                var session = Session.SymGetDiaSession(ModuleBase);

                return session.SymbolById(TypeId);
            }
        }
#endif

        public long ModuleBase { get; }

        public int TypeId { get; }

        public SymTagEnum SymTag => GetPropertyValue<SymTagEnum>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_SYMTAG);

        public int? BaseId => GetPropertyValue<int?>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_TYPEID);

        public BasicType? BasicType => GetPropertyValue<BasicType?>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_BASETYPE);

        public DataKind? DataKind => GetPropertyValue<DataKind?>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_DATAKIND);

        public int? ChildrenCount => GetPropertyValue<int?>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_CHILDRENCOUNT);

        public int Length => (int) GetPropertyValue<long>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_LENGTH);

        public int BitPosition => GetPropertyValue<int>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_BITPOSITION);

        public int Offset => GetPropertyValue<int>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_OFFSET);

        public string SymName => GetPropertyValue<string>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_SYMNAME);

        public bool Nested => GetPropertyValue<bool>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_NESTED);

        public unsafe int[] Children
        {
            get
            {
                if (cache.TryGetValue(IMAGEHLP_SYMBOL_TYPE_INFO.TI_FINDCHILDREN, out var existing))
                    return (int[]) existing;

                if (ChildrenCount > 0)
                {
                    var bufferSize = Marshal.SizeOf<TI_FINDCHILDREN_PARAMS>() + (Marshal.SizeOf<int>() * ChildrenCount.Value);

                    var buffer = Marshal.AllocHGlobal(bufferSize);

                    try
                    {
                        var pChildren = (TI_FINDCHILDREN_PARAMS*)buffer;
                        pChildren->Start = 0;
                        pChildren->Count = ChildrenCount.Value;

                        Session.TrySymGetTypeInfo(ModuleBase, TypeId, IMAGEHLP_SYMBOL_TYPE_INFO.TI_FINDCHILDREN, ref buffer).ThrowOnNotOK();

                        var result = new int[ChildrenCount.Value];

                        Marshal.Copy((IntPtr) pChildren->ChildId, result, 0, pChildren->Count);

                        cache[IMAGEHLP_SYMBOL_TYPE_INFO.TI_FINDCHILDREN] = result;

                        return result;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
                else
                    return null;
            }
        }

        public DbgHelpSession Session { get; }

        public DbgHelpTypeInfo(string expr, DbgHelpSession session)
        {
            var symbol = session.SymGetTypeFromName(0, expr);
            ModuleBase = symbol.ModuleBase;
            TypeId = symbol.TypeIndex;

            Session = session;
        }

        public DbgHelpTypeInfo(long moduleBase, int typeId, DbgHelpSession session)
        {
            //SymFromIndex seems to operate on Index, but we need to operate on TypeIndex

            ModuleBase = moduleBase;
            TypeId = typeId;
            Session = session;
        }

        public T GetPropertyValue<T>(IMAGEHLP_SYMBOL_TYPE_INFO property)
        {
            TryGetPropertyValue<T>(property, out var result).ThrowOnNotOK();
            return result;
        }

        public HRESULT TryGetPropertyValue<T>(IMAGEHLP_SYMBOL_TYPE_INFO property, out T result)
        {
            object Convert(Type type, IntPtr value)
            {
                if (type.IsEnum)
                    return Enum.Parse(type, value.ToInt32().ToString());

                if (type == typeof(int))
                    return value.ToInt32();

                if (type == typeof(long))
                    return value.ToInt64();

                if (type == typeof(bool))
                    return value != IntPtr.Zero;

                if (type == typeof(string))
                {
                    try
                    {
                        return Marshal.PtrToStringUni(value);
                    }
                    finally
                    {
                        Kernel32.Native.LocalFree(value);
                    }
                }

                throw new NotImplementedException($"Don't know how to handle value of type '{type.Name}'.");
            }

            HRESULT hr = HRESULT.S_OK;

            if (!cache.TryGetValue(property, out var existing))
            {
                var underlying = Nullable.GetUnderlyingType(typeof(T));

                if (underlying != null)
                {
                    //The value is optional

                    IntPtr raw = IntPtr.Zero;

                    hr = Session.TrySymGetTypeInfo(ModuleBase, TypeId, property, ref raw);

                    if (hr == HRESULT.S_OK)
                        existing = Convert(underlying, raw);

                    //Ignore any errors
                    hr = HRESULT.S_OK;
                }
                else
                {
                    IntPtr raw = IntPtr.Zero;

                    hr = Session.TrySymGetTypeInfo(ModuleBase, TypeId, property, ref raw);

                    if (hr != HRESULT.S_OK)
                    {
                        result = default;
                        return hr;
                    }

                    existing = Convert(typeof(T), raw);
                }

                cache[property] = existing;
            }

            result = (T) existing;
            return hr;
        }

        public override string ToString()
        {
            return TypeId.ToString();
        }
    }
}