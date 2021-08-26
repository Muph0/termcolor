
using TermColor.AnsiEscapeCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermColor {

    /// <summary>
    /// Text buffer capable of storing color data.
    /// Outputs text formatted with ANSI escape codes.
    /// </summary>
    /// <typeparam name="TColor">Internal representation of color.</typeparam>
    internal class AnsiTermBuffer<TColor> : ITerminalBuffer
        where TColor : struct, ANSIColor {

        public int Width { get; private set; }
        public int Height { get; private set; }

        char[,] _characters;
        TColor[,] _foreground;
        TColor[,] _background;

        /// <summary>
        /// Create a new buffer with the given dimensions.
        /// </summary>
        /// <param name="width">Number of columns of the buffer.</param>
        /// <param name="height">Number of rows of the buffer.</param>
        public AnsiTermBuffer(int width, int height) {
            Width = width;
            Height = height;

            _characters = new char[Width, Height];
            _foreground = new TColor[Width, Height];
            _background = new TColor[Width, Height];
            Clear();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                Win32TermBuffer.EnableVTProcessing();
            }
        }

        void ITerminalBuffer.Clear<TColorValue>(char ch, in TColorValue foreground, in TColorValue background)
            => Clear(ch, foreground, background);

        public void Clear()
            => Clear<Color4>(' ', default, default);


        /// <summary>
        /// Fill the buffer with given (<see cref="char"/>, <see cref="IColor"/>, <see cref="IColor"/>) tuple.
        /// </summary>
        /// <typeparam name="TColorValue">Type of the color.</typeparam>
        /// <param name="c">The character to clear with.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        public void Clear<TColorValue>(char ch, in TColorValue foreground, in TColorValue background)
            where TColorValue : IColor {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    _characters[x, y] = ch;
                    _foreground[x, y].From(foreground);
                    _background[x, y].From(background);
                }
            }
        }

        public void SetChar(int x, int y, char ch) {
            _characters[x, y] = ch;
        }

        public void SetForeground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor {
            _foreground[x, y].From(color);
        }

        public void SetBackground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor {
            _background[x, y].From(color);
        }

        public void Flush(TextWriter output) {
            Flush(output, 0, 0);
        }

        public void Flush(TextWriter output, int offsetX, int offsetY) {

            if (output == null) {
                throw new ArgumentNullException(nameof(output));
            }

            StringBuilder sb = new(Width * Height);

            string lastForeground = _foreground[0, 0].Foreground;
            string lastBackground = _background[0, 0].Background;

            sb.Append(lastForeground);
            sb.Append(lastBackground);

            for (int y = 0; y < Height; y++) {
                sb.Append(CSI.SetCursorPosition(offsetX, offsetY + y));

                for (int x = 0; x < Width; x++) {
                    if (_foreground[x, y].Foreground != lastForeground) {
                        sb.Append(lastForeground = _foreground[x, y].Foreground);
                    }
                    if (_background[x, y].Background != lastBackground) {
                        sb.Append(lastBackground = _background[x, y].Background);
                    }

                    sb.Append(_characters[x, y]);
                }
            }

            sb.Append(SGR.ResetColor());
            Console.Write(sb);
        }
    }
}
