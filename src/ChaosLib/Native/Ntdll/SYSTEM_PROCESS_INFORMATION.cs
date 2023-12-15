using System;
using ClrDebug;

namespace ChaosLib
{
    public struct SYSTEM_PROCESS_INFORMATION
    {
        public int NextEntryOffset;
        public int NumberOfThreads;
        public LARGE_INTEGER SpareLi1;
        public LARGE_INTEGER SpareLi2;
        public LARGE_INTEGER SpareLi3;
        public LARGE_INTEGER CreateTime;
        public LARGE_INTEGER UserTime;
        public LARGE_INTEGER KernelTime;
        public UNICODE_STRING ImageName;
        public int BasePriority;
        public IntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
        public int HandleCount;
        public int SessionId;
        public IntPtr PageDirectoryBase;
        public IntPtr PeakVirtualSize;
        public IntPtr VirtualSize;
        public int PageFaultCount;
        public IntPtr PeakWorkingSetSize;
        public IntPtr WorkingSetSize;
        public IntPtr QuotaPeakPagedPoolUsage;
        public IntPtr QuotaPagedPoolUsage;
        public IntPtr QuotaPeakNonPagedPoolUsage;
        public IntPtr QuotaNonPagedPoolUsage;
        public IntPtr PagefileUsage;
        public IntPtr PeakPagefileUsage;
        public IntPtr PrivatePageCount;
        public LARGE_INTEGER ReadOperationCount;
        public LARGE_INTEGER WriteOperationCount;
        public LARGE_INTEGER OtherOperationCount;
        public LARGE_INTEGER ReadTransferCount;
        public LARGE_INTEGER WriteTransferCount;
        public LARGE_INTEGER OtherTransferCount;
    }
}