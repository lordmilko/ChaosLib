using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct OBJECT_BASIC_INFORMATION
    {
        public int Attributes;
        public int GrantedAccess;
        public int HandleCount;
        public int PointerCount;
        public int PagedPoolCharge;
        public int NonPagedPoolCharge;
        public fixed int Reserved[3];
        public int NameInfoSize;
        public int TypeInfoSize;
        public int SecurityDescriptorSize;
        public LARGE_INTEGER CreationTime;
    }
}