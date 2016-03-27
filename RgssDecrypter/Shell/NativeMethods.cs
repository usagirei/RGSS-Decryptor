// --------------------------------------------------
// RgssDecrypter - NativeMethods.cs
// --------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace RgssDecrypter.Shell
{
    public static class NativeMethods
    {
        public const int SHCNE_ASSOCCHANGED = 0x08000000;
        public const int SHCNF_IDLIST = 0x0000;

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
