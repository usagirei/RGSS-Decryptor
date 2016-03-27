// --------------------------------------------------
// RgssDecrypter.Lib - RgssArchiveV1.cs
// --------------------------------------------------

using System.IO;
using System.Text;

namespace RgssDecrypter.Lib
{
    public class RgssArchiveV1 : RgssArchive
    {
        public RgssArchiveV1(string path) : base(path)
        {
            using (var fs = File.OpenRead(path))
            {
                switch (CheckVersion(path, 1))
                {
                    case -1:
                        throw new InvalidDataException("Invalid RGSS Data");
                    case 0:
                        throw new InvalidDataException("Invalid RGSSAD Version");
                    case 1:
                    default:
                        break;
                }
                using (var br = new BinaryReader(fs))
                {
                    fs.Position = 8;
                    var dKey = new RgssDecryptionKey(7, 3);
                    dKey.PushState(0xDEADCAFE);

                    while (fs.Length - fs.Position > 0)
                    {
                        var fp = new RgssFilePointer();

                        fp.Source = this;
                        fp.Name = ReadEncryptedString(br, dKey);
                        fp.Size = ReadEncryptedInt(br, dKey);
                        fp.Offset = fs.Position;
                        fp.Key = dKey.Current();
                        fs.Position += fp.Size;

                        FilePointers.Add(fp);
                    }
                }
            }
        }

        private static string ReadEncryptedString(BinaryReader br, RgssDecryptionKey key)
        {
            int dataLenght = ReadEncryptedInt(br, key);
            var bytes = br.ReadBytes(dataLenght);
            return DecryptString(bytes, key);
        }

        private static int ReadEncryptedInt(BinaryReader br, RgssDecryptionKey key)
        {
            return DecryptInt(br.ReadInt32(), key);
        }

        private static string DecryptString(byte[] input, RgssDecryptionKey key)
        {
            byte[] decBytes = new byte[input.Length];
            for (int i = 0; i < decBytes.Length; i++)
            {
                decBytes[i] = (byte) (input[i] ^ (key.Current() & 0xFF));
                key.Step();
            }
            return Encoding.UTF8.GetString(decBytes);
        }

        private static int DecryptInt(int input, RgssDecryptionKey key)
        {
            var dec = (int) (input ^ key.Current());
            key.Step();
            return dec;
        }
    }
}
