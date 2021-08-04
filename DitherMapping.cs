using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TermColor {

    /// <summary>
    /// Represents a dithering character set with precomputed palette.
    /// </summary>
    /// This class can find closest approximation of a given color by mixing different colors and masking characters.
    /// It precomputes this approximation to a certain level of detail (see <see cref="Precompute(int)"/>)
    /// and can later find color aproximations (see <see cref="Map(in ColorRGB)"/>).
    public class DitherMapping {

        /// <summary>
        /// The default dithering preset with the following configuration:
        /// <code>        
        /// { new ( ' ', 0 / 4f ),
        ///   new ( '░', 1 / 4f ),
        ///   new ( '▒', 2 / 4f ),
        ///   new ( '▓', 3 / 4f ) }
        /// </code>
        /// </summary>
        public static readonly DitherMapping Default = new DitherMapping(new KeyValuePair<char, float>[] {
            new ( ' ', 0 / 4f ),
            new ( '░', 1 / 4f ),
            new ( '▒', 2 / 4f ),
            new ( '▓', 3 / 4f ),
        });

        /// <summary>
        /// Configuration of this preset. List of dithering characters with their respective opacities.
        /// </summary>
        public IReadOnlyDictionary<char, float> MaskOpacity => _maskOpacity;
        private ReadOnlyDictionary<char, float> _maskOpacity;

        /// <summary>
        /// Resolution of the precomputed palette.
        /// </summary>
        private int _resolution;

        private ColoredChar[,,] _data = null;

        /// <summary>
        /// True if the preset has already been precomputed.
        /// </summary>
        public bool IsComputed => _data != null;

        /// <summary>
        /// Resolution of the approximation.
        /// </summary>
        public int Resolution {
            get => IsComputed ? _resolution : throw new InvalidOperationException();
        }

        /// <summary>
        /// A tuple of (<see cref="ConsoleColor"/>, <see cref="ConsoleColor"/>, <see cref="char"/>)
        /// </summary>
        public struct ColoredChar {
            public ColoredChar(ConsoleColor fg, ConsoleColor bg, char mask) {
                Mask = mask;
                Foreground = fg;
                Background = bg;
            }
            public readonly char Mask;
            public readonly ConsoleColor Foreground, Background;

            public override string ToString() => $"{Foreground} {Background} {Mask}";

            public void Deconstruct(out ConsoleColor fg, out ConsoleColor bg, out char mask) {
                fg = Foreground;
                bg = Background;
                mask = Mask;
            }
        }

        /// <summary>
        /// Create a dithering character set.
        /// </summary>
        /// <param name="configuration">List of dithering characters with their respective opacities. See <see cref="Default"/> for an example value.</param>
        public DitherMapping(IEnumerable<KeyValuePair<char, float>> configuration) {
            _maskOpacity = new ReadOnlyDictionary<char, float>(new Dictionary<char, float>(configuration));
        }

        /// <summary>
        /// Mix two colors using the specified character.
        /// </summary>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        /// <param name="mask">The mask character.</param>
        /// <returns>The perceived color when the mask character is printed on screen with given colors.</returns>
        internal ColorRGB Mix(ColorRGB foreground, ColorRGB background, char mask) {
            float amount = _maskOpacity[mask];
            return ((foreground * foreground) * amount + (background * background) * (1 - amount)).Sqrt();
        }

        /// <summary>
        /// Find best approximation of <paramref name="color"/> using the precomputed data.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not precomputed.</exception>
        /// <param name="color">The color to approximate</param>
        /// <returns></returns>
        public ColoredChar Map(in ColorRGB color) {
            float size = Resolution - 1;
            int r = (int)MathF.Round(color.Red.Clamp(0, 1) * size);
            int g = (int)MathF.Round(color.Green.Clamp(0, 1) * size);
            int b = (int)MathF.Round(color.Blue.Clamp(0, 1) * size);

            var precomputed = _data[r, g, b];
            return new (precomputed.Foreground, precomputed.Background, precomputed.Mask);
        }

        /// <summary>
        /// Search the entire color space and find the closest approximation of <paramref name="color"/>.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        internal ColoredChar FindClosest(in ColorRGB color) {

            float minDist = 0;
            ColoredChar? result = null;

            foreach (var bg in Enum.GetValues<ConsoleColor>()) {
                foreach (var fg in Enum.GetValues<ConsoleColor>()) {
                    foreach (var mask in _maskOpacity.Keys) {

                        var candidate = Mix(new Color4(fg).ToRGB(), new Color4(bg).ToRGB(), mask);

                        float dist = (candidate * candidate - color * color).Length();
                        if (result == null || dist < minDist) {
                            minDist = dist;
                            result = new(fg, bg, mask);
                        }
                    }
                }
            }

            return result ?? throw new Exception("assertion failed");
        }

        /// <summary>
        /// Precompute the color mapping approximation.
        /// </summary>
        /// <param name="resolution"></param>
        public void Precompute(int resolution = 6) {
            if (IsComputed) {
                throw new InvalidOperationException("Preset already precomputed");
            }

            _data = new ColoredChar[resolution, resolution, resolution];
            _resolution = resolution;

            float size = resolution - 1f;

            for (int r = 0; r < resolution; r++) {
                for (int g = 0; g < resolution; g++) {
                    for (int b = 0; b < resolution; b++) {
                        _data[r, g, b] = FindClosest(new ColorRGB(r / size, g / size, b / size));
                    }
                }
            }
        }
    }
}
