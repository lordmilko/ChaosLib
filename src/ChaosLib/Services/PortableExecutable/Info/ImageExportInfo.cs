using System;

namespace ChaosLib.Metadata
{
    public interface IImageExportInfo
    {
        string Name { get; }

        int Index { get; }

        int Ordinal { get; }
    }

    public class ImageForwardedExportInfo : IImageExportInfo
    {
        public string Name { get; }

        public int Index { get; }

        public string TargetFunction { get; }

        public int Ordinal { get; }

        public ImageForwardedExportInfo(string name, int index, string targetFunction, int ordinal)
        {
            Name = name;
            Index = index;
            TargetFunction = targetFunction;
            Ordinal = ordinal;
        }

        public override string ToString()
        {
            return $"{Name} -> {TargetFunction}";
        }
    }

    public class ImageExportInfo : IImageExportInfo
    {
        public string Name { get; }

        public int Index { get; }

        public IntPtr FunctionAddress { get; }

        public int Ordinal { get; }

        public ImageExportInfo(string name, int index, IntPtr functionAddress, int ordinal)
        {
            Name = name;
            Index = index;
            FunctionAddress = functionAddress;
            Ordinal = ordinal;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return Name;

            return Ordinal.ToString();
        }
    }
}
