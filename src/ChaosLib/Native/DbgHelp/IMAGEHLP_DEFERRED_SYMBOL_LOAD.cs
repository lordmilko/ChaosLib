using System;
using System.Runtime.InteropServices;

namespace ChaosLib
{
    public unsafe struct IMAGEHLP_DEFERRED_SYMBOL_LOAD64
    {
        public int SizeOfStruct;
        public long BaseOfImage;
        public int CheckSum;
        public int TimeDateStamp;
        public fixed byte FileName[260];
        public byte Reparse; //BOOLEAN. I'm not sure if it's a good idea to cast to a struct pointer containing custom marshalling
        public IntPtr hFile;

        public override string ToString()
        {
            fixed (byte* b = FileName)
                return Marshal.PtrToStringAnsi((IntPtr) b) ?? base.ToString();
        }
    }
}