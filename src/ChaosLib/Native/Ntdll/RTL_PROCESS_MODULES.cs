using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RTL_PROCESS_MODULES
    {
        public int NumberOfModules;
        public IntPtr Modules; //This is a fixed array of RTL_PROCESS_MODULE_INFORMATION. Access a module by doing ((RTL_PROCESS_MODULE_INFORMATION*) &Modules)[i]
    }
}