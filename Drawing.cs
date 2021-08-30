using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TermColor.Drawing {

    public static class ITerminalBufferEx {

        private static void ensureInOrder(ref int a, ref int b) {
            if (a > b) {
                int temp = a;
                a = b;
                b = temp;
            }
        }

        /// <summary>
        /// Fill a segment of a row with characters of the same color.
        /// </summary>
        /// <typeparam name="TColor">Type of the used color.</typeparam>
        /// <param name="buffer"></param>
        /// <param name="startX">Starting column number.</param>
        /// <param name="endX">End column number.</param>
        /// <param name="y">Row number.</param>
        /// <param name="ch">The character to use.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        public static void DrawHorizontalLine<TColor>(this ITerminalBuffer buffer, int startX, int endX, int y, char ch, in TColor foreground, in TColor background)
            where TColor : IColor {

            ensureInOrder(ref startX, ref endX);
#if DEBUG
            if (startX < 0 || endX >= buffer.Width) {
                throw new ArgumentOutOfRangeException(
                    $"{nameof(startX)}, {nameof(endX)}", $"Given range ({startX}, {endX}) is outside of the Width{{{buffer.Width}}} of buffer.");
            }
#endif
            for (int x = startX; x <= endX; x++) {
                buffer.SetPoint(x, y, ch, foreground, background);
            }
        }

        /// <summary>
        /// Fill a segment of a column with characters of the same color.
        /// </summary>
        /// <typeparam name="TColor">Type of the used color.</typeparam>
        /// <param name="buffer"></param>
        /// <param name="startY">First row number.</param>
        /// <param name="endY">Last row number.</param>
        /// <param name="y">Row number.</param>
        /// <param name="ch">The character to use.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        public static void DrawVerticalLine<TColor>(this ITerminalBuffer buffer, int x, int startY, int endY, char ch, in TColor foreground, in TColor background)
            where TColor : IColor {

            ensureInOrder(ref startY, ref endY);
#if DEBUG
            if (startY < 0 || endY >= buffer.Height) {
                throw new ArgumentOutOfRangeException(
                    $"{nameof(startY)}, {nameof(endY)}", $"Given range ({startY}, {endY}) is outside of the Height{{{buffer.Height}}} of buffer.");
            }
#endif
            for (int y = startY; y <= endY; y++) {
                buffer.SetPoint(x, y, ch, foreground, background);
            }
        }

        /// <summary>
        /// Draw an outline of a rectangle with characters of given color.
        /// </summary>
        /// <typeparam name="TColor">Type of the used color.</typeparam>
        /// <param name="buffer"></param>
        /// <param name="startY">First row number.</param>
        /// <param name="endY">Last row number.</param>
        /// <param name="startX">First column number.</param>
        /// <param name="endX">Last column number.</param>
        /// <param name="ch">The character to use.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        public static void DrawRectangle<TColor>(this ITerminalBuffer buffer,
            int startX, int startY, int endX, int endY, char ch, in TColor foreground, in TColor background)
            where TColor : IColor {

            DrawHorizontalLine(buffer, startX+1, endX-1, startY, ch, foreground, background);
            DrawHorizontalLine(buffer, startX+1, endX-1, endY, ch, foreground, background);

            DrawVerticalLine(buffer, startX, startY, endY, ch, foreground, background);
            DrawVerticalLine(buffer, endX, startY, endY, ch, foreground, background);
        }

        /// <summary>
        /// Fill a rectangle with a characters of given color.
        /// </summary>
        /// <typeparam name="TColor">Type of the used color.</typeparam>
        /// <param name="buffer"></param>
        /// <param name="startY">First row number.</param>
        /// <param name="endY">Last row number.</param>
        /// <param name="startX">First column number.</param>
        /// <param name="endX">Last column number.</param>
        /// <param name="ch">The character to use.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        public static void FillRectangle<TColor>(this ITerminalBuffer buffer,
            int startX, int startY, int endX, int endY, char ch, in TColor foreground, in TColor background)
            where TColor : IColor {

            ensureInOrder(ref startX, ref endX);
            ensureInOrder(ref startY, ref endY);
#if DEBUG
            if (startX < 0 || endX >= buffer.Width) {
                throw new ArgumentOutOfRangeException(
                    $"{nameof(startX)}, {nameof(endX)}", $"Given range ({startX}, {endX}) is outside of the Width{{{buffer.Width}}} of buffer.");
            }
            if (startY < 0 || endY >= buffer.Height) {
                throw new ArgumentOutOfRangeException(
                    $"{nameof(startY)}, {nameof(endY)}", $"Given range ({startY}, {endY}) is outside of the Height{{{buffer.Height}}} of buffer.");
            }
#endif

            for (int y = startY; y <= endY; y++) {
                for (int x = startX; x <= endX; x++) {
                    buffer.SetPoint(x, y, ch, foreground, background);
                }
            }

        }

        /// <summary>
        /// Draw a line from (startX, startY) to (endX, endY).
        /// </summary>
        /// <typeparam name="TColor">Type of the used color.</typeparam>
        /// <param name="buffer"></param>
        /// <param name="startY">First row number.</param>
        /// <param name="endY">Last row number.</param>
        /// <param name="startX">First column number.</param>
        /// <param name="endX">Last column number.</param>
        /// <param name="ch">The character to use.</param>
        /// <param name="foreground">Foreground color of the line.</param>
        /// <param name="background">Background color of the line.</param>
        public static void DrawLine<TColor>(this ITerminalBuffer buffer,
            int startX, int startY, int endX, int endY, char ch, TColor foreground, in TColor background)
            where TColor : IColor {
#if DEBUG
            if (startX < 0 || startX >= buffer.Width || startY < 0 || startY >= buffer.Height) {
                throw new ArgumentOutOfRangeException(
                    $"{nameof(startX)}, {nameof(startY)}", $"Start point ({startX}, {startY}) is outside of the buffer.");
            }
            if (endX < 0 || endX >= buffer.Width || endY < 0 || endY >= buffer.Height) {
                throw new ArgumentOutOfRangeException(
                    $"{nameof(endX)}, {nameof(endY)}", $"End point ({endX}, {endY}) is outside of the buffer.");
            }
#endif
            int dx = endX - startX;
            int dy = endY - startY;

            int steps = Math.Abs(dx) > Math.Abs(dy) ? Math.Abs(dx) : Math.Abs(dy);

            float stepX = dx / (float)steps;
            float stepY = dy / (float)steps;

            float X = startX;
            float Y = startY;
            for (int i = 0; i <= steps; i++) {
                buffer.SetPoint((int)X, (int)Y, ch, foreground, background);
                X += stepX;
                Y += stepY;
            }
        }
    }
}

