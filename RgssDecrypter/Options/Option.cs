// --------------------------------------------------
// RgssDecrypter - Option.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RgssDecrypter.Options
{
    //
    // A Getopt::Long-inspired option parsing library for C#.
    //
    // NDesk.Options.OptionSet is built upon a key/value table, where the
    // key is a option format string and the value is a delegate that is 
    // invoked when the format string is matched.
    //
    // Option format strings:
    //  Regex-like BNF Grammar: 
    //    name: .+
    //    type: [=:]
    //    sep: ( [^{}]+ | '{' .+ '}' )?
    //    aliases: ( name type sep ) ( '|' name type sep )*
    // 
    // Each '|'-delimited name is an alias for the associated action.  If the
    // format string ends in a '=', it has a required value.  If the format
    // string ends in a ':', it has an optional value.  If neither '=' or ':'
    // is present, no value is supported.  `=' or `:' need only be defined on one
    // alias, but if they are provided on more than one they must be consistent.
    //
    // Each alias portion may also end with a "key/value separator", which is used
    // to split option values if the option accepts > 1 value.  If not specified,
    // it defaults to '=' and ':'.  If specified, it can be any character except
    // '{' and '}' OR the *string* between '{' and '}'.  If no separator should be
    // used (i.e. the separate values should be distinct arguments), then "{}"
    // should be used as the separator.
    //
    // Options are extracted either from the current option by looking for
    // the option name followed by an '=' or ':', or is taken from the
    // following option IFF:
    //  - The current option does not contain a '=' or a ':'
    //  - The current option requires a value (i.e. not a Option type of ':')
    //
    // The `name' used in the option format string does NOT include any leading
    // option indicator, such as '-', '--', or '/'.  All three of these are
    // permitted/required on any named option.
    //
    // Option bundling is permitted so long as:
    //   - '-' is used to start the option group
    //   - all of the bundled options are a single character
    //   - at most one of the bundled options accepts a value, and the value
    //     provided starts from the next character to the end of the string.
    //
    // This allows specifying '-a -b -c' as '-abc', and specifying '-D name=value'
    // as '-Dname=value'.
    //
    // Option processing is disabled by specifying "--".  All options after "--"
    // are returned by OptionSet.Parse() unchanged and unprocessed.
    //
    // Unprocessed options are returned from OptionSet.Parse().
    //
    // Examples:
    //  int verbose = 0;
    //  OptionSet p = new OptionSet ()
    //    .Add ("v", v => ++verbose)
    //    .Add ("name=|value=", v => Console.WriteLine (v));
    //  p.Parse (new string[]{"-v", "--v", "/v", "-name=A", "/name", "B", "extra"});
    //
    // The above would parse the argument string array, and would invoke the
    // lambda expression three times, setting `verbose' to 3 when complete.  
    // It would also print out "A" and "B" to standard output.
    // The returned array would contain the string "extra".
    //
    // C# 3.0 collection initializers are supported and encouraged:
    //  var p = new OptionSet () {
    //    { "h|?|help", v => ShowHelp () },
    //  };
    //
    // System.ComponentModel.TypeConverter is also supported, allowing the use of
    // custom data types in the callback type; TypeConverter.ConvertFromString()
    // is used to convert the value option to an instance of the specified
    // type:
    //
    //  var p = new OptionSet () {
    //    { "foo=", (Foo f) => Console.WriteLine (f.ToString ()) },
    //  };
    //
    // Random other tidbits:
    //  - Boolean options (those w/o '=' or ':' in the option format string)
    //    are explicitly enabled if they are followed with '+', and explicitly
    //    disabled if they are followed with '-':
    //      string a = null;
    //      var p = new OptionSet () {
    //        { "a", s => a = s },
    //      };
    //      p.Parse (new string[]{"-a"});   // sets v != null
    //      p.Parse (new string[]{"-a+"});  // sets v != null
    //      p.Parse (new string[]{"-a-"});  // sets v == null
    //

    public abstract class Option
    {
        private static readonly char[] NameTerminator = new[]
        {
            '=', ':'
        };

        public string Description { get; }

        public bool Hidden { get; }

        public int MaxValueCount { get; }

        internal string[] Names { get; }

        public OptionValueType OptionValueType { get; }

        public string Prototype { get; }

        internal string[] ValueSeparators { get; private set; }

        protected Option(string prototype, string description)
            : this(prototype, description, 1, false) {}

        protected Option(string prototype, string description, int maxValueCount)
            : this(prototype, description, maxValueCount, false) {}

        protected Option(string prototype, string description, int maxValueCount, bool hidden)
        {
            if (prototype == null)
                throw new ArgumentNullException(nameof(prototype));
            if (prototype.Length == 0)
                throw new ArgumentException("Cannot be the empty string.", nameof(prototype));
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValueCount));

            Prototype = prototype;
            Description = description;
            MaxValueCount = maxValueCount;
            Names = (this is OptionSet.Category)
                        // append GetHashCode() so that "duplicate" categories have distinct
                        // names, e.g. adding multiple "" categories should be valid.
                        ? new[]
                        {
                            prototype + GetHashCode()
                        }
                        : prototype.Split('|');

            if (this is OptionSet.Category)
                return;

            OptionValueType = ParsePrototype();
            Hidden = hidden;

            if (MaxValueCount == 0 && OptionValueType != OptionValueType.None)
                throw new ArgumentException(
                    "Cannot provide maxValueCount of 0 for OptionValueType.Required or " +
                    "OptionValueType.Optional.",
                    nameof(maxValueCount));
            if (OptionValueType == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException(
                    $"Cannot provide maxValueCount of {maxValueCount} for OptionValueType.None.",
                    nameof(maxValueCount));
            if (Array.IndexOf(Names, "<>") >= 0 &&
                ((Names.Length == 1 && OptionValueType != OptionValueType.None) ||
                 (Names.Length > 1 && MaxValueCount > 1)))
                throw new ArgumentException(
                    "The default option handler '<>' cannot require values.",
                    nameof(prototype));
        }

        protected static T Parse<T>(string value, OptionContext c)
        {
            Type tt = typeof(T);
            bool nullable = tt.IsValueType && tt.IsGenericType &&
                            !tt.IsGenericTypeDefinition &&
                            tt.GetGenericTypeDefinition() == typeof(Nullable<>);
            Type targetType = nullable ? tt.GetGenericArguments()[0] : typeof(T);
            TypeConverter conv = TypeDescriptor.GetConverter(targetType);
            T t = default(T);
            try
            {
                if (value != null)
                    t = (T) conv.ConvertFromString(value);
            }
            catch (Exception e)
            {
                throw new OptionException(
                    string.Format(
                                  c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."),
                        value,
                        targetType.Name,
                        c.OptionName),
                    c.OptionName,
                    e);
            }
            return t;
        }

        private static void AddSeparators(string name, int end, ICollection<string> seps)
        {
            int start = -1;
            for (int i = end + 1; i < name.Length; ++i)
            {
                switch (name[i])
                {
                    case '{':
                        if (start != -1)
                            throw new ArgumentException(
                                $"Ill-formed name/value separator found in \"{name}\".",
                                "prototype");
                        start = i + 1;
                        break;
                    case '}':
                        if (start == -1)
                            throw new ArgumentException(
                                $"Ill-formed name/value separator found in \"{name}\".",
                                "prototype");
                        seps.Add(name.Substring(start, i - start));
                        start = -1;
                        break;
                    default:
                        if (start == -1)
                            seps.Add(name[i].ToString());
                        break;
                }
            }
            if (start != -1)
                throw new ArgumentException(
                    $"Ill-formed name/value separator found in \"{name}\".",
                    "prototype");
        }

        public string[] GetNames()
        {
            return (string[]) Names.Clone();
        }

        public string[] GetValueSeparators()
        {
            if (ValueSeparators == null)
                return new string[0];
            return (string[]) ValueSeparators.Clone();
        }

        public void Invoke(OptionContext c)
        {
            OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        public override string ToString()
        {
            return Prototype;
        }

        protected abstract void OnParseComplete(OptionContext c);

        private OptionValueType ParsePrototype()
        {
            char type = '\0';
            List<string> seps = new List<string>();
            for (int i = 0; i < Names.Length; ++i)
            {
                string name = Names[i];
                if (name.Length == 0)
                    throw new ArgumentException("Empty option names are not supported.", "prototype");

                int end = name.IndexOfAny(NameTerminator);
                if (end == -1)
                    continue;
                Names[i] = name.Substring(0, end);
                if (type == '\0' || type == name[end])
                    type = name[end];
                else
                    throw new ArgumentException(
                        $"Conflicting option types: '{type}' vs. '{name[end]}'.",
                        "prototype");
                AddSeparators(name, end, seps);
            }

            if (type == '\0')
                return OptionValueType.None;

            if (MaxValueCount <= 1 && seps.Count != 0)
                throw new ArgumentException(
                    $"Cannot provide key/value separators for Options taking {MaxValueCount} value(s).",
                    "prototype");
            if (MaxValueCount > 1)
            {
                if (seps.Count == 0)
                    ValueSeparators = new[]
                    {
                        ":", "="
                    };
                else if (seps.Count == 1 && seps[0].Length == 0)
                    ValueSeparators = null;
                else
                    ValueSeparators = seps.ToArray();
            }

            return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
        }
    }
}
