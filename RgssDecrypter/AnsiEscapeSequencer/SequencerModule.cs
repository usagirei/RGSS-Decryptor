// --------------------------------------------------
// RgssDecrypter - SequencerModule.cs
// --------------------------------------------------

namespace RgssDecrypter.AnsiEscapeSequencer
{
    public abstract class SequencerModule
    {
        public abstract void Init();

        public abstract void DeInit();

        public bool Registered { get; set; }
    }
}
