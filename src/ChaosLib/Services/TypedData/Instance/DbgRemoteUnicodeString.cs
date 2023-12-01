using System;
using System.Text;

namespace ChaosLib.TypedData
{
    public class DbgRemoteUnicodeString : DbgRemoteObject
    {
        public string String { get; }

        public DbgRemoteUnicodeString(long address, DbgRemoteType type, ITypedDataProvider provider) : base(address, type, provider)
        {
            var length = Convert.ToInt32(this["Length"].Value);
            var buffer = Fields["Buffer"].Address;
            var maxLength = Convert.ToInt32(this["MaximumLength"].Value);

            if (buffer != 0 && length < maxLength)
            {
                //LDR_DATA_TABLE_ENTRY.FullDllName can contain garbage data (and a length longer than max length which is bogus)
                if (provider.TryReadVirtual(buffer, length, out var bytes) == ClrDebug.HRESULT.S_OK)
                    String = Encoding.Unicode.GetString(bytes);
            }
        }

        public static implicit operator string(DbgRemoteUnicodeString value) => value.String;

        public override string ToString()
        {
            return String ?? base.ToString();
        }
    }
}