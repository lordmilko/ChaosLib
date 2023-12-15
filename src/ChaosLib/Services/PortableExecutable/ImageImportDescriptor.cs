namespace ChaosLib.Metadata
{
    public interface IImageImportDescriptor
    {
        int OriginalFirstThunk { get; }
        int TimeDateStamp { get; }
        int ForwarderChain { get; }
        int Name { get; }
        int FirstThunk { get; }
    }

    //IMAGE_IMPORT_DESCRIPTOR
    public class ImageImportDescriptor : IImageImportDescriptor
    {
        public int OriginalFirstThunk { get; }
        public int TimeDateStamp { get; }
        public int ForwarderChain { get; }
        public int Name { get; }
        public int FirstThunk { get; }

        public static int Size =
            sizeof(int) + //OriginalFirstThunk
            sizeof(int) + //TimeDateStamp
            sizeof(int) + //ForwarderChain
            sizeof(int) + //Name
            sizeof(int); //FirstThunk

        internal ImageImportDescriptor(PEBinaryReader reader)
        {
            OriginalFirstThunk = reader.ReadInt32();
            TimeDateStamp = reader.ReadInt32();
            ForwarderChain = reader.ReadInt32();
            Name = reader.ReadInt32();
            FirstThunk = reader.ReadInt32();
        }
    }
}