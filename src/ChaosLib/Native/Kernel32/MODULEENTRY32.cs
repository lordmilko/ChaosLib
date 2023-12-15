using System;

namespace ChaosLib
{
    public unsafe struct MODULEENTRY32
    {
        private const int MAX_MODULE_NAME32 = 255;

        public int dwSize;
        public int th32ModuleID;
        public int th32ProcessID;
        public int GlblcntUsage;
        public int ProccntUsage;
        public IntPtr modBaseAddr;
        public int modBaseSize;
        public IntPtr hModule;
        public fixed byte szModule[MAX_MODULE_NAME32 + 1];
        public fixed byte szExePath[Kernel32.MAX_PATH];
    }
}