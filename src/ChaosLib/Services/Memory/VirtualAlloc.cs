using System;
using ClrDebug;

namespace ChaosLib.Memory
{
    /// <summary>
    /// Represents an area of memory that has been allocated by VirtualAllocEx.
    /// </summary>
    public class VirtualAlloc : IDisposable
    {
        private IntPtr hProcess;
        private IntPtr address;

        public VirtualAlloc(IntPtr hProcess, int size, AllocationType type, MemoryProtection protection)
        {
            this.hProcess = hProcess;
            address = Kernel32.VirtualAllocEx(this.hProcess, size, type, protection);
        }

        public static implicit operator IntPtr(VirtualAlloc value) => value.address;

        public static implicit operator CLRDATA_ADDRESS(VirtualAlloc value) => value.address;

        public void Dispose()
        {
            Kernel32.VirtualFreeEx(hProcess, address);
        }
    }
}
