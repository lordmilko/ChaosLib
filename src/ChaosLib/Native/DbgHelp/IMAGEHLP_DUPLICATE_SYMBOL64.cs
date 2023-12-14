namespace ChaosLib
{
    public unsafe struct IMAGEHLP_DUPLICATE_SYMBOL64
    {
        public int SizeOfStruct;           // set to sizeof(IMAGEHLP_DUPLICATE_SYMBOL64)
        public int NumberOfDups;           // number of duplicates in the Symbol array
        public IMAGEHLP_SYMBOL64* Symbol;  // array of duplicate symbols
        public int SelectedSymbol;         // symbol selected (-1 to start)
    }
}