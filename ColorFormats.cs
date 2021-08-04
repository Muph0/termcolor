using TermColor.AnsiEscapeCodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TermColor {

    /// <summary>
    /// Represents a color.
    /// </summary>
    public interface IColor {

        /// <summary>
        /// Convert this color to its HSV equivalent.
        /// </summary>
        /// <returns></returns>
        ColorHSV ToHSV();

        /// <summary>
        /// Convert this color to its RGB equivalent
        /// </summary>
        /// <returns></returns>
        ColorRGB ToRGB();

        /// <summary>
        /// Change this color to approximate <paramref name="color"/>.
        /// </summary>
        /// <typeparam name="TColor">Type of the color.</typeparam>
        /// <param name="color">The color to approximate.</param>
        void From<TColor>(in TColor color) where TColor : IColor;
    }

    /// <summary>
    /// Represents a color mappable to an ANSI escape code.
    /// </summary>
    interface ANSIColor : IColor {

        /// <summary>
        /// Get the foreground ANSI escape code for this color.
        /// </summary>
        string Foreground { get; }

        /// <summary>
        /// Get the background ANSI escape code for this color.
        /// </summary>
        string Background { get; }
    }

    /// <summary>
    /// Color represented as (Hue,Saturation,Value) vector.
    /// </summary>
    public struct ColorHSV : IColor {

        /// <summary>
        /// Gets/sets the hue of the color. Ranges from 0 to 360.
        /// </summary>
        public float Hue { get; set; }

        /// <summary>
        /// Gets/sets the saturation of the color. Ranges from 0 to 1.
        /// </summary>
        public float Saturation { get; set; }

        /// <summary>
        /// Gets/sets the value of the color. Ranges from 0 to 1.
        /// </summary>
        public float Value { get; set; }

        public ColorHSV(float hue, float saturation, float value) {
            Hue = (hue % 360 + 360) % 360;
            Saturation = saturation;
            Value = value;
        }

        public ColorHSV ToHSV() {
            return this;
        }

        public ColorRGB ToRGB() {
            float c = Saturation * Value;
            float x = c * (1 - MathF.Abs((Hue / 60) % 2 - 1));
            float m = Value - c;

            float r, g, b;

            switch ((int)Hue / 60) {
                case 0:
                    (r, g, b) = (c, x, 0);
                    break;
                case 1:
                    (r, g, b) = (x, c, 0);
                    break;
                case 2:
                    (r, g, b) = (0, c, x);
                    break;
                case 3:
                    (r, g, b) = (0, x, c);
                    break;
                case 4:
                    (r, g, b) = (x, 0, c);
                    break;
                case 5:
                default:
                    (r, g, b) = (c, 0, x);
                    break;
            }

            return new ColorRGB(r + m, g + m, b + m);
        }

        public void From<TColor>(in TColor color) where TColor : IColor {
            var rgb = color.ToHSV();
            Hue = rgb.Hue;
            Saturation = rgb.Saturation;
            Value = rgb.Value;
        }

        public override string ToString() {
            return $"HSV: {Hue} {Saturation:F2} {Value:F2}";
        }
    }

    /// <summary>
    /// Color represented as a vector of (Red,Green,Blue) components in range [0.0, 1.0].
    /// </summary>
    public struct ColorRGB : IColor {
        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }

        public ColorRGB(float red, float green, float blue) {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public ColorHSV ToHSV() {
            float Cmax, Cmin;
            float hue, delta = 0, saturation, value;
            int offset = 0;

            float red = Red.Clamp(0, 1);
            float green = Green.Clamp(0, 1);
            float blue = Blue.Clamp(0, 1);

            if (red > green) {
                if (red > blue) {
                    Cmax = red;
                    Cmin = MathF.Min(blue, green);
                    hue = green - blue;
                } else {
                    goto maxB;
                }
            } else {
                if (green > blue) {
                    Cmax = green;
                    Cmin = MathF.Min(red, blue);
                    offset = 2;
                    hue = blue - red;
                } else {
                    goto maxB;
                }
            }

            goto post;
        maxB:
            Cmax = blue;
            Cmin = MathF.Min(red, green);
            offset = 4;
            hue = red - green;

        post:
            delta = Cmax - Cmin;

            if (delta > 0) {
                hue = 60 * ((hue / delta + offset) % 6);
            } else {
                hue = 0;
            }

            saturation = Cmax > 0 ? delta / Cmax : 0;
            value = Cmax;

            return new(hue, saturation, value);
        }
        public ColorRGB ToRGB() {
            return this;
        }

        public float Dot(in ColorRGB other) {
            return Red * other.Red + Green * other.Green + Blue * other.Blue;
        }

        /// <summary>
        /// Computes square root of Red, Green and Blue.
        /// </summary>
        /// <returns>The square root color.</returns>
        public ColorRGB Sqrt() {
            return new ColorRGB(MathF.Sqrt(Red), MathF.Sqrt(Green), MathF.Sqrt(Blue));
        }

        public void From<TColor>(in TColor color) where TColor : IColor {
            var rgb = color.ToRGB();
            Red = rgb.Red;
            Green = rgb.Green;
            Blue = rgb.Blue;
        }

        public override string ToString() {
            return $"RGB: {Red:F2} {Green:F2} {Blue:F2}";
        }

        public static ColorRGB operator +(in ColorRGB a, in ColorRGB b) {
            return new ColorRGB(a.Red + b.Red, a.Green + b.Green, a.Blue + b.Blue);
        }
        public static ColorRGB operator -(in ColorRGB a, in ColorRGB b) {
            return new ColorRGB(a.Red - b.Red, a.Green - b.Green, a.Blue - b.Blue);
        }
        public static ColorRGB operator *(in ColorRGB a, in ColorRGB b) {
            return new(a.Red * b.Red, a.Green * b.Green, a.Blue * b.Blue);
        }
        public static ColorRGB operator *(in ColorRGB c, float scale) {
            return new ColorRGB(c.Red * scale, c.Green * scale, c.Blue * scale);
        }
        public static ColorRGB operator *(float scale, in ColorRGB c) {
            return new ColorRGB(c.Red * scale, c.Green * scale, c.Blue * scale);
        }
        public static ColorRGB operator /(in ColorRGB c, float scale) {
            return new ColorRGB(c.Red / scale, c.Green / scale, c.Blue / scale);
        }

        public float Length() {
            return MathF.Sqrt(this.Dot(this));
        }
    }

    /// <summary>
    /// A color from 16-color palette.
    /// </summary>
    public struct Color4 : ANSIColor {

        public ConsoleColor Color { get; set; }

        public string Foreground => SGR.Foreground(this);
        public string Background => SGR.Background(this);

        public Color4(ConsoleColor consoleColor) {
            Color = consoleColor;
        }
        public static implicit operator Color4(ConsoleColor consoleColor) {
            return new Color4(consoleColor);
        }

        public ColorHSV ToHSV() {
            bool dark = (int)Color < 7 || (int)Color == 8;

            switch (Color) {
                case ConsoleColor.Black:
                case ConsoleColor.White:
                    return new(0, 0, dark ? 0 : 1);
                case ConsoleColor.DarkRed:
                case ConsoleColor.Red:
                    return new(0, 1, dark ? 0.5f : 1);
                case ConsoleColor.DarkYellow:
                case ConsoleColor.Yellow:
                    return new(60, 1, dark ? 0.5f : 1);
                case ConsoleColor.DarkGreen:
                case ConsoleColor.Green:
                    return new(120, 1, dark ? 0.5f : 1);
                case ConsoleColor.DarkCyan:
                case ConsoleColor.Cyan:
                    return new(180, 1, dark ? 0.5f : 1);
                case ConsoleColor.DarkBlue:
                case ConsoleColor.Blue:
                    return new(240, 1, dark ? 0.5f : 1);
                case ConsoleColor.DarkMagenta:
                case ConsoleColor.Magenta:
                    return new(300, 1, dark ? 0.5f : 1);
                case ConsoleColor.DarkGray:
                case ConsoleColor.Gray:
                    return new(0, 0, dark ? 0.5f : 0.75f);
            }

            throw new InvalidOperationException();
        }

        public ColorRGB ToRGB() {
            return this.ToHSV().ToRGB();
        }

        internal void MakeClosest<TColor>(in TColor color) where TColor : IColor {
            ColorRGB rgb = color.ToRGB();

            float minDist = 99f;
            for (ConsoleColor i = 0; i <= ConsoleColor.White; i++) {
                var c = new Color4(i).ToRGB();

                float dist = (c * c - rgb * rgb).Length();
                if (dist < minDist) {
                    minDist = dist;
                    Color = i;
                }
            }
        }

        public void From<TColor>(in TColor color) where TColor : IColor {

            ColorHSV hsv = color.ToHSV();
            if (hsv.Value < 0.30f) {
                Color = ConsoleColor.Black;
            } else if (hsv.Saturation < 1.3f * (hsv.Value - 0.25f) && hsv.Saturation < 0.4f) {
                float value = hsv.Value - hsv.Saturation * 0.3f;
                if (value < 0.625f) {
                    Color = ConsoleColor.DarkGray;
                } else if (value < 0.875f) {
                    Color = ConsoleColor.Gray;
                } else {
                    Color = ConsoleColor.White;
                }
            } else {
                bool dark = hsv.Value <= 0.75f;

                switch ((int)MathF.Round(hsv.Hue / 60) % 6) {
                    case 0:
                        Color = dark ? ConsoleColor.DarkRed : ConsoleColor.Red;
                        break;
                    case 1:
                        Color = dark ? ConsoleColor.DarkYellow : ConsoleColor.Yellow;
                        break;
                    case 2:
                        Color = dark ? ConsoleColor.DarkGreen : ConsoleColor.Green;
                        break;
                    case 3:
                        Color = dark ? ConsoleColor.DarkCyan : ConsoleColor.Cyan;
                        break;
                    case 4:
                        Color = dark ? ConsoleColor.DarkBlue : ConsoleColor.Blue;
                        break;
                    case 5:
                        Color = dark ? ConsoleColor.DarkMagenta : ConsoleColor.Magenta;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }//*/
        }

        public override string ToString() {
            return Color.ToString();
        }
    }

    /// <summary>
    /// A color from 256-color palette.
    /// </summary>
    public struct Color8 : ANSIColor {
        public byte Color { get; set; }

        public string Foreground => SGR.Foreground(this);
        public string Background => SGR.Background(this);

        public Color8(byte colorId) {
            Color = colorId;
        }

        /// <summary>
        /// Create a color from RGB 
        /// </summary>
        /// <param name="r">Red component (in range 0 to 5)</param>
        /// <param name="g">Green component (in range 0 to 5)</param>
        /// <param name="b">Blue component (in range 0 to 5)</param>
        public Color8(byte r, byte g, byte b) {
            if (r > 5 || g > 5 || b > 5) {
                throw new ArgumentOutOfRangeException($"(R,G,B) values must be in range [0..5], ({r},{g},{b}) given.");
            }
            Color = (byte)(16 + (r * 6 / 256 * 36 + g * 6 / 256 * 6 + b * 6 / 256));
        }

        public ColorHSV ToHSV() {
            return this.ToRGB().ToHSV();
        }

        public ColorRGB ToRGB() {
            throw new NotImplementedException();
        }

        public static bool operator ==(in Color8 a, in Color8 b)
                => a.Color == b.Color;
        public static bool operator !=(in Color8 a, in Color8 b)
                => a.Color != b.Color;

        public override bool Equals(object other) {

            if (other is Color24) {
                return other.Equals(this);
            }
            if (other is Color8) {
                return this == (Color8)other;
            }
            if (other is Color4) {
                Color8 clr8 = new();
                clr8.From((IColor)other);
                return this == clr8;
            }

            return base.Equals(other);
        }
        public override int GetHashCode() {
            return Color.GetHashCode();
        }

        public void From<TColor>(in TColor color) where TColor : IColor {
            var hsv = color.ToHSV();

            if (hsv.Saturation == 0.0f) {
                Color = (byte)(MathF.Pow(hsv.Value, 0.9f) * 24.999f + 232);
                if (Color == 0) {
                    Color = 15;
                }
            } else {
                var rgb = color.ToRGB();

                byte r = (byte)(rgb.Red * 255.999f);
                byte g = (byte)(rgb.Green * 255.999f);
                byte b = (byte)(rgb.Blue * 255.999f);

                Color = (byte)(16 + (r * 6 / 256 * 36 + g * 6 / 256 * 6 + b * 6 / 256));
            }
        }
    }

    /// <summary>
    /// A color from 24-bit palette.
    /// </summary>
    public struct Color24 : ANSIColor {

        public Color24(byte red, byte green, byte blue) {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public string Foreground => SGR.Foreground(this);
        public string Background => SGR.Background(this);

        public void From<TColor>(in TColor color) where TColor : IColor {
            var rgb = color.ToRGB();
            Red = (byte)(MathF.Min(255, MathF.Max(0, rgb.Red)) * 255f);
            Green = (byte)(MathF.Min(255, MathF.Max(0, rgb.Green)) * 255f);
            Blue = (byte)(MathF.Min(255, MathF.Max(0, rgb.Blue)) * 255f);
        }

        public static bool operator ==(in Color24 a, in Color24 b)
                => a.Red == b.Red
                && a.Green == b.Green
                && a.Blue == b.Blue;
        public static bool operator !=(in Color24 a, in Color24 b)
                => a.Red != b.Red
                || a.Green != b.Green
                || a.Blue != b.Blue;

        public override bool Equals(object other) {
            {
                if (other is Color24 clr) {
                    return this == clr;
                }
            }
            {
                if (other is Color8 || other is Color4) {
                    Color24 clr24 = new Color24();
                    clr24.From((IColor)other);
                    return this == clr24;
                }
            }

            return base.Equals(other);
        }
        public override int GetHashCode() {
            return (int)(BitOperations.RotateLeft((uint)Red.GetHashCode(), 5)
                ^ BitOperations.RotateLeft((uint)Green.GetHashCode(), 15)
                ^ BitOperations.RotateLeft((uint)Blue.GetHashCode(), 25));
        }

        public ColorHSV ToHSV() {
            ColorRGB c = new(Red / 255f, Green / 255f, Blue / 255f);
            return c.ToHSV();
        }

        public ColorRGB ToRGB() {
            return new(Red / 255f, Green / 255f, Blue / 255f);
        }

        public static readonly Color24
            MediumVioletRed = new(199, 21, 133),
            DeepPink = new(255, 20, 147),
            PaleVioletRed = new(219, 112, 147),
            HotPink = new(255, 105, 180),
            LightPink = new(255, 182, 193),
            Pink = new(255, 192, 203),
            DarkRed = new(139, 0, 0),
            RedColor = new(255, 0, 0),
            Firebrick = new(178, 34, 34),
            Crimson = new(220, 20, 60),
            IndianRed = new(205, 92, 92),
            LightCoral = new(240, 128, 128),
            Salmon = new(250, 128, 114),
            DarkSalmon = new(233, 150, 122),
            LightSalmon = new(255, 160, 122),
            OrangeRed = new(255, 69, 0),
            Tomato = new(255, 99, 71),
            DarkOrange = new(255, 140, 0),
            Coral = new(255, 127, 80),
            Orange = new(255, 165, 0),
            DarkKhaki = new(189, 183, 107),
            Gold = new(255, 215, 0),
            Khaki = new(240, 230, 140),
            PeachPuff = new(255, 218, 185),
            Yellow = new(255, 255, 0),
            PaleGoldenrod = new(238, 232, 170),
            Moccasin = new(255, 228, 181),
            PapayaWhip = new(255, 239, 213),
            LightGoldenrodYellow = new(250, 250, 210),
            LemonChiffon = new(255, 250, 205),
            LightYellow = new(255, 255, 224),
            Maroon = new(128, 0, 0),
            Brown = new(165, 42, 42),
            SaddleBrown = new(139, 69, 19),
            Sienna = new(160, 82, 45),
            Chocolate = new(210, 105, 30),
            DarkGoldenrod = new(184, 134, 11),
            Peru = new(205, 133, 63),
            RosyBrown = new(188, 143, 143),
            Goldenrod = new(218, 165, 32),
            SandyBrown = new(244, 164, 96),
            Tan = new(210, 180, 140),
            Burlywood = new(222, 184, 135),
            Wheat = new(245, 222, 179),
            NavajoWhite = new(255, 222, 173),
            Bisque = new(255, 228, 196),
            BlanchedAlmond = new(255, 235, 205),
            Cornsilk = new(255, 248, 220),
            DarkGreen = new(0, 100, 0),
            GreenColor = new(0, 128, 0),
            DarkOliveGreen = new(85, 107, 47),
            ForestGreen = new(34, 139, 34),
            SeaGreen = new(46, 139, 87),
            Olive = new(128, 128, 0),
            OliveDrab = new(107, 142, 35),
            MediumSeaGreen = new(60, 179, 113),
            LimeGreen = new(50, 205, 50),
            Lime = new(0, 255, 0),
            SpringGreen = new(0, 255, 127),
            MediumSpringGreen = new(0, 250, 154),
            DarkSeaGreen = new(143, 188, 143),
            MediumAquamarine = new(102, 205, 170),
            YellowGreen = new(154, 205, 50),
            LawnGreen = new(124, 252, 0),
            Chartreuse = new(127, 255, 0),
            LightGreen = new(144, 238, 144),
            GreenYellow = new(173, 255, 47),
            PaleGreen = new(152, 251, 152),
            Teal = new(0, 128, 128),
            DarkCyan = new(0, 139, 139),
            LightSeaGreen = new(32, 178, 170),
            CadetBlue = new(95, 158, 160),
            DarkTurquoise = new(0, 206, 209),
            MediumTurquoise = new(72, 209, 204),
            Turquoise = new(64, 224, 208),
            Aqua = new(0, 255, 255),
            Cyan = new(0, 255, 255),
            Aquamarine = new(127, 255, 212),
            PaleTurquoise = new(175, 238, 238),
            LightCyan = new(224, 255, 255),
            Navy = new(0, 0, 128),
            DarkBlue = new(0, 0, 139),
            MediumBlue = new(0, 0, 205),
            BlueColor = new(0, 0, 255),
            MidnightBlue = new(25, 25, 112),
            RoyalBlue = new(65, 105, 225),
            SteelBlue = new(70, 130, 180),
            DodgerBlue = new(30, 144, 255),
            DeepSkyBlue = new(0, 191, 255),
            CornflowerBlue = new(100, 149, 237),
            SkyBlue = new(135, 206, 235),
            LightSkyBlue = new(135, 206, 250),
            LightSteelBlue = new(176, 196, 222),
            LightBlue = new(173, 216, 230),
            PowderBlue = new(176, 224, 230),
            Indigo = new(75, 0, 130),
            Purple = new(128, 0, 128),
            DarkMagenta = new(139, 0, 139),
            DarkViolet = new(148, 0, 211),
            DarkSlateBlue = new(72, 61, 139),
            BlueViolet = new(138, 43, 226),
            DarkOrchid = new(153, 50, 204),
            Fuchsia = new(255, 0, 255),
            Magenta = new(255, 0, 255),
            SlateBlue = new(106, 90, 205),
            MediumSlateBlue = new(123, 104, 238),
            MediumOrchid = new(186, 85, 211),
            MediumPurple = new(147, 112, 219),
            Orchid = new(218, 112, 214),
            Violet = new(238, 130, 238),
            Plum = new(221, 160, 221),
            Thistle = new(216, 191, 216),
            Lavender = new(230, 230, 250),
            MistyRose = new(255, 228, 225),
            AntiqueWhite = new(250, 235, 215),
            Linen = new(250, 240, 230),
            Beige = new(245, 245, 220),
            WhiteSmoke = new(245, 245, 245),
            LavenderBlush = new(255, 240, 245),
            OldLace = new(253, 245, 230),
            AliceBlue = new(240, 248, 255),
            Seashell = new(255, 245, 238),
            GhostWhite = new(248, 248, 255),
            Honeydew = new(240, 255, 240),
            FloralWhite = new(255, 250, 240),
            Azure = new(240, 255, 255),
            MintCream = new(245, 255, 250),
            Snow = new(255, 250, 250),
            Ivory = new(255, 255, 240),
            White = new(255, 255, 255),
            Black = new(0, 0, 0),
            DarkSlateGray = new(47, 79, 79),
            DimGray = new(105, 105, 105),
            SlateGray = new(112, 128, 144),
            Gray = new(128, 128, 128),
            LightSlateGray = new(119, 136, 153),
            DarkGray = new(169, 169, 169),
            Silver = new(192, 192, 192),
            LightGray = new(211, 211, 211),
            Gainsboro = new(220, 220, 220);

        // source: https://en.wikipedia.org/wiki/Web_colors
    }
}
