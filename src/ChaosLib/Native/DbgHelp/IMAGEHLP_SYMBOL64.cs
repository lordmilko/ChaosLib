namespace ChaosLib
{
    public unsafe struct IMAGEHLP_SYMBOL64
    {
        public int SizeOfStruct;           // set to sizeof(IMAGEHLP_SYMBOL64)
        public long Address;               // virtual address including dll base address
        public int Size;                   // estimated size of symbol, can be zero
        public int Flags;                  // info about the symbols, see the SYMF defines
        public int MaxNameLength;          // maximum size of symbol name in 'Name'
        public fixed byte Name[1];         // symbol name (null terminated string)
    }
}