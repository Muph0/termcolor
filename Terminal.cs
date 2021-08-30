using System;
using System.IO;
using System.Numerics;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TermColorDemo")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TermColorTest")]

namespace TermColor {

    public enum ColorMode {
        /// <summary>
        /// 16 color mode.
        /// </summary>
        Plain4bit,

        /// <summary>
        /// 16 color mode with dithering. Originally designed for DOS palette with the MS raster fonts.
        /// However, the <see cref="Terminal.DitherPreset" /> can be modified to work with different font and dithering characters.
        /// </summary>
        Dither4bit,

        /// <summary>
        /// 256 color mode.
        /// </summary>
        Plain8bit,

        /// <summary>
        /// Truecolor mode.
        /// </summary>
        Plain24bit,
    }

    /// <summary>
    /// Represents a colored text buffer. Not thread-safe.
    /// </summary>
    /// Internally uses <see cref="AnsiTermBuffer" /> and <see cref="Win32TermBuffer" />
    /// based on the current <see cref="Terminal.ColorMode" />.
    /// On Windows, <see cref="Win32TermBuffer" /> is used in 16 color mode, otherwise <see cref="AnsiTermBuffer" /> is used.
    public class Terminal : TextWriter, ITerminalBuffer {

        public override Encoding Encoding => Encoding.Default;

        /// <summary>
        /// Gets the width of the buffer.
        /// </summary>
        public int Width => Buffer.Width;

        /// <summary>
        /// Gets the height of the buffer.
        /// </summary>
        public int Height => Buffer.Height;

        /// <summary>
        /// Gets or sets the column position of the cursor within the buffer.
        /// Columns are numbered from left to right starting at 0.
        /// </summary>
        public int CursorLeft {
            get => _cursorX;
            set => _cursorX = value >= 0 && value < Width ? value :
                throw new ArgumentOutOfRangeException($"Cursor left position must be at least 0 and smaller than {nameof(Width)}, {value} given.");
        }
        private int _cursorX;

        public DitherMapping DitherPreset { get; set; } = DitherMapping.Default;

        /// <summary>
        /// Gets or sets the row position of the cursor within the buffer.
        /// Rows are numbered from top to bottom starting at 0.
        /// </summary>
        public int CursorTop {
            get => _cursorY;
            set => _cursorY = value >= 0 && value < Height ? value : throw new ArgumentOutOfRangeException(
                    $"Cursor top position must be at least 0 and smaller than {nameof(Height)}, {value} given.", (Exception)null);
        }
        private int _cursorY;

        /// <summary>
        /// Gets or sets the foreground color of the buffer.
        /// </summary>
        public IColor ForegroundColor {
            get => _foregroundColor;
            set => _foregroundColor = value != null ? value : throw new ArgumentNullException();

        }
        private IColor _foregroundColor;

        /// <summary>
        /// Gets or sets the background color of the buffer.
        /// </summary>
        public IColor BackgroundColor {
            get => _backgroundColor;
            set => _backgroundColor = value != null ? value : throw new ArgumentNullException();
        }
        private IColor _backgroundColor;

        internal ITerminalBuffer Buffer { get; private set; }

        /// <summary>
        /// Gets or sets the output target. Calling <see cref="Flush()"/> forwards contents of this <see cref="Terminal"/> to <see cref="Out"/>.
        /// </summary>
        public TextWriter Out { get; set; } = Console.Out;

        /// <summary>
        /// Create a new terminal buffer in 24-bit color mode with dimensions taken from (<see cref="Console.WindowWidth" />, <see cref="Console.WindowHeight" />).
        /// </summary>
        public Terminal() : this(Console.WindowWidth, Console.WindowHeight, ColorMode.Plain24bit) { }

        /// <summary>
        /// Create a new terminal buffer with given dimensions in 24-bit color mode.
        /// </summary>
        /// <param name="width">The number of columns in the new buffer.</param>
        /// <param name="height">The number of rows in the new buffer.</param>
        public Terminal(int width, int height) : this(width, height, ColorMode.Plain24bit) { }

        /// <summary>
        /// Create a new terminal buffer with given dimensions and color mode.
        /// </summary>
        /// <param name="width">The number of columns in the new buffer.</param>
        /// <param name="height">The number of rows in the new buffer.</param>
        /// <param name="colorMode">The color mode of the new buffer.</param>
        public Terminal(int width, int height, ColorMode colorMode) {
#if DEBUG
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
#endif
            ResetColor();
            Buffer = createBuffer(width, height, colorMode);
            _colorMode = colorMode;
        }

        /// <summary>
        /// Sets the position of the cursor.
        /// </summary>
        /// <param name="left">The column position of the cursor. Columns are numbered from left to right starting at 0.</param>
        /// <param name="top">The row position of the cursor. Rows are numbered from top to bottom starting at 0.</param>
        public void SetCursorPosition(int left, int top) {
            CursorLeft = left;
            CursorTop = top;
        }

        /// <summary>
        /// Sets the foreground and background colors to their defaults.
        /// </summary>
        public void ResetColor() {
            ForegroundColor = (Color4)ConsoleColor.Gray;
            BackgroundColor = (Color4)ConsoleColor.Black;
        }

        /// <summary>
        /// Gets or sets the color mode of the buffer.
        /// </summary>
        public ColorMode ColorMode {
            get => _colorMode;
            set {
                _colorMode = value;
                this.Buffer = createBuffer(Width, Height, _colorMode);
            }
        }
        private ColorMode _colorMode;
        private bool _dither => _colorMode == ColorMode.Dither4bit;

        /// <summary>
        /// Writes the specified Unicode character value to the buffer at the current cursor position and advances the cursor.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public override void Write(char value) {

            if (value < ' ') {
                switch (value) {

                    case '\r':
                        _cursorX = 0;
                        break;

                    case '\n':
                        _cursorY++;
                        goto case '\r';

                    case '\t':
                        Write(' ');
                        for (int x = _cursorX; x < Width && x % 4 != 0; x++) {
                            Write(' ');
                        }
                        break;

                    default:
                        Write('?');
                        break;
                }
            } else {

                if (_dither && (value == ' ' || value == '█')) {

                    if (!DitherPreset.IsComputed) {
                        DitherPreset.Precompute(8);
                    }

                    IColor target = value == ' ' ? BackgroundColor : ForegroundColor;

                    var (fg, bg, mask) = DitherPreset.Map(target.ToRGB());

                    Buffer.SetPoint<Color4>(_cursorX, _cursorY, mask, fg, bg);


                } else {
                    Buffer.SetPoint(_cursorX, _cursorY, value, ForegroundColor, BackgroundColor);
                }

                _cursorX++;
            }

            if (_cursorX == Width) {
                _cursorX = 0;
                _cursorY++;
            }

            if (_cursorY == Height) {
                _cursorY = 0;
            }
        }

        /// <summary>
        /// Forward the internal buffers out to <see cref="Terminal.Out" />.
        /// </summary>
        public override void Flush() {
            if (Out != null) {
                Buffer.Flush(Out);
            }
        }

        /// <summary>
        /// Forward the internal buffers out to <see cref="Terminal.Out" />.
        /// </summary>
        /// <param name="x">Column position of the top left corner of the displayed output.</param>
        /// <param name="y">Row position of the top left corner of the displayed output.</param>
        public void Flush(int x, int y) {
            if (Out != null) {
                Buffer.Flush(Out, x, y);
            }
        }

        /// <summary>
        /// Selects, which implementation of <see cref="ITerminalBuffer" /> suits best for
        /// current platform and <see cref="Terminal.ColorMode">, and returns new instance of that.
        /// </summary>
        private static ITerminalBuffer createBuffer(int width, int height, ColorMode mode) {
            switch (mode) {
                case ColorMode.Dither4bit:
                case ColorMode.Plain4bit:
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                        return new Win32TermBuffer(width, height);
                    }
                    return new AnsiTermBuffer<Color4>(width, height);

                case ColorMode.Plain8bit:
                    return new AnsiTermBuffer<Color8>(width, height);
                case ColorMode.Plain24bit:
                    return new AnsiTermBuffer<Color24>(width, height);
            }

            return null;
        }

        public void SetChar(int x, int y, char ch)
            => this.Buffer.SetChar(x, y, ch);

        public void SetForeground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor
            => Buffer.SetForeground(x, y, color);

        public void SetBackground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor
            => Buffer.SetBackground(x, y, color);

        /// <summary>
        /// Clears the buffer with <see cref="BackgroundColor"/> and sets the cursor to the top left corner.
        /// </summary>
        public void Clear() {
            Buffer.Clear();
            SetCursorPosition(0, 0);
        }

        public void Clear<TColorValue>(char c, in TColorValue foreground, in TColorValue background) where TColorValue : IColor
            => Buffer.Clear(c, foreground, background);

        public void Flush(TextWriter output) {
            if (output != null) {
                Buffer.Flush(output);
            }
        }

        public void Flush(TextWriter output, int offsetLeft, int offsetTop) {
            if (output != null) {
                Buffer.Flush(output, offsetLeft, offsetTop);
            }
        }
    }
}
