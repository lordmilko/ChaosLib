using System;

namespace ChaosLib
{
    [Flags]
    public enum TH32CS : uint
    {
        /// <summary>
        /// Includes all heaps of the process specified in th32ProcessID in the snapshot. To enumerate the heaps, see Heap32ListFirst.
        /// </summary>
        TH32CS_SNAPHEAPLIST = 0x00000001,

        /// <summary>
        /// Includes all processes in the system in the snapshot. To enumerate the processes, see Process32First.
        /// </summary>
        TH32CS_SNAPPROCESS = 0x00000002,

        /// <summary>
        /// Includes all threads in the system in the snapshot. To enumerate the threads, see Thread32First.<para/>
        /// To identify the threads that belong to a specific process, compare its process identifier to the th32OwnerProcessID member of the THREADENTRY32 structure when enumerating the threads.
        /// </summary>
        TH32CS_SNAPTHREAD = 0x00000004,

        /// <summary>
        /// Includes all modules of the process specified in th32ProcessID in the snapshot. To enumerate the modules, see Module32First.
        /// If the function fails with ERROR_BAD_LENGTH, retry the function until it succeeds.<para/>
        /// 
        /// 64-bit Windows:  Using this flag in a 32-bit process includes the 32-bit modules of the process specified in th32ProcessID, while using it in a 64-bit process includes the 64-bit modules.
        /// To include the 32-bit modules of the process specified in th32ProcessID from a 64-bit process, use the TH32CS_SNAPMODULE32 flag.
        /// </summary>
        TH32CS_SNAPMODULE = 0x00000008,

        /// <summary>
        /// Includes all 32-bit modules of the process specified in th32ProcessID in the snapshot when called from a 64-bit process.
        /// This flag can be combined with TH32CS_SNAPMODULE or TH32CS_SNAPALL. If the function fails with ERROR_BAD_LENGTH, retry the function until it succeeds.
        /// </summary>
        TH32CS_SNAPMODULE32 = 0x00000010,

        /// <summary>
        /// Indicates that the snapshot handle is to be inheritable.
        /// </summary>
        TH32CS_INHERIT = 0x80000000,

        /// <summary>
        /// Includes all processes and threads in the system, plus the heaps and modules of the process specified in th32ProcessID.<para/>
        /// Equivalent to specifying the TH32CS_SNAPHEAPLIST, TH32CS_SNAPMODULE, TH32CS_SNAPPROCESS, and TH32CS_SNAPTHREAD values combined using an OR operation ('|').
        /// </summary>
        TH32CS_SNAPALL = TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD
    }
}