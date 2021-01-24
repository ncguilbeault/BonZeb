using OpenTK;
using OpenCV.Net;
using System;

namespace BonZeb.Design
{
    public sealed class Utilities
    {

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
