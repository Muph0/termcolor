using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermColor;

namespace TermColor.Test {
    class TextOnlyBuffer : ITerminalBuffer {
        public int Width { get; }
        public int Height { get; }

        public TextOnlyBuffer(int width, int height) {
            Width = width;
            Height = height;

            data = new char[width * height];
            Clear();
        }

        char[] data;

        public void Clear<TColorValue>(char c, in TColorValue foreground, in TColorValue background) where TColorValue : IColor {
            Array.Fill(data, c);
        }

        public void Clear() {
            Clear<Color4>(' ', default, default);
        }

        public void Flush(TextWriter output) {
            for (int i = 0; i < data.Length; i++) {
                output.Write(data[i]);
                if (i % Width == Width - 1) {
                    output.Write('\n');
                }
            }
        }

        public void Flush(TextWriter output, int offsetLeft, int offsetTop) {
            Flush(output);
        }

        public void SetBackground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor { }

        public void SetChar(int x, int y, char ch) {
#if DEBUG
            if (x < 0 || x >= Width) {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y < 0 || y >= Height) {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
#endif

            data[x + y * Width] = ch;
        }

        public void SetForeground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor { }
    }
}
