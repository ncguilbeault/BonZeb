using OpenTK;
using OpenCV.Net;
using System;
using System.Drawing;

namespace BonZeb.Design
{
    public sealed class Utilities
    {
        public static Color GetIdentityColor(int identity)
        {
            // Returns the color used to draw a tracked identity. Hues advance by the golden ratio so that consecutive identities are well separated on the color wheel.
            const double goldenRatio = 0.618033988749895;
            double hue = (identity * goldenRatio) % 1.0;
            double value = identity / 8 % 2 == 0 ? 1.0 : 0.7;
            return ColorFromHsv(hue, 0.9, value);
        }

        static Color ColorFromHsv(double hue, double saturation, double value)
        {
            // Converts a hue in the range [0, 1), with saturation and value in the range [0, 1], to an RGB color.
            double h = hue * 6;
            int sector = (int)Math.Floor(h) % 6;
            double f = h - Math.Floor(h);
            double p = value * (1 - saturation);
            double q = value * (1 - f * saturation);
            double t = value * (1 - (1 - f) * saturation);
            double r, g, b;
            switch (sector)
            {
                case 0: r = value; g = t; b = p; break;
                case 1: r = q; g = value; b = p; break;
                case 2: r = p; g = value; b = t; break;
                case 3: r = p; g = q; b = value; break;
                case 4: r = t; g = p; b = value; break;
                default: r = value; g = p; b = q; break;
            }
            return Color.FromArgb((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
        }

        public static Vector2 NormalizePoint(Point2f point, OpenCV.Net.Size imageSize)
        {
            return new Vector2((point.X * 2f / imageSize.Width) - 1, -((point.Y * 2f / imageSize.Height) - 1));
        }

        public static Vector2 NormalizePointForTailAngle(Point2f point, double angle, OpenCV.Net.Size imageSize)
        {
            return new Vector2(((point.X + (float)Math.Cos(angle) * 10f) * 2f / imageSize.Width) - 1, -(((point.Y + (float)Math.Sin(angle) * 10f) * 2f / imageSize.Height) - 1));
        }
    }
}
