using System;
using Microsoft.Win32.SafeHandles;

namespace ChaosLib.Memory
{
    public class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeThreadHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        public static implicit operator SafeThreadHandle(IntPtr value) => new SafeThreadHandle(value);

        public static implicit operator IntPtr(SafeThreadHandle value) => value.handle;

        protected override bool ReleaseHandle() => Kernel32.CloseHandle(handle);
    }
}
