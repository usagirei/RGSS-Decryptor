// --------------------------------------------------
// RgssDecrypter.Lib - RgssFileStream.cs
// --------------------------------------------------

using System;
using System.IO;

namespace RgssDecrypter.Lib
{
    public class RgssFileStream : Stream
    {
        private readonly RgssDecryptionKey _decKey;
        private readonly RangeStream _fileRange;
        private readonly FileStream _fileStream;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _fileRange.Length;

        public override long Position
        {
            get { return _fileRange.Position; }
            set { _fileRange.Position = value; }
        }

        public RgssFileStream(RgssFilePointer fp)
        {
            _fileStream = File.OpenRead(fp.Source.DataPath);
            _decKey = new RgssDecryptionKey(7, 3);
            _decKey.PushState(fp.Key);
            _fileRange = new RangeStream(_fileStream, fp.Offset, fp.Size);
        }

        public override void Flush()
        {
            _fileRange.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _fileRange.Read(buffer, offset, count);

            int j = 0;
            for (int i = 0; i < read; i++)
            {
                buffer[offset + i] = (byte) (buffer[offset + i] ^ ((_decKey.Current() >> 8 * j) & 0xFF));

                if ((j = ++j % 4) == 0)
                    _decKey.Step();
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            _fileRange.Dispose();
            _fileStream.Dispose();
        }
    }
}
