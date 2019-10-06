using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    public sealed class Utilities
    {

        // Contains useful enum types, classes, and functions that are repeatedly used throughout the Bonsai.TailTracking package.

        public const double twoPi = Math.PI * 2;

        public enum PixelSearch
        {
            // Enum type used for searching pixel-wise extrema
            Darkest = 0,
            Brightest = 1
        }

        public enum ThresholdType
        {
            // Enum type used for determining threshold direction.
            Binary = 0,
            BinaryInvert = 1
        }

        public enum TailCalculationMethod
        {
            // Enum type used for tail calculation method.
            CenterOfMass = 0,
            PixelSearch = 1
        }

        public enum TailCurvatureDetectionMethod
        {
            // Enum type used for tail curvature detection method. Only used if an array of tail curvatures is provided.
            Cumulative = 0,
            Mean = 1,
            EndOfTail = 2,
            StartOfTail = 3,
            None = 4
        }

        public class RawImageData
        {
            // Class used for creating a raw image data type.
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

        public class TailBeatKinematics
        {
            // Class used for creating a data type which contains the amplitudes, frequency, and instances of bouts in tail curvature data.
            public double Frequency { get; set; }
            public double Amplitude { get; set; }
            public bool Instance { get; set; }
            public TailBeatKinematics(double freuency, double amplitude, bool instance)
            {
                Frequency = Frequency;
                Amplitude = amplitude;
                Instance = instance;
            } 
        }

        public class DrawParameters
        {
            // Class used for creating a draw parameters type.
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

        public static Point2f CalculateNextPoint(int startIteration, int nIterations, Point2f[] potentialPoints, PixelSearch method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /*Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array.*/

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

            /*Function that returns the center of mass along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array.*/

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
            // Function that generates an array of points that lie on the circumference of a circle with a given radius and an origin at 0,0.
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

        public static Point2f[] OffsetPoints(Point2f[] points, float offsetX, float offsetY)
        {
            // Function that applies an offset to an array of points.
            Point2f[] newPoints = new Point2f[points.Length];
            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = new Point2f(points[i].X + offsetX, points[i].Y + offsetY);
            }
            return newPoints;
        }

        public static Point2f OffsetPoint(Point2f point, float offsetX, float offsetY)
        {
            // Function that applies an offset to a point.
            return new Point2f(point.X + offsetX, point.Y + offsetY);
        }

        public static Point2f[] RotatePoints(Point2f[] points, Point2f origin, double angle)
        {
            // Function that rotates a set of points around an origin by a given angle.
            Point2f[] newPoints = new Point2f[points.Length];
            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = new Point2f((float)(((points[i].X - origin.X) * Math.Cos(angle) - (points[i].Y - origin.Y) * Math.Sin(angle)) + origin.X), (float)(((points[i].X - origin.X) * Math.Sin(angle) + (points[i].Y - origin.Y) * Math.Cos(angle)) + origin.Y));
            }
            return newPoints;
        }

        public static Point2f[] RotatePoints(Point2f[] points, double angle)
        {
            // Function that rotates a set of points by a given angle.
            Point2f[] newPoints = new Point2f[points.Length];
            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = new Point2f((float)(points[i].X * Math.Cos(angle) - points[i].Y * Math.Sin(angle)), (float)(points[i].X * Math.Sin(angle) + points[i].Y * Math.Cos(angle)));
            }
            return newPoints;
        }
        public static Point2f RotatePoint(Point2f point, Point2f origin, double angle)
        {
            // Function that rotates a single point around an origin by a given angle.
            return new Point2f((float)(((point.X - origin.X) * Math.Cos(angle) - (point.Y - origin.Y) * Math.Sin(angle)) + origin.X), (float)(((point.X - origin.X) * Math.Sin(angle) + (point.Y - origin.Y) * Math.Cos(angle)) + origin.Y));
        }

        public static Point2f RotatePoint(Point2f point, double angle)
        {
            // Function that rotates a single point by a given angle.
            return new Point2f((float)(point.X * Math.Cos(angle) - point.Y * Math.Sin(angle)), (float)(point.X * Math.Sin(angle) + point.Y * Math.Cos(angle)));
        }

        public static double ConvertRadiansToDegrees(double radians)
        {
            // Function that converts radians into degrees.
            return radians * 180 / Math.PI;
        }

        public static double[] ConvertRadiansToDegrees(double[] values)
        {
            // Function that converts array of values in radians into array of values in degrees.
            double[] output = new double[values.Length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = values[i] * 180 / Math.PI;
            }
            return output;
        }

        public static double ConvertDegreesToRadians(double degrees)
        {
            // Function that converts degrees into radians.
            return degrees * Math.PI / 180;
        }

        public static double[] ConvertDegreesToRadians(double[] values)
        {
            // Function that converts array of values in radians into array of values in degrees.
            double[] output = new double[values.Length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = values[i] * Math.PI / 180;
            }
            return output;
        }

        public static byte[] ConvertIplImageToByteArray(IplImage image)
        {
            // Function that converts an IplImage into a byte array.
            byte[] imageData = new byte[image.WidthStep * image.Height];
            Marshal.Copy(image.ImageData, imageData, 0, image.WidthStep * image.Height);
            return imageData;
        }

        public static double CalculateSum(double[] values)
        {
            // Function that takes an array of doubles and calculates the sum of the values.
            double sum = 0;
            foreach (double value in values)
            {
                sum += value;
            }
            return sum;
        }

        public static double CalculateMean(double[] values)
        {
            // Function that takes an array of doublas and calcualtes the mean of the values.
            return CalculateSum(values) / values.Length;
        }
    }
}
