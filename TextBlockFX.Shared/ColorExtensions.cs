using System;
using Windows.UI;

namespace TextBlockFX.Shared
{
    internal static class ColorExtensions
    {
        public static Color WithAlpha(this Color color, double alpha)
        {
            double safeAlpha = Math.Max(Math.Min(alpha, 1.0), 0.0);
            byte alphaByte = (byte)(safeAlpha * 255);
            return Color.FromArgb(alphaByte, color.R, color.G, color.B);
        }
    }
}
