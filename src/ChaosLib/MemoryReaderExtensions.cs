using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    public static class MemoryReaderExtensions
    {
        public static T ReadVirtual<T>(this MemoryReader reader, CLRDATA_ADDRESS address) where T : struct
        {
            T value;
            TryReadVirtual(reader, address, out value).ThrowOnNotOK();
            return value;
        }

        public static HRESULT TryReadVirtual<T>(this MemoryReader reader, CLRDATA_ADDRESS address, out T value) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buffer = Marshal.AllocHGlobal(size);

            try
            {
                int read;
                var hr = reader.ReadVirtual(address, buffer, size, out read);

                if (hr == HRESULT.S_OK)
                    value = Marshal.PtrToStructure<T>(buffer);
                else
                    value = default(T);

                return hr;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static byte[] ReadVirtual(this MemoryReader reader, CLRDATA_ADDRESS address, int size)
        {
            byte[] value;
            TryReadVirtual(reader, address, size, out value).ThrowOnNotOK();
            return value;
        }

        public static HRESULT TryReadVirtual(this MemoryReader reader, CLRDATA_ADDRESS address, int size, out byte[] value)
        {
            var buffer = Marshal.AllocHGlobal(size);

            try
            {
                int read;
                var hr = reader.ReadVirtual(address, buffer, size, out read);

                if (hr == HRESULT.S_OK)
                {
                    value = new byte[read];
                    Marshal.Copy(buffer, value, 0, read);
                }
                else
                    value = null;

                return hr;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}