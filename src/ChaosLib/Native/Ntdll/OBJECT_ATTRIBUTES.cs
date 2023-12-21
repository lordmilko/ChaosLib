using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct OBJECT_ATTRIBUTES
    {
        public int Length;
        public IntPtr RootDirectory;
        public UNICODE_STRING* ObjectName;
        public OBJ Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }
}