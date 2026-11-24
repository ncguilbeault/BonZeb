using OpenCV.Net;
using System;
using System.Linq;

namespace BonZeb.Design
{
    internal static class DesignHelper
    {
        // Sizes are given for an image of this height and scaled to the height of the image drawn on.
        const float ReferenceHeight = 640;
        const float PointRadius = 2.5f;
        const float LineThickness = 3;
        const float SegmentExtension = 10;

        public static bool TryGetTails(object value, out Point2f[][] tails, out IplImage image, out bool multipleAnimals)
        {
            // Reads the tail points and the image from any of the tail data types, or from an array of them holding one element for each identity.
            Point2f[][] points;
            IplImage[] images;
            multipleAnimals = value is Array;
            switch (value)
            {
                case TailPointData tailPoints:
                    points = new[] { tailPoints.Points };
                    images = new[] { tailPoints.Image };
                    break;
                case TailAngleData<double> tailAngle:
                    points = new[] { tailAngle.Points };
                    images = new[] { tailAngle.Image };
                    break;
                case TailAngleData<double[]> tailCurvature:
                    points = new[] { tailCurvature.Points };
                    images = new[] { tailCurvature.Image };
                    break;
                case TailPointData[] tailPointsArray:
                    points = Array.ConvertAll(tailPointsArray, tail => tail?.Points);
                    images = Array.ConvertAll(tailPointsArray, tail => tail?.Image);
                    break;
                case TailAngleData<double>[] tailAngleArray:
                    points = Array.ConvertAll(tailAngleArray, tail => tail?.Points);
                    images = Array.ConvertAll(tailAngleArray, tail => tail?.Image);
                    break;
                case TailAngleData<double[]>[] tailCurvatureArray:
                    points = Array.ConvertAll(tailCurvatureArray, tail => tail?.Points);
                    images = Array.ConvertAll(tailCurvatureArray, tail => tail?.Image);
                    break;
                default:
                    tails = null;
                    image = null;
                    return false;
            }

            // Identities without points are kept as empty arrays so that the remaining identities keep their colors.
            tails = Array.ConvertAll(points, tail => tail ?? new Point2f[0]);
            image = images.FirstOrDefault(tailImage => tailImage != null);
            return true;
        }

        public static void DrawTailPoints(IplImage image, Point2f[][] tails, bool multipleAnimals)
        {
            // Draws each tail point as a filled circle.
            int radius = Scale(PointRadius, image);
            for (int i = 0; i < tails.Length; i++)
            {
                Scalar color = GetColor(i, multipleAnimals);
                foreach (Point2f point in tails[i])
                {
                    if (IsValid(point))
                    {
                        CV.Circle(image, ToPoint(point.X, point.Y), radius, color, -1, LineFlags.AntiAliased);
                    }
                }
            }
        }

        public static void DrawTailAngles(IplImage image, Point2f[][] tails, bool multipleAnimals)
        {
            // Draws each tail segment as a line extended beyond both of its points.
            int thickness = Scale(LineThickness, image);
            for (int i = 0; i < tails.Length; i++)
            {
                Scalar color = GetColor(i, multipleAnimals);
                Point2f[] points = tails[i];
                for (int j = 0; j < points.Length - 1; j++)
                {
                    if (!IsValid(points[j]) || !IsValid(points[j + 1]))
                    {
                        continue;
                    }
                    double angle = Math.Atan2(points[j + 1].Y - points[j].Y, points[j + 1].X - points[j].X);
                    double offsetX = Math.Cos(angle) * SegmentExtension;
                    double offsetY = Math.Sin(angle) * SegmentExtension;
                    CV.Line(image, ToPoint(points[j].X - offsetX, points[j].Y - offsetY), ToPoint(points[j + 1].X + offsetX, points[j + 1].Y + offsetY), color, thickness, LineFlags.AntiAliased);
                }
            }
        }

        static Scalar GetColor(int identity, bool multipleAnimals)
        {
            System.Drawing.Color color = multipleAnimals ? Utilities.GetIdentityColor(identity) : System.Drawing.Color.Red;
            return Scalar.Rgb(color.R, color.G, color.B);
        }

        static int Scale(float size, IplImage image)
        {
            return Math.Max(1, (int)Math.Round(size * image.Height / ReferenceHeight));
        }

        static bool IsValid(Point2f point)
        {
            return !float.IsNaN(point.X) && !float.IsNaN(point.Y) && !float.IsInfinity(point.X) && !float.IsInfinity(point.Y);
        }

        static Point ToPoint(double x, double y)
        {
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }
    }
}
