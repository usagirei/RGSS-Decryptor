// --------------------------------------------------
// RgssDecrypter.Lib - RangeStream.cs
// --------------------------------------------------

using System;
using System.IO;

namespace RgssDecrypter.Lib
{
    public class RangeStream : Stream
    {
        public Stream BaseStream { get; private set; }

        public override bool CanRead => BaseStream.CanRead;
        public override bool CanSeek => BaseStream.CanSeek;
        public override bool CanWrite => BaseStream.CanWrite;
        public override long Length => RangeLenght;

        public override long Position
        {
            get { return BaseStream.Position - RangeOffset; }
            set
            {
                var min = RangeOffset;
                var max = RangeOffset + RangeLenght;
                var target = RangeOffset + value;
                var clamp = Math.Max(min, Math.Min(target, max));
                BaseStream.Position = clamp;
            }
        }

        public int RangeLenght { get; }
        public long RangeOffset { get; }

        public RangeStream(Stream baseStream, long offset, int lenght)
        {
            BaseStream = baseStream;
            RangeLenght = lenght;
            RangeOffset = offset;
            BaseStream.Position = offset;
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            BaseStream = null;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Truncate Read
            var max = (int) (RangeOffset + RangeLenght);
            var available = (int) Math.Max(max - BaseStream.Position, 0);
            var toRead = Math.Min(count, available);

            return BaseStream.Read(buffer, offset, toRead);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = RangeLenght + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Truncate Write
            var max = (int) (RangeOffset + RangeLenght);
            var available = (int) Math.Max(max - BaseStream.Position, 0);
            var toWrite = Math.Min(count, available);

            BaseStream.Write(buffer, offset, toWrite);
        }
    }
}
