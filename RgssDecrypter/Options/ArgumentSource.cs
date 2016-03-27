// --------------------------------------------------
// RgssDecrypter - ArgumentSource.cs
// --------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RgssDecrypter.Options
{
    public abstract class ArgumentSource
    {
        public abstract string Description { get; }

        protected ArgumentSource() {}

        public static IEnumerable<string> GetArguments(TextReader reader)
        {
            return GetArguments(reader, false);
        }

        public static IEnumerable<string> GetArgumentsFromFile(string file)
        {
            return GetArguments(File.OpenText(file), true);
        }

        // Cribbed from mcs/driver.cs:LoadArgs(string)
        private static IEnumerable<string> GetArguments(TextReader reader, bool close)
        {
            try
            {
                StringBuilder arg = new StringBuilder();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    int t = line.Length;

                    for (int i = 0; i < t; i++)
                    {
                        char c = line[i];

                        if (c == '"' || c == '\'')
                        {
                            char end = c;

                            for (i++; i < t; i++)
                            {
                                c = line[i];

                                if (c == end)
                                    break;
                                arg.Append(c);
                            }
                        }
                        else if (c == ' ')
                        {
                            if (arg.Length > 0)
                            {
                                yield return arg.ToString();
                                arg.Length = 0;
                            }
                        }
                        else
                            arg.Append(c);
                    }
                    if (arg.Length > 0)
                    {
                        yield return arg.ToString();
                        arg.Length = 0;
                    }
                }
            }
            finally
            {
                if (close)
                    reader.Close();
            }
        }

        public abstract bool GetArguments(string value, out IEnumerable<string> replacement);

        public abstract string[] GetNames();
    }
}
