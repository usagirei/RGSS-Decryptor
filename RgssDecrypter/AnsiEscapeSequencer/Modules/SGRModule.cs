// --------------------------------------------------
// RgssDecrypter - SGRModule.cs
// --------------------------------------------------

using System;

namespace RgssDecrypter.AnsiEscapeSequencer.Modules
{
    class SGRModule : SequencerModule
    {
        private static readonly ConsoleColor[] ColorMap =
        {
            ConsoleColor.Black,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkCyan,
            ConsoleColor.Gray,
            //
            ConsoleColor.DarkGray,
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.Cyan,
            ConsoleColor.White
        };

        private readonly ConsoleColor _defaultBack;

        private readonly ConsoleColor _defaultFore;

        public bool IsBold { get; private set; }

        public bool IsInverted { get; private set; }

        public SGRModule()
        {
            _defaultFore = Console.ForegroundColor;
            _defaultBack = Console.BackgroundColor;
        }

        public override void Init()
        {
            if (Registered)
                return;
            AnsiSequencer.RegisterAction('m', HandleSelectGraphicsRendering);
        }

        public override void DeInit()
        {
            if (!Registered)
                return;
            AnsiSequencer.UnregisterAction('m');
        }

        private void HandleSelectGraphicsRendering(AnsiSequencer s, params byte[] b)
        {
            foreach (var param in b)
            {
                // Foreground
                if (param >= 30 && param <= 37)
                {
                    SetForeColor((byte) (param - 30), false);
                }
                else if (param >= 90 && param <= 97)
                {
                    SetForeColor((byte) (param - 90), true);
                }
                // Foreground
                else if (param >= 40 && param <= 47)
                {
                    SetBackColor((byte) (param - 40), false);
                }
                else if (param >= 100 && param <= 107)
                {
                    SetBackColor((byte) (param - 100), true);
                }
                else
                {
                    switch (param)
                    {
                        // Reset
                        case 0:
                            SetBold(false);
                            SetInverse(false);
                            ResetForeColor();
                            ResetBackColor();
                            break;
                        case 2:
                        case 22:
                            SetBold(false);
                            break;
                        case 39:
                            ResetForeColor();
                            break;
                        case 49:
                            ResetBackColor();
                            break;
                        // Intensity
                        case 1:
                            SetBold(true);
                            break;
                        //  Inverse Video
                        case 7:
                            SetInverse(true);
                            break;
                        case 27:
                            SetInverse(false);
                            break;
                    }
                }
            }
        }

        private void ResetBackColor()
        {
            if (IsInverted)
                Console.BackgroundColor = _defaultBack;
            else
                Console.ForegroundColor = _defaultBack;
        }

        private void ResetForeColor()
        {
            if (IsInverted)
                Console.BackgroundColor = _defaultFore;
            else
                Console.ForegroundColor = _defaultFore;
        }

        private void SetBackColor(byte val, bool intense)
        {
            if (IsInverted)
                Console.ForegroundColor = ColorMap[val + ((IsBold | intense) ? 8 : 0)];
            else
                Console.BackgroundColor = ColorMap[val + (intense ? 8 : 0)];
        }

        private void SetForeColor(byte val, bool intense)
        {
            if (IsInverted)
                Console.BackgroundColor = ColorMap[val + (intense ? 8 : 0)];
            else
                Console.ForegroundColor = ColorMap[val + ((IsBold | intense) ? 8 : 0)];
        }

        private void SetBold(bool value)
        {
            if (IsBold == value)
                return;
            IsBold = value;
            var index = Array.IndexOf(ColorMap, Console.ForegroundColor);
            var oldFg = (byte) (index % 8);
            SetForeColor(oldFg, index >= 8);
        }

        private void SetInverse(bool value)
        {
            if (IsInverted == value)
                return;
            var c = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = c;
            IsInverted = !IsInverted;
        }
    }
}
