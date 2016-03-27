// --------------------------------------------------
// RgssDecrypter.Lib - RgssArchive.cs
// --------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RgssDecrypter.Lib
{
    public abstract class RgssArchive
    {
        public string DataPath { get; }

        protected RgssArchive(string path)
        {
            DataPath = path;
            FilePointers = new List<RgssFilePointer>();
        }

        public static Stream GetFile(RgssFilePointer fp)
        {
            return new RgssFileStream(fp);
        }

        public static int CheckVersion(string path, int target)
        {
            var version = GetVersion(path);
            if (version == -1)
                return -1;

            return version == target
                       ? 1
                       : 0;
        }

        public static int GetVersion(string path)
        {
            if (!File.Exists(path))
                return -1;
            using (var fs = File.OpenRead(path))
            {
                byte[] magic = new byte[8];
                fs.Read(magic, 0, 8);
                return Encoding.ASCII.GetString(magic, 0, 6) != "RGSSAD"
                           ? -1
                           : magic[7];
            }
        }

        public List<RgssFilePointer> FilePointers { get; }
    }
}
