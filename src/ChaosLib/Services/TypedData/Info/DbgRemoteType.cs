using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClrDebug;
using ClrDebug.DIA;
using static ClrDebug.DIA.BasicType;

namespace ChaosLib.TypedData
{
    public class DbgRemoteType
    {
        public string FullName => Module.Name + "!" + Name;

        public string Name { get; }

        public string DisplayName
        {
            get
            {
                var current = this;

                while (current != null)
                {
                    //If any part of our hierarchy is a basic type,
                    //return or name without including the module name
                    if (current.BasicType != null)
                        return Name;

                    current = current.BaseType;
                }

                return FullName;
            }
        }

        /// <summary>
        /// Gets module in which this object's type is defined.
        /// </summary>
        public DbgRemoteModule Module { get; }

        /// <summary>
        /// Gets the type ID 
        /// </summary>
        public int TypeId { get; }

        public SymTagEnum Tag { get; }

        public int Length { get; }

        /// <summary>
        /// Gets the base symbol type of the current type, e.g. if the current
        /// type is a pointer, the base type will be the type of value the pointer points to.
        /// </summary>
        public DbgRemoteType BaseType { get; }

        /// <summary>
        /// Gets the fundamental type this type represents, e.g. an integer, etc.
        /// </summary>
        public BasicType? BasicType { get; }

        public DbgRemoteFieldInfo[] Fields
        {
            get
            {
                var fields = new List<DbgRemoteFieldInfo>();

                var typeInfo = provider.GetTypeInfo(Module.BaseAddress, TypeId);

                while (typeInfo.SymTag == SymTagEnum.PointerType)
                    typeInfo = provider.GetTypeInfo(Module.BaseAddress, typeInfo.BaseId.Value);

                var children = typeInfo.Children;

                if (children == null)
                    return Array.Empty<DbgRemoteFieldInfo>();

                foreach (var child in children)
                {
                    var field = new DbgRemoteFieldInfo(Module.BaseAddress, child, provider);

                    fields.Add(field);
                }

                return fields.ToArray();
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ITypedDataProvider provider;

        public static DbgRemoteType New(string expr, ITypedDataProvider provider)
        {
            var typeInfo = provider.GetTypeInfo(expr);

            return New(typeInfo, provider);
        }

        public static DbgRemoteType New(long moduleBase, int typeId, ITypedDataProvider provider)
        {
            var typeInfo = provider.GetTypeInfo(moduleBase, typeId);

            return New(typeInfo, provider);
        }

        public static DbgRemoteType New(DbgHelpTypeInfo typeInfo, ITypedDataProvider provider)
        {
            var tag = typeInfo.SymTag;
            var baseId = typeInfo.BaseId;

            DbgRemoteType baseType = null;

            if (typeInfo.TryGetPropertyValue<bool>(IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_NESTED, out var nested) == HRESULT.S_OK && nested)
                throw new NotImplementedException("The offset of nested structs has not been testeed. Need to compare against the DbgEngTypedData sample to see if TI_GET_OFFSET we use here takes into consideration the true position within the nested struct.");

            if (baseId != null)
                baseType = New(typeInfo.ModuleBase, baseId.Value, provider);

            //All struct fields (items of type Data) will have a base entry which is the original type. We don't need to store the fact it's a field
            //(if it is a field this type will be contained in a DbgField)
            if (tag == SymTagEnum.Data && baseType != null)
                return baseType;

            return new DbgRemoteType(typeInfo, tag, baseType, provider);
        }

        protected DbgRemoteType(DbgHelpTypeInfo typeInfo, SymTagEnum tag, DbgRemoteType baseType, ITypedDataProvider provider)
        {
            TypeId = typeInfo.TypeId;
            Module = provider.CreateModule(typeInfo.ModuleBase);
            Tag = tag;
            BaseType = baseType;
            this.provider = provider;

            switch (tag)
            {
                case SymTagEnum.BaseType:
                    BasicType = typeInfo.BasicType;
                    Name = GetBasicTypeName(typeInfo.Length);
                    break;

                case SymTagEnum.PointerType:
                    Name = BaseType.Name + "*";
                    break;

                case SymTagEnum.ArrayType:
                    Name = BaseType.Name + "[]";
                    break;

                case SymTagEnum.FunctionType:
                    Name = "<function>";
                    break;

                default:
                    Name = typeInfo.SymName;
                    break;
            }

            //Length
            switch (tag)
            {
                case SymTagEnum.FunctionType:
                    Length = IntPtr.Size;
                    break;

                default:
                    Length = typeInfo.Length;
                    break;
            }
        }

        private string GetBasicTypeName(long length)
        {
            var unknownLength = new NotImplementedException($"Don't know how to handle {BasicType} with length {length}");

            return BasicType switch
            {
                btVoid => "void",
                btChar => "char",
                btWChar => "wchar",
                btInt => length switch
                {
                    4 => "int",
                    _ => throw unknownLength
                },
                btUInt => length switch
                {
                    1 => "UCHAR",
                    2 => "USHORT",
                    8 => "UINT64",
                    _ => throw unknownLength
                },
                btFloat => length switch
                {
                    4 => "float",
                    8 => "double",
                    _ => throw unknownLength
                },
                btULong => "ULONG",
                _ => throw new NotImplementedException($"Don't know how to get the name of a value of type {BasicType}")
            };
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}