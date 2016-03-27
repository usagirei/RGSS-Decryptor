// --------------------------------------------------
// RgssDecrypter.Lib - RgssFilePointer.cs
// --------------------------------------------------

namespace RgssDecrypter.Lib
{
    public struct RgssFilePointer
    {
        public long Offset;
        public int Size;
        public string Name;
        public uint Key;
        public RgssArchive Source;

        public override string ToString()
        {
            return $"{Offset:X8}:\"{Name}\"@{Size}";
        }
    }
}
