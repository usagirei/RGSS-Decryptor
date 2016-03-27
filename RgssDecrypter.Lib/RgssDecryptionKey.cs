// --------------------------------------------------
// RgssDecrypter.Lib - RgssDecryptionKey.cs
// --------------------------------------------------

namespace RgssDecrypter.Lib
{
    public class RgssDecryptionKey
    {
        private readonly byte _multipliter;
        private readonly byte _accumulator;
        private uint _state;

        public RgssDecryptionKey(byte mult, byte accu)
        {
            _multipliter = mult;
            _accumulator = accu;
        }

        public uint Current()
        {
            return _state;
        }

        public void PushState(uint state)
        {
            _state = state;
        }

        public void Step()
        {
            _state *= _multipliter;
            _state += _accumulator;
        }
    }
}
