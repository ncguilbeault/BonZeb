using System;
using System.Collections.Generic;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    public sealed class Utilities
    {
        public enum TailTrackingMethod
        {
            EyeTracking = 0,
            SeededTailBasePoint = 1,
            Centroid = 2
        }

        public enum PixelSearch
        {
            Darkest = 0,
            Brightest = 1
        }

        public enum LocationOfTailCurvature
        {
            StartOfTail = 0,
            MiddleOfTail = 1,
            EndOfTail = 2
        }

        public enum ThresholdType
        {
            Binary = 0,
            BinaryInvert = 1
        }

        public enum TailCalculationMethod
        {
            CenterOfMass = 0,
            PixelSearch = 1,
        }

        public class RawMoments
        {
            public double M00;
            public double M01;
            public double M10;
            public RawMoments()
            {

            }
            public void Initialize()
            {
                M00 = 0;
                M01 = 0;
                M10 = 0;
            }
        }

        public class RawImageData
        {
            public byte[] ImageData { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int WidthStep { get; set; }
            public RawImageData(byte[] imageData, int width, int height, int widthStep)
            {
                ImageData = imageData;
                Width = width;
                Height = height;
                WidthStep = widthStep;
            }
        }

        public class DrawParameters
        {
            public double XOffset { get; set; }
            public double YOffset { get; set; }
            public double XRange { get; set; }
            public double YRange { get; set; }
            public Scalar Colour { get; set; }
            public DrawParameters(double xOffset, double yOffset, double xRange, double yRange, Scalar colour)
            {
                XOffset = xOffset;
                YOffset = yOffset;
                XRange = xRange;
                YRange = yRange;
                Colour = colour;
            }
        }

        private static double[] LinSpace(double a, double b, int length)
        {

            /* Function that returns an array of values with known length linearly interspaced between values a and b. */
            double[] linSpace = new double[length];
            double range = b - a;
            double step = range / length;

            for (int i = 0; i < length; i++)
            {
                linSpace[i] = a + (step * i);
            }

            return linSpace;

        }
        public static Point2f CalculateNextPoint(int startIteration, int nIterations, Point2f[] potentialPoints, PixelSearch method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /* Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array. */

            Point2f point = new Point2f(0, 0);

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                float potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                float potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                point = i == 0 || (method == PixelSearch.Darkest && byteArray[(int)potentialY * frameWidth + (int)potentialX] < byteArray[(int)point.Y * frameWidth + (int)point.X]) || (method == PixelSearch.Brightest && byteArray[(int)potentialY * frameWidth + (int)potentialX] > byteArray[(int)point.Y * frameWidth + (int)point.X]) ? new Point2f(potentialX, potentialY) : point;
            }

            return point;
        }

        public static Point2f FindCenterOfMassAlongArc(int startIteration, int nIterations, Point2f[] potentialPoints, ThresholdType thresholdType, double thresholdValue, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /* Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array. */

            Point2f point = new Point2f(0, 0);
            double M00 = 0, M01 = 0, M10 = 0;

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                double potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                double potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                double pixelValue = (thresholdType == ThresholdType.Binary && byteArray[(int)potentialY * frameWidth + (int)potentialX] > thresholdValue) || (thresholdType == ThresholdType.BinaryInvert && byteArray[(int)potentialY * frameWidth + (int)potentialX] < thresholdValue) ? 255 : 0;
                M00 += pixelValue;
                M01 += pixelValue * potentialY;
                M10 += pixelValue * potentialX;
            }

            point = M00 > 0 ? new Point2f((float)(M10 / M00), (float)(M01 / M00)) : point;

            return point;
        }

        public static Point2f[] GeneratePotentialPoints(int radius)
        {
            List<Point2f> points = new List<Point2f>();
            int X = radius;
            int Y = 0;
            points.Add(new Point2f(X, Y));
            while (X > Y)
            {
                Y++;
                if (Math.Pow(X, 2) + Math.Pow(Y, 2) - Math.Pow(radius, 2) > 0)
                {
                    X--;
                }
                points.Add(new Point2f(X, Y));
            }
            for (int i = points.Count - 2; i >= 0; i--)
            {
                points.Add(new Point2f(points[i].Y, points[i].X));
            }
            for (int i = points.Count - 2; i >= 0; i--)
            {
                points.Add(new Point2f(-points[i].X, points[i].Y));
            }
            for (int i = points.Count - 2; i >= 1; i--)
            {
                points.Add(new Point2f(points[i].X, -points[i].Y));
            }
            return points.ToArray();
        }

        public static Point2f AddOffsetToPoint(Point2f point, int offsetX, int offsetY)
        {
            return new Point2f(point.X + offsetX, point.Y + offsetY);
        }

        public static Point2f[] AddOffsetToPoints(Point2f[] points, int offsetX, int offsetY)
        {
            Point2f[] newPoints = new Point2f[points.Length];
            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = new Point2f(points[i].X + offsetX, points[i].Y + offsetY);
            }
            return newPoints;
        }

        public static Point2f RotatePoint(Point2f point, Point2f origin, double angle)
        {
            return new Point2f((float)((point.X - origin.X) * Math.Cos(angle) - (point.Y - origin.Y) * Math.Sin(angle)), (float)((point.X - origin.X) * Math.Sin(angle) + (point.Y - origin.Y) * Math.Cos(angle)));
        }

        public static Point2f[] RotatePoints(Point2f[] points, Point2f origin, double angle)
        {
            Point2f[] newPoints = new Point2f[points.Length];
            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = new Point2f((float)((points[i].X - origin.X) * Math.Cos(angle) - (points[i].Y - origin.Y) * Math.Sin(angle)), (float)((points[i].X - origin.X) * Math.Sin(angle) + (points[i].Y - origin.Y) * Math.Cos(angle)));
            }
            return newPoints;
        }
    }
}
