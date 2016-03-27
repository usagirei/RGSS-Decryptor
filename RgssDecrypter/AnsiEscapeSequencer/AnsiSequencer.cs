// --------------------------------------------------
// RgssDecrypter - AnsiSequencer.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RgssDecrypter.AnsiEscapeSequencer.Modules;

namespace RgssDecrypter.AnsiEscapeSequencer
{
    public class AnsiSequencer : TextWriter
    {
        public delegate void Action<in T1, in T2>(T1 objA, T2 objB);

        public static AnsiSequencer Instance { get; } = new AnsiSequencer();

        public enum States
        {
            Text,
            Escape,
            Code,
        }

        private const char CODE_ESCAPE = '\x1B';

        private const char CODE_SEPARATOR = ';';
        private const char CODE_START = '[';

        private static readonly Dictionary<char, Action<AnsiSequencer, byte[]>> Actions;

        public static Dictionary<Type, SequencerModule> Modules;
        private TextWriter _consoleOut;
        private readonly StringBuilder _escapeBuffer = new StringBuilder();

        private States _state = States.Text;

        public override Encoding Encoding => _consoleOut.Encoding;

        public States State => _state;

        static AnsiSequencer()
        {
            Actions = new Dictionary<char, Action<AnsiSequencer, byte[]>>();
            Modules = new Dictionary<Type, SequencerModule>();
        }

        private AnsiSequencer() {}

        public static void Enable()
        {
            Instance._consoleOut = Console.Out;
            Console.SetOut(Instance);
        }

        public static bool RegisterAction(char code, Action<AnsiSequencer, byte[]> action)
        {
            if (Actions.ContainsKey(code))
                return false;
            Actions.Add(code, action);
            return true;
        }

        public static bool UnregisterAction(char code)
        {
            if (!Actions.ContainsKey(code))
                return false;
            Actions.Remove(code);
            return true;
        }

        public static bool DisableModule<T>() where T : SequencerModule
        {
            if (!Modules.ContainsKey(typeof(T)))
                return false;
            var mod = Modules[typeof(T)];
            mod.DeInit();
            mod.Registered = false;
            Modules.Remove(typeof(T));
            return true;
        }

        public static bool EnableModule<T>() where T : SequencerModule
        {
            if (Modules.ContainsKey(typeof(T)))
                return false;
            var mod = Activator.CreateInstance<T>();
            mod.Init();
            Modules.Add(typeof(T), mod);
            mod.Registered = true;
            return true;
        }

        public override void Write(char value)
        {
            switch (_state)
            {
                case States.Text:
                {
                    if (value == CODE_ESCAPE)
                    {
                        _state = States.Escape;
                        _escapeBuffer.Length = 0;
                    }
                    else
                    {
                        _consoleOut.Write(value);
                    }
                    break;
                }
                case States.Escape:
                {
                    if (value != CODE_START)
                    {
                        _consoleOut.Write(CODE_ESCAPE);
                        _consoleOut.Write(value);
                        _state = States.Text;
                    }
                    else
                    {
                        _state = States.Code;
                    }
                    break;
                }
                case States.Code:
                {
                    if ((value >= '0' && value <= '9') || value == CODE_SEPARATOR)
                    {
                        _escapeBuffer.Append(value);
                    }
                    else
                    {
                        _state = States.Text;
                        var strBytes = _escapeBuffer.ToString().Split(CODE_SEPARATOR);
                        var bytes = new byte[strBytes.Length];

                        if (!Actions.ContainsKey(value))
                            break;

                        var abort = false;
                        for (var i = 0; i < strBytes.Length; i++)
                        {
                            if (byte.TryParse(strBytes[i], out bytes[i]))
                                continue;
                            abort = true;
                            break;
                        }
                        if (abort)
                            break;

                        Actions[value].Invoke(this, bytes);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disable();
                foreach (var module in Modules.Values)
                    module.DeInit();
            }
        }

        public static void Disable()
        {
            Console.SetOut(Instance._consoleOut);
        }
    }
}
