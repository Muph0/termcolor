using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TermColor {

    internal class Win32TermBuffer : ITerminalBuffer {

        private int _width, _height;
        private CharInfo[] data;

        public int Length { get { return _width * _height; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        /// <summary>
        /// Creates new ConsoleBuffer with given size.
        /// </summary>
        public Win32TermBuffer(int width, int height) {
            if (width * height == 0)
                throw new Exception("Cannot create a buffer of zero size");
            if (width * height < 0)
                throw new Exception("Cannot create a buffer of negative size");

            this._width = width;
            this._height = height;
            init();
        }

        private void init() {
            data = new CharInfo[_width * _height];
            Clear();
        }

        public void Clear()
            => Clear<Color4>(' ', default, default);
        public void Clear<TColorValue>(char c, in TColorValue foreground, in TColorValue background) where TColorValue : IColor {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {

                    SetChar(x, y, c);
                    SetForeground(x, y, foreground);
                    SetBackground(x, y, background);
                }
            }
        }

        public void SetChar(int x, int y, char ch) {

            if (ch <= 255) {
                data[x + y * Width].Char.UnicodeChar = ch;
            } else {
                int index = ASCII.IndexOf(ch);
                if (index == -1) index = ASCII.IndexOf('?');
                data[x + y * Width].Char.AsciiChar = (byte)index;
            }
        }
        public void SetForeground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor {
            Color4 back = new();
            back.From(color);
            data[x + y * Width].Attributes.Color = (byte)((data[x + y * Width].Attributes.Color & 0b11110000) | (int)back.Color);
        }
        public void SetBackground<TColorValue>(int x, int y, in TColorValue color) where TColorValue : IColor {
            Color4 back = new();
            back.From(color);
            data[x + y * Width].Attributes.Color = (byte)((data[x + y * Width].Attributes.Color & 0b00001111) | ((int)back.Color << 4));
        }

        #region DllImport

        const string ASCII = "\0☺☻♥♦♣♠•◘○\n♂♀\r♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼ !\"#$%&'()*+,-./0123456789:;<=>?@" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~⌂" +
            "ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜ¢£¥₧ƒáíóúñÑªº¿⌐¬½¼¡«»░▒▓│┤╡╢╖╕╣║╗╝╜╛┐" +
            "└┴┬├─┼╞╟╚╔╩╦╠═╬╧╨╤╥╙╘╒╓╫╪┘┌█▄▌▐▀αßΓπΣσµτΦΘΩδ∞φε∩≡±≥≤⌠⌡÷≈°∙·√ⁿ²■ ";


        public const int STD_OUTPUT_HANDLE = -11;
        public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetConsoleMode(
            IntPtr hConsoleHandle,
            uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetConsoleMode(
            IntPtr hConsoleHandle,
            out uint lpMode);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Coord {
            public short X;
            public short Y;

            public Coord(short X, short Y) {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        internal struct CharUnion {
            [FieldOffset(0)]
            public char UnicodeChar;
            [FieldOffset(0)]
            public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct CharInfo {
            [FieldOffset(0)]
            public CharUnion Char;
            [FieldOffset(2)]
            public CharAttributes Attributes;
        }

        [StructLayout(LayoutKind.Explicit, Size = 2)]
        internal struct CharAttributes {
            [FieldOffset(0)]
            public byte Color;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SmallRect {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }
        #endregion

        internal static bool EnableVTProcessingThrows = true;
        internal static void EnableVTProcessing() {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (handle != new IntPtr(-1)) {
                if (GetConsoleMode(handle, out uint mode)) {
                    mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                    if (SetConsoleMode(handle, mode)) {
                        return;
                    }
                }
            } 

            if (EnableVTProcessingThrows) {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void Flush(TextWriter output) {
            Flush(output, 0, 0);
        }

        public void Flush(TextWriter output, int offsetX, int offsetY) {

            SafeFileHandle h = CreateFile("CONOUT$", 0x40000000, 2,
                IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);


            SmallRect rect = new SmallRect() {
                Left = (short)offsetX,
                Top = (short)offsetY,
                Right = (short)(_width + offsetX),
                Bottom = (short)(_height + offsetY)
            };

            bool b = WriteConsoleOutput(h, data, new Coord((short)(_width), (short)_height),
                new Coord((short)0, (short)0), ref rect);
        }
    }
}

