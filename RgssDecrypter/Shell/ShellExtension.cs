// --------------------------------------------------
// RgssDecrypter - ShellExtension.cs
// --------------------------------------------------

using System;
using System.Reflection;

using Microsoft.Win32;

namespace RgssDecrypter.Shell
{
    public static class ShellExtension
    {
        private const string EXTMENU = "Extract with RGSS Decryptor";
        private const string PROGID = "UsagiRGSSDecryptor.arch";
        private const string PROGID_DESCR = "RPG Maker RGSS Archive";

        private static readonly string[] EXTENSIONS = {".rgssad", ".rgss2a", ".rgss3a"};

        public static void RegisterExtensions()
        {
            RegisterProgID();

            foreach (var ext in EXTENSIONS)
            {
                var extKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + ext);
                extKey.SetValue("", PROGID);
            }

            NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED, NativeMethods.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        public static void UnregisterExtensions()
        {
            UnregisterProgID();

            foreach (var ext in EXTENSIONS)
            {
                var extKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + ext);
                extKey.DeleteValue("", false);
            }

            NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED, NativeMethods.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        private static void RegisterProgID()
        {
            var app = Assembly.GetExecutingAssembly().Location;

            var pidKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + PROGID);
            pidKey.SetValue("", PROGID_DESCR, RegistryValueKind.String);

            var shellKey = pidKey.CreateSubKey("shell");
            shellKey.SetValue("", "open");

            var extractKey = shellKey.CreateSubKey("extract");
            extractKey.SetValue("", EXTMENU);

            var cmdKey = extractKey.CreateSubKey("command");
            cmdKey.SetValue("", $"\"{app}\" \"%1\" -p", RegistryValueKind.String);
        }

        private static void UnregisterProgID()
        {
            var hkcusc = Registry.CurrentUser.CreateSubKey("Software\\Classes\\");
            RegistryKey rk;
            if ((rk = hkcusc.OpenSubKey(PROGID)) != null)
            {
                rk.Close();
                hkcusc.DeleteSubKeyTree(PROGID);
            }
        }
    }
}
