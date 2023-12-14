using System;

namespace ChaosLib
{
    public unsafe struct IMAGEHLP_CBA_EVENT
    {
        public enum Sev
        {
            sevInfo = 0,
            sevProblem = 1,
            sevAttn = 2,
            sevFatal = 3
        }

        public Sev severity;
        public int code;
        public byte* desc;
        public IntPtr @object;
    }
}