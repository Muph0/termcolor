using System.IO;

namespace TermColor {

    /// <summary>
    /// Two dimensional buffer of triplets (<see cref="char"/>, <see cref="IColor"/>, <see cref="IColor"/>).
    /// Color might be internally represented as a specific implementation of <see cref="IColor"/>.
    /// Can be flushed out to other buffers or to a display device.
    /// </summary>
    internal interface ITerminalBuffer {

        /// <summary>
        /// Nuber of columns of the buffer.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Number of rows of the buffer.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Write a character, foreground and background color to one point in the buffer.
        /// Rows and columns are numbered from the top left starting at zero.
        /// </summary>
        /// <typeparam name="TColorValue">Type of the color.</typeparam>
        /// <param name="x">Column number.</param>
        /// <param name="y">Row number.</param>
        /// <param name="ch">The character to write.</param>
        /// <param name="foreground">Foreground color of the character.</param>
        /// <param name="background">Background color of the character.</param>
        void SetPoint<TColorValue>(int x, int y, char ch, in TColorValue foreground, in TColorValue background)
            where TColorValue : IColor {

            SetChar(x, y, ch);
            SetForeground(x, y, foreground);
            SetBackground(x, y, background);
        }

        /// <summary>
        /// Write a character to one point in the buffer.
        /// Rows and columns are numbered from the top left starting at zero.
        /// </summary>
        /// <param name="x">Column number.</param>
        /// <param name="y">Row number.</param>
        /// <param name="ch">The character to write.</param>
        void SetChar(int x, int y, char ch);

        /// <summary>
        /// Write foreground color to one point in the buffer.
        /// Rows and columns are numbered from the top left starting at zero.
        /// </summary>
        /// <param name="x">Column number.</param>
        /// <param name="y">Row number.</param>
        /// <param name="foreground">Foreground color of the character.</param>
        void SetForeground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor;

        /// <summary>
        /// Write background color to one point in the buffer.
        /// Rows and columns are numbered from the top left starting at zero.
        /// </summary>
        /// <param name="x">Column number.</param>
        /// <param name="y">Row number.</param>
        /// <param name="background">Background color of the character.</param>
        void SetBackground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor;

        /// <summary>
        /// Forward the contents of the buffer to an output device.
        /// </summary>
        /// <param name="output">The output device.</param>
        void Flush(TextWriter output);

        /// <summary>
        /// Forward the contents of the buffer to an output device at a specific offset.
        /// </summary>
        /// <param name="output">The output device.</param>
        void Flush(TextWriter output, int offsetLeft, int offsetTop);

        /// <summary>
        /// Fill the buffer with given (<see cref="char"/>, <see cref="IColor"/>, <see cref="IColor"/>) tuple.
        /// </summary>
        /// <typeparam name="TColorValue">Type of the color.</typeparam>
        /// <param name="c">The character to clear with.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        void Clear<TColorValue>(char c, in TColorValue foreground, in TColorValue background) where TColorValue : IColor;

        /// <summary>
        /// Fill the buffer with empty characters and default colors.
        /// </summary>
        void Clear();

    }
}
