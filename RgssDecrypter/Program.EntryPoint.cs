// --------------------------------------------------
// RgssDecrypter - Program.EntryPoint.cs
// --------------------------------------------------

//#define ANSI

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using RgssDecrypter.AnsiEscapeSequencer;
using RgssDecrypter.AnsiEscapeSequencer.Modules;
using RgssDecrypter.Options;

namespace RgssDecrypter
{
    partial class Program
    {
        public static bool IsCurrentOSContains(string name)
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string productName = (string) reg.GetValue("ProductName");

            return productName.Contains(name);
        }

        public static bool IsWindows10() { return IsCurrentOSContains("Windows 10"); }

        private static T GetAttribute<T>(ICustomAttributeProvider t)
        {
            var attribs = t.GetCustomAttributes(typeof (T), true);
            return (T) (attribs.Length > 0
                ? attribs[0]
                : null);
        }

        static void Main(string[] args)
        {
#if !ANSI
            if (!IsWindows10()) {
                AnsiSequencer.Enable();
                AnsiSequencer.EnableModule<SGRModule>();
            }
#else
            AnsiSequencer.Enable();
            AnsiSequencer.EnableModule<SGRModule>();
#endif

            PrintHeader();

            var unparsed = new List<string>();
            var opts = new OptionSet();
            var argsObj = new ProgramArguments();

            opts.Add("<>",
                arg =>
                {
                    var m = OptionSet.ValueOptionRegex.Match(arg);
                    if (m.Success) {
                        PrintInvalid(m.Groups["name"].Value);
                        Environment.Exit(1);
                    } else if (File.Exists(arg)) {
                        argsObj.RgssArchive = arg;
                    } else {
                        unparsed.Add(arg);
                    }
                }
                );

            opts.Add("?|help",
                "Displays this help message",
                arg =>
                {
                    PrintHelp(opts);
                    Environment.Exit(0);
                });

            opts.Add("d|dump",
                $"Only Dumps Archive Information\n(Default: {argsObj.InfoDump})",
                arg => argsObj.InfoDump = true);

            opts.Add("o|output=",
                $"Output Directory, Relative to RGSS Archive.\n(Default: {argsObj.OutputDir})",
                arg => argsObj.OutputDir = arg);

            opts.Add("p|proj",
                $"Creates Project File.\n(Default: {argsObj.CreateProjectFile})",
                arg => argsObj.CreateProjectFile = true);

            opts.Add("q|quiet",
                $"Supresses Output. \n(Default: {argsObj.SupressOutput})",
                arg => argsObj.SupressOutput = true);

            opts.Add("r|register",
                "Registers Context Menu Handler",
                arg => argsObj.RegisterContext = true);

            opts.Add("u|unregister",
                "Unregisters Context Menu Handler",
                arg => argsObj.UnregisterContext = true);

            if (args.Length == 0) {
                PrintHelp(opts);
                Environment.Exit(1);
            }

            var ext = opts.Parse(args);
            SubMain(argsObj);
        }

        private static void PrintHeader()
        {
            var assName = GetAttribute<AssemblyFileVersionAttribute>(typeof (Program).Assembly);
            var comName = GetAttribute<AssemblyCompanyAttribute>(typeof (Program).Assembly);
            const string headerFmt = "--- RGSS Decryptor {0} - by {1} ---";
            var header = string.Format(headerFmt,
                assName.Version,
                comName.Company);
            var len = header.Length;
            header = "\x1B[" + ((Console.BufferWidth - len) / 2) + "G" + header;
            Console.WriteLine(header);
        }

        private static void PrintHelp(OptionSet opts)
        {
            var exeName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Console.WriteLine($"Usage: {exeName} [options] <RgssArch>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            opts.WriteOptionDescriptions(Console.Out);
        }

        private static void PrintInvalid(string flag = null)
        {
            var exeName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            if (!string.IsNullOrEmpty(flag))
                Console.WriteLine($"Unrecognized Option '{flag}'.");
            Console.WriteLine($"The syntax of the command is incorrect.\nCall '{exeName} /?' for more information.");
        }

        static partial void SubMain(ProgramArguments args);
    }
}