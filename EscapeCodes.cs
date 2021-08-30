using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermColor.AnsiEscapeCodes {

    /// <summary>
    /// Helper class for generating "Select Graphic Rendition" ANSI codes.
    /// For detailed explanation, see the Wikipedia article https://en.wikipedia.org/wiki/ANSI_escape_code
    /// </summary>
    public static class SGR {

        static string[] foreground_ = new string[] {
            "\x1b[30m",
            "\x1b[34m",
            "\x1b[32m",
            "\x1b[36m",
            "\x1b[31m",
            "\x1b[35m",
            "\x1b[33m",
            "\x1b[37m",
            "\x1b[90m",
            "\x1b[94m",
            "\x1b[92m",
            "\x1b[96m",
            "\x1b[91m",
            "\x1b[95m",
            "\x1b[93m",
            "\x1b[97m",
        };
        static string[] background_ = new string[] {
            "\x1b[40m",
            "\x1b[44m",
            "\x1b[42m",
            "\x1b[46m",
            "\x1b[41m",
            "\x1b[45m",
            "\x1b[43m",
            "\x1b[47m",
            "\x1b[100m",
            "\x1b[104m",
            "\x1b[102m",
            "\x1b[106m",
            "\x1b[101m",
            "\x1b[105m",
            "\x1b[103m",
            "\x1b[107m",
        };

        /// <summary>
        /// Creates a sequence which sets the foreground color to specified color in 16 color palette.
        /// </summary>
        public static string Foreground(Color4 clr) {
            return foreground_[(int)clr.Color];
        }
        /// <summary>
        /// Creates a sequence which sets the foreground color to specified color in 256 color palette.
        /// </summary>
        public static string Foreground(Color8 clr) {
            return string.Concat("\x1b[38;5;", clr.Color, "m");
        }
        /// <summary>
        /// Creates a sequence which sets the foreground color to specified color in RGB color palette.
        /// </summary>
        public static string Foreground(Color24 clr) {
            return string.Concat("\x1b[38;2;", clr.Red, ";", clr.Green, ";", clr.Blue, "m");
        }

        /// <summary>
        /// Writes a sequence which sets the foreground color to specified color in 16 color palette to the specified writer.
        /// </summary>
        public static void Foreground(this TextWriter tw, Color4 clr) {
            tw.Write(foreground_[(int)clr.Color]);
        }
        /// <summary>
        /// Writes a sequence which sets the foreground color to specified color in 256 color palette to the specified writer.
        /// </summary>
        public static void Foreground(this TextWriter tw, Color8 clr) {
            tw.Write($"\x1b[38;5;{clr.Color}m");
        }
        /// <summary>
        /// Writes a sequence which sets the foreground color to specified color in RGB color palette to the specified writer.
        /// </summary>
        public static void Foreground(this TextWriter tw, Color24 clr) {
            tw.Write($"\x1b[38;2;{clr.Red};{clr.Green};{clr.Blue}m");
        }



        /// <summary>
        /// Creates a sequence which sets the background color to specified color in 16 color palette.
        /// </summary>
        public static string Background(Color4 clr) {
            return background_[(int)clr.Color];
        }
        /// <summary>
        /// Creates a sequence which sets the background color to specified color in 256 color palette.
        /// </summary>
        public static string Background(Color8 clr) {
            return string.Concat("\x1b[48;5;", clr.Color, "m");
        }
        /// <summary>
        /// Creates a sequence which sets the background color to specified color in RGB color palette.
        /// </summary>
        public static string Background(Color24 clr) {
            return string.Concat("\x1b[48;2;", clr.Red, ";", clr.Green, ";", clr.Blue, "m");
        }
        /// <summary>
        /// Writes a sequence which sets the background color to specified color in 16 color palette to the specified writer.
        /// </summary>
        public static void Background(this TextWriter tw, Color4 clr) => tw.Write(Background(clr));
        /// <summary>
        /// Writes a sequence which sets the background color to specified color in 256 color palette to the specified writer.
        /// </summary>
        public static void Background(this TextWriter tw, Color8 clr) => tw.Write(Background(clr));
        /// <summary>
        /// Writes a sequence which sets the background color to specified color in RGB color palette to the specified writer.
        /// </summary>
        public static void Background(this TextWriter tw, Color24 clr) => tw.Write(Background(clr));

        /// <summary>
        /// Creates a sequence which resets the text formatting.
        /// </summary>
        public static string ResetColor() => "\x1b[0m";
        /// <summary>
        /// Writes a sequence which resets the text formatting to the specified writer.
        /// </summary>
        public static void ResetColor(this TextWriter tw) => tw.Write(SGR.ResetColor());
    }

    /// <summary>
    /// Helper class for generating "Control Sequence Introducer" ANSI sequences
    /// For detailed explanation, see the Wikipedia article https://en.wikipedia.org/wiki/ANSI_escape_code
    /// </summary>
    public static class CSI {

        /// <summary>
        /// Generate cursor position escape code. Top left corner is (0,0).
        /// </summary>
        /// <param name="x">Horizontal position</param>
        /// <param name="y">Vertical position</param>
        /// <returns>The generated code</returns>
        public static string SetCursorPosition(int x, int y) {
            return string.Concat("\x1b[", y + 1, ";", x + 1, "H");
        }
    }
}
