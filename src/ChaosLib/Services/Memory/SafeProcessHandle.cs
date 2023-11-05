using System;
using Microsoft.Win32.SafeHandles;

namespace ChaosLib.Memory
{
    public class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeProcessHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        public static implicit operator SafeProcessHandle(IntPtr value) => new SafeProcessHandle(value);

        public static implicit operator IntPtr(SafeProcessHandle value) => value.handle;

        protected override bool ReleaseHandle() => Kernel32.CloseHandle(handle);
    }
}