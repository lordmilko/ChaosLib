using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct LDR_DLL_NOTIFICATION
    {
        [FieldOffset(0)]
        public LDR_DLL_LOADED_NOTIFICATION_DATA Loaded;

        [FieldOffset(0)]
        public LDR_DLL_UNLOADED_NOTIFICATION_DATA Unloaded;
    }
}