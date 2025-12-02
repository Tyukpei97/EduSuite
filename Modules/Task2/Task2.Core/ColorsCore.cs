using System;
using System.Collections.Generic;
using System.Drawing;

namespace Task2.Core
{
    public interface IColorParser
    {
        Color ParseHex(string hex);

        string ToHex(Color color);
    }

    public interface IColorMixer
    {
        Color Mix(IReadOnlyList<Color> colors);
    }

    public class SimpleColorParser : IColorParser
    {
        public Color ParseHex(string hex)
        {
            if (hex == null)
            {
                throw new ArgumentNullException(nameof(hex));
            }

            hex = hex.Trim();

            if (hex.StartsWith("#", StringComparison.Ordinal))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length != 6)
            {
                throw new FormatException("HEX-цвет должен быть в формате #RRGGBB.");
            }

            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);

            return Color.FromArgb(r, g, b);
        }

        public string ToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    public class AverageColorMixer : IColorMixer
    {
        public Color Mix(IReadOnlyList<Color> colors)
        {
            if (colors == null || colors.Count == 0)
            {
                throw new ArgumentException("Список цветов пуст.", nameof(colors));
            }

            long sumR = 0;
            long sumG = 0;
            long sumB = 0;

            foreach (var color in colors)
            {
                sumR += color.R;
                sumG += color.G;
                sumB += color.B;
            }

            byte r = (byte)(sumR / colors.Count);
            byte g = (byte)(sumG / colors.Count);
            byte b = (byte)(sumB / colors.Count);

            return Color.FromArgb(r, g, b);
        }
    }
}
