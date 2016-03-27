// --------------------------------------------------
// RgssDecrypter - Program.cs
// --------------------------------------------------

using System;
using System.IO;
using RgssDecrypter.Lib;

namespace RgssDecrypter
{
    partial class Program
    {
        public static string Right(string str, int len)
        {
            if (string.IsNullOrEmpty(str))
                str = string.Empty;
            else if (str.Length > len)
                str = str.Substring(str.Length - len, len);
            return str;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024]; // 16Kb Buffer
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        private static void CreateProjectFile(string rgssDataFile, string outDir)
        {
            var ext = Path.GetExtension(rgssDataFile).ToLowerInvariant();
            switch (ext)
            {
                case ".rgssad":
                {
                    File.WriteAllText(Path.Combine(outDir, "Game.rxproj"), "RPGXP 1.02");
                    break;
                }
                case ".rgss2a":
                {
                    File.WriteAllText(Path.Combine(outDir, "Game.rvproj"), "RPGVX 1.02");
                    break;
                }
                case ".rgss3a":
                {
                    File.WriteAllText(Path.Combine(outDir, "Game.rvproj2"), "RPGVXAce 1.00");
                    break;
                }
                default:
                {
                    Console.WriteLine("\x1B[31mCouldn't Determine RPG Maker Version\x1B[39m");
                    break;
                }
            }
        }

        private static void DumpInfo(RgssArchive archive)
        {
            const string headFormat = "\x1B[39m{0,-6}|{1,-8}|{2,-42}|{3,-10}|{4,-8}\x1B[39m";
            const string rowFormat =
                "\x1B[39m{0,6}\x1B[39m|\x1B[31m{1:X8}\x1B[39m|{2}\x1B[39m|\x1B[40m{3,-10}\x1B[39m|\x1B[32m{4:X8}\x1B[39m";

            var header = string.Format(headFormat, "Num", "Offset", "Name", "Size Bytes", "Key");
            var footer = string.Format(headFormat,
                new string('-', 6),
                new string('-', 8),
                new string('-', 42),
                new string('-', 10),
                new string('-', 8));

            Console.WriteLine(header);
            Console.WriteLine(footer);
            for (int i = 0; i < archive.FilePointers.Count; i++)
            {
                var pointer = archive.FilePointers[i];
                var clipName = Right(pointer.Name, 42);
                var rem = 42 - clipName.Length;
                var nameHighlight
                    = "\x1B[30;1m"
                      + clipName.Insert(clipName.LastIndexOf('\\') + 1, "\x1B[37;1m")
                      + new string(' ', rem)
                      + "\x1B[0m";
                Console.WriteLine(rowFormat, i, pointer.Offset, nameHighlight, pointer.Size, pointer.Key);
            }
            Console.WriteLine(footer);
        }

        private static void ExtractFiles(RgssArchive archive, string outDir, bool supress)
        {
            var fileCount = archive.FilePointers.Count;
            var fileCurr = 0;
            Console.WriteLine("------");
            Console.WriteLine("{0} Files Found", fileCount);
            Console.WriteLine("Output Folder: {0}", outDir);
            Console.WriteLine("------");

            foreach (var pointer in archive.FilePointers)
            {
                var targetPath = Path.Combine(outDir, pointer.Name);

                if (!supress)
                {
                    var digits = Math.Floor(Math.Log10(fileCount) + 1);
                    var fmt = File.Exists(targetPath)
                        ? "\x1B[31m[{1," + digits + "}/{2}] Overwrite:\x1B[37m {0}\x1B[0m\x1B[39m"
                        : "\x1B[32m[{1," + digits + "}/{2}] Create   :\x1B[37m {0}\x1B[0m\x1B[39m";

                    var nameHighlight
                        = "\x1B[1;30m"
                          + pointer.Name.Insert(pointer.Name.LastIndexOf('\\') + 1, "\x1B[37m")
                          + "\x1B[2;37m";

                    Console.WriteLine(fmt,
                        nameHighlight,
                        ++fileCurr,
                        fileCount);
                }

                var targetDir = Path.GetDirectoryName(targetPath);
                Directory.CreateDirectory(targetDir);

                using (var ps = RgssArchive.GetFile(pointer))
                using (var fs = File.Create(targetPath))
                    CopyStream(ps, fs);
            }
        }

        static partial void SubMain(ProgramArguments args)
        {
            if (args.UnregisterContext)
            {
                Shell.ShellExtension.UnregisterExtensions();
                Console.WriteLine("\x1B[31mUnregistered Shell Extension\x1B[39m");
            }
            if (args.RegisterContext)
            {
                Shell.ShellExtension.RegisterExtensions(args);

                Console.WriteLine("\x1B[32mRegistered Shell Extension\x1B[39m");
            }

            RgssArchive archive;
            if (!TryOpenArchive(args, out archive))
                return;

            var baseDir = Path.GetDirectoryName(args.RgssArchive);
            var outDir = Path.Combine(baseDir, args.OutputDir);

            if (args.InfoDump)
            {
                DumpInfo(archive);
            } else
            {
                ExtractFiles(archive, outDir, args.SupressOutput);
                if (args.CreateProjectFile)
                    CreateProjectFile(args.RgssArchive, outDir);
            }
        }

        private static bool TryOpenArchive(ProgramArguments args, out RgssArchive archive)
        {
            var ver = RgssArchive.GetVersion(args.RgssArchive);

            if (string.IsNullOrEmpty(args.RgssArchive))
            {
                archive = null;
                return false;
            }

            switch (ver)
            {
                case 1:
                    archive = new RgssArchiveV1(args.RgssArchive);
                    return true;
                case 3:
                    archive = new RgssArchiveV3(args.RgssArchive);
                    return true;
                case -1:
                    Console.WriteLine("\x1B[31mInvalid RGSSAD Archive\x1B[39m");
                    archive = null;
                    return false;
                default:
                    Console.WriteLine("\x1B[31mUnknown RGSSAD Version\x1B[39m");
                    archive = null;
                    return false;
            }
        }
    }
}