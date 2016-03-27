using System;

namespace RgssDecrypter
{
    public class ProgramArguments
    {
        public bool CreateProjectFile { get; set; } = false;
        public bool InfoDump { get; set; } = false;
        public string OutputDir { get; set; } = ".";

        public bool RegisterContext { get; set; } = false;
        public string RgssArchive { get; set; } = String.Empty;
        public bool UnregisterContext { get; set; } = false;
        public bool SupressOutput { get; set; } = false;
    }
}