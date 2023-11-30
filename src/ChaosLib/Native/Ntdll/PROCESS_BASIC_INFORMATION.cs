using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    //PEB is an ever changing and volatile structure, and is only defined here in minimal form for the purpose of retrieving the most
    //stable members. For a table that illustrates the evolution of the PEB, please see http://blog.rewolf.pl/blog/wp-content/uploads/2013/03/PEB_Evolution.pdf
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PEB
    {
        public fixed byte Reserved1[2]; //as of writing, has always been InheritedAddressSpace and ReadImageFileExecOptions
        public byte BeingDebugged;
        public fixed byte Reserved2[1];

        public IntPtr Mutant;
        public IntPtr ImageBaseAddress;
        public IntPtr Ldr;
        public IntPtr ProcessParameters;
        public IntPtr SubSystemData;
        public IntPtr ProcessHeap;

        //Other possible members omitted
    }

    //PROCESS_BASIC_INFORMATION is 6 "pointers worth" in size.
    //Strictly speaking, there are two definitions: PROCESS_BASIC_INFORMATION
    //and PROCESS_BASIC_INFORMATION64, with the main difference being that after
    //the "ExitStatus" and "BasePriority" fields, there is padding in the 64-bit version
    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_BASIC_INFORMATION
    {
        public IntPtr Reserved1; //NTSTATUS ExitStatus (+ padding in 64-bit process)
        public IntPtr PebBaseAddress;
        public IntPtr AffinityMask;
        public IntPtr Reserved2; //KPRIORITY (int) BasePriority (+ padding in 64-bit process)
        public IntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
    }
}