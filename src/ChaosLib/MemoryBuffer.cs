using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    public class MemoryBuffer : IDisposable
    {
        protected IntPtr value;
        private bool disposed;

        public MemoryBuffer(int size)
        {
            if (size == 0)
                throw new ArgumentException("Size cannot be 0");

            value = Marshal.AllocHGlobal(size);
        }

        public MemoryBuffer(IntPtr value)
        {
            this.value = value;
        }

        public static implicit operator IntPtr(MemoryBuffer buffer) => buffer.value;

        public virtual void Dispose()
        {
            if (disposed)
                return;

            if (value != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(value);
                value = IntPtr.Zero;
            }

            disposed = true;
        }
    }

    public class MemoryBuffer<T> : MemoryBuffer
    {
        public MemoryBuffer(in T value) : base(Marshal.SizeOf<T>())
        {
            Marshal.StructureToPtr(value, this.value, false);
        }

        public override void Dispose()
        {
            Marshal.DestroyStructure(value, typeof(T));

            base.Dispose();
        }
    }
}
