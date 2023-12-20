using System.Runtime.InteropServices;

namespace ChaosLib
{
    //This structure is followed by the string contained in TypeName. Thus, a buffer
    //large enough to store OBJECT_TYPE_NAME and the type name must be allocated
    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_TYPE_INFORMATION
    {
        public UNICODE_STRING TypeName;
        public int TotalNumberOfObjects;
        public int TotalNumberOfHandles;
        public int TotalPagedPoolUsage;
        public int TotalNonPagedPoolUsage;
        public int TotalNamePoolUsage;
        public int TotalHandleTableUsage;
        public int HighWaterNumberOfObjects;
        public int HighWaterNumberOfHandles;
        public int HighWaterPagedPoolUsage;
        public int HighWaterNonPagedPoolUsage;
        public int HighWaterNamePoolUsage;
        public int HighWaterHandleTableUsage;
        public int InvalidAttributes;
        public GENERIC_MAPPING GenericMapping;
        public int ValidAccessMask;
        public byte SecurityRequired; //BOOLEAN
        public byte MaintainHandleCount; //BOOLEAN
        public byte TypeIndex;
        public int PoolType;
        public int DefaultPagedPoolCharge;
        public int DefaultNonPagedPoolCharge;
    }
}