// --------------------------------------------------
// RgssDecrypter.Lib - RgssArchiveV3.cs
// --------------------------------------------------

using System.IO;
using System.Text;

namespace RgssDecrypter.Lib
{
    public class RgssArchiveV3 : RgssArchive
    {
        public RgssArchiveV3(string path) : base(path)
        {
            using (var fs = File.OpenRead(path))
            {
                switch (CheckVersion(path, 3))
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
                    var dKey = new RgssDecryptionKey(9, 3);
                    dKey.PushState(br.ReadUInt32());
                    dKey.Step();

                    while (true)
                    {
                        var fp = new RgssFilePointer();

                        fp.Offset = ReadEncryptedInt(br, dKey);
                        if (fp.Offset == 0)
                            break;

                        fp.Source = this;
                        fp.Size = ReadEncryptedInt(br, dKey);
                        fp.Key = (uint) ReadEncryptedInt(br, dKey);
                        fp.Name = ReadEncryptedString(br, dKey);

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

        private static int ReadEncryptedInt(BinaryReader br, RgssDecryptionKey dKey)
        {
            return DecryptInt(br.ReadInt32(), dKey);
        }

        private static string DecryptString(byte[] input, RgssDecryptionKey key)
        {
            byte[] decBytes = new byte[input.Length];
            var j = 0;
            for (var i = 0; i < decBytes.Length; i++)
            {
                decBytes[i] = (byte) (input[i] ^ ((key.Current() >> 8 * j) & 0xFF));
                j += 1;
                j %= 4;
            }
            return Encoding.UTF8.GetString(decBytes);
        }

        private static int DecryptInt(int input, RgssDecryptionKey key)
        {
            return (int) (input ^ key.Current());
        }
    }
}
