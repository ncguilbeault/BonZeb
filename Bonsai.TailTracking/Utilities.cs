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

        public static Point CalculateNextPoint(double angle, double rangeAngles, int nAngles, Point initPoint, int radius, PixelSearch method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /* Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array. */

            Point point = new Point(0, 0);
            int potential_x;
            int potential_y;
            double[] angles = LinSpace(angle - rangeAngles / 2, angle + rangeAngles / 2, nAngles);

            for (int i = 0; i < angles.Length; i++)
            {
                angles[i] = angles[i] * Math.PI / 180;
                potential_x = Math.Min(Math.Max((int)Math.Round(initPoint.X + (radius * Math.Sin(angles[i])), 0, MidpointRounding.AwayFromZero), 0), frameWidth - 1);
                potential_y = Math.Min(Math.Max((int)Math.Round(initPoint.Y + (radius * Math.Cos(angles[i])), 0, MidpointRounding.AwayFromZero), 0), frameHeight - 1);
                if (potential_x != point.X || potential_y != point.Y)
                {
                    point = i == 0 || (method == PixelSearch.Darkest && byteArray[potential_y * frameWidth + potential_x] < byteArray[point.Y * frameWidth + point.X]) || (method == PixelSearch.Brightest && byteArray[potential_y * frameWidth + potential_x] > byteArray[point.Y * frameWidth + point.X]) ? new Point(potential_x, potential_y) : point;
                }
            }
            return point;
        }

        public static Point2f CalculateNextPoint(double angle, double rangeAngles, int nAngles, Point2f initPoint, int radius, PixelSearch method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /* Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array. */

            Point2f point = new Point2f(0, 0);
            float potentialX, potentialY;
            double[] angles = LinSpace(angle - rangeAngles / 2, angle + rangeAngles / 2, nAngles);

            try
            {

                for (int i = 0; i < angles.Length; i++)
                {
                    potentialX = (float)Math.Min(Math.Max(initPoint.X + (radius * Math.Cos(angles[i])), 0), frameWidth - 1);
                    potentialY = (float)Math.Min(Math.Max(initPoint.Y + (radius * Math.Sin(angles[i])), 0), frameHeight - 1);
                    point = i == 0 || ((potentialX != point.X || potentialY != point.Y) && (method == PixelSearch.Darkest && byteArray[(int)potentialY * frameWidth + (int)potentialX] < byteArray[(int)point.Y * frameWidth + (int)point.X]) || (method == PixelSearch.Brightest && byteArray[(int)potentialY * frameWidth + (int)potentialX] > byteArray[(int)point.Y * frameWidth + (int)point.X])) ? new Point2f(potentialX, potentialY) : point;
                }

            }
            catch
            {
                point = new Point2f(0, 0);
            }

            return point;
        }

        public static Point2f CalculateNextPoint(double angle, double rangeAngles, Point2f[] potentialPoints, int radius, Point2f initPoint, PixelSearch method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /* Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array. */

            Point2f point = new Point2f(0, 0);
            //int startIndex = Array.FindIndex(potentialPoints, startPoint => startPoint == new Point((int)(radius * Math.Cos(angle - rangeAngles / 2)), (int)(radius * Math.Sin(angle - rangeAngles / 2))));
            //int endIndex = Array.FindIndex(potentialPoints, startPoint => startPoint == new Point((int)(radius * Math.Cos(angle + rangeAngles / 2)), (int)(radius * Math.Sin(angle + rangeAngles / 2))));
            //Console.WriteLine("Angle potential point 1: {0}.\nTail angle: {1}.\nRange angles: {2}.\n Acceptable point: {3}.\n", Math.Atan2(potentialPoints[1].Y, potentialPoints[1].X), angle, rangeAngles, Math.Atan2(potentialPoints[1].Y, potentialPoints[1].X) > angle - rangeAngles / 2 && Math.Atan2(potentialPoints[1].Y, potentialPoints[1].X) < angle + rangeAngles / 2);
            try
            {
                for (int i = 0; i < potentialPoints.Length; i++)
                {
                    float potentialX = Math.Min(Math.Max(initPoint.X + RotatePoint(potentialPoints[i], new Point2f(0, 0), -angle).X, 0), frameWidth - 1);
                    float potentialY = Math.Min(Math.Max(initPoint.Y + RotatePoint(potentialPoints[i], new Point2f(0, 0), -angle).Y, 0), frameHeight - 1);
                    point = i == 0 || (potentialX != point.X || potentialY != point.Y) && ((method == PixelSearch.Darkest && byteArray[(int)potentialY * frameWidth + (int)potentialX] < byteArray[(int)point.Y * frameWidth + (int)point.X]) || (method == PixelSearch.Brightest && byteArray[(int)potentialY * frameWidth + (int)potentialX] > byteArray[(int)point.Y * frameWidth + (int)point.X])) ? new Point2f(potentialX, potentialY) : point;
                }

            }
            catch
            {
                point = new Point2f(0, 0);
            }

            return point;
        }

        public static Point2f CalculateTailBasePoint(Point2f[] potentialPoints, int radius, Point2f initPoint, PixelSearch method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /* Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array. */

            Point2f point = new Point2f(0, 0);
            //int startIndex = Array.FindIndex(potentialPoints, startPoint => startPoint == new Point((int)(radius * Math.Cos(angle - rangeAngles / 2)), (int)(radius * Math.Sin(angle - rangeAngles / 2))));
            //int endIndex = Array.FindIndex(potentialPoints, startPoint => startPoint == new Point((int)(radius * Math.Cos(angle + rangeAngles / 2)), (int)(radius * Math.Sin(angle + rangeAngles / 2))));
            //Console.WriteLine("Angle potential point 1: {0}.\nTail angle: {1}.\nRange angles: {2}.\n Acceptable point: {3}.\n", Math.Atan2(potentialPoints[1].Y, potentialPoints[1].X), angle, rangeAngles, Math.Atan2(potentialPoints[1].Y, potentialPoints[1].X) > angle - rangeAngles / 2 && Math.Atan2(potentialPoints[1].Y, potentialPoints[1].X) < angle + rangeAngles / 2);
            try
            {
                for (int i = 0; i < potentialPoints.Length; i++)
                {
                    float potentialX = Math.Min(Math.Max(initPoint.X + potentialPoints[i].X, 0), frameWidth - 1);
                    float potentialY = Math.Min(Math.Max(initPoint.Y + potentialPoints[i].Y, 0), frameHeight - 1);
                    point = i == 0 || (potentialX != point.X || potentialY != point.Y) && ((method == PixelSearch.Darkest && byteArray[(int)potentialY * frameWidth + (int)potentialX] < byteArray[(int)point.Y * frameWidth + (int)point.X]) || (method == PixelSearch.Brightest && byteArray[(int)potentialY * frameWidth + (int)potentialX] > byteArray[(int)point.Y * frameWidth + (int)point.X])) ? new Point2f(potentialX, potentialY) : point;
                }

            }
            catch
            {
                point = new Point2f(0, 0);
            }

            return point;
        }

        public static Point2f FindCenterOfMassAlongArc(double angle, double rangeAngles, int nAngles, Point2f initPoint, int radius, ThresholdType thresholdType, double thresholdValue, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /* Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array. */

            Point2f point = new Point2f(0, 0);
            double[] angles = LinSpace(angle - rangeAngles / 2, angle + rangeAngles / 2, nAngles);
            int potentialX, potentialY, prevX = -1, prevY = -1;

            try
            {
                double M00 = 0, M01 = 0, M10 = 0;
                for (int i = 0; i < angles.Length; i++)
                {
                    //angles[i] = angles[i] * Math.PI / 180;
                    potentialX = (int)Math.Min(Math.Max(initPoint.X + (radius * Math.Cos(angles[i])), 0), frameWidth - 1);
                    potentialY = (int)Math.Min(Math.Max(initPoint.Y + (radius * Math.Sin(angles[i])), 0), frameHeight - 1);
                    if (potentialX != prevX || potentialY != prevY)
                    {
                        double pixelValue = (thresholdType == ThresholdType.Binary && byteArray[potentialY * frameWidth + potentialX] > thresholdValue) || (thresholdType == ThresholdType.BinaryInvert && byteArray[potentialY * frameWidth + potentialX] < thresholdValue) ? 255 : 0;
                        //double pixelValue = byteArray[(int)potentialY * frameWidth + (int)potentialX];
                        M00 += pixelValue;
                        M01 += pixelValue * potentialY;
                        M10 += pixelValue * potentialX;
                        prevX = potentialX;
                        prevY = potentialY;
                    }
                }
                point = new Point2f((float)(M10/M00), (float)(M01/M00));
            }
            catch
            {
                point = new Point2f(0, 0);
            }

            return point;
        }

        public static Point2f[] GeneratePotentialTailBasePoints(int radius)
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

        public static Point2f[] GeneratePotentialTailPoints(int radius, double rangeAngles)
        {
            List<Point2f> points = new List<Point2f>();
            float X = radius;
            float Y = 0;
            double pointAngle = Math.Atan2(Y, X);

            if (pointAngle <= rangeAngles)
            {
                points.Add(new Point2f(X, Y));
            }
            else
            {
                return points.ToArray();
            }
            
            while (X > Y)
            {
                Y++;
                if (Math.Pow(X, 2) + Math.Pow(Y, 2) - Math.Pow(radius, 2) > 0)
                {
                    X--;
                }
                pointAngle = Math.Atan2(Y, X);
                if (pointAngle <= rangeAngles)
                {
                    points.Add(new Point2f(X, Y));
                }
                else
                {
                    return points.ToArray();
                }
            }

            for (int i = points.Count - 2; i >= 0; i--)
            {
                X = points[i].Y;
                Y = points[i].X;

                pointAngle = Math.Atan2(Y, X);
                if (pointAngle <= rangeAngles)
                {
                    points.Add(new Point2f(X, Y));
                }
                else
                {
                    return points.ToArray();
                }
            }

            for (int i = points.Count - 2; i >= 0; i--)
            {
                X = -points[i].X;
                Y = points[i].Y;

                pointAngle = Math.Atan2(Y, X);
                if (pointAngle <= rangeAngles)
                {
                    points.Add(new Point2f(X, Y));
                }
                else
                {
                    return points.ToArray();
                }
            }

            for (int i = points.Count - 2; i >= 1; i--)
            {
                X = points[i].X;
                Y = -points[i].Y;

                pointAngle = Math.Atan2(Y, X);
                if (pointAngle <= rangeAngles)
                {
                    points.Add(new Point2f(X, Y));
                }
                else
                {
                    return points.ToArray();
                }
            }

            return points.ToArray();
        }

        public static Point[] CalculateTailPointsUsingEyeTracking(int numTailPoints, PixelSearch pixelSearch, int frameHeight, int frameWidth, byte[] frameData, int x_offset, int y_offset, int numEyeAngles, int distEyes, int numTailBaseAngles, int distTailBase, double rangeTailPointAngles, int numTailPointAngles, int distTailPoints)
        {
            Point[] points = new Point[numTailPoints + 1];
            int x = 0;
            int y = 0;

            for (int i = 0; i < frameHeight; i++)
            {
                for (int j = 0; j < frameWidth; j++)
                {
                    if (pixelSearch == PixelSearch.Darkest && (int)frameData[(i * frameWidth) + j] < (int)frameData[(y * frameWidth) + x])
                    {
                        y = i;
                        x = j;
                    }
                    else if (pixelSearch == PixelSearch.Brightest && (int)frameData[(i * frameWidth) + j] > (int)frameData[(y * frameWidth) + x])
                    {
                        y = i;
                        x = j;
                    }
                }
            }

            Point firstEyePoint = new Point(x, y);
            Point secondEyePoint = CalculateNextPoint(0, 360, numEyeAngles, firstEyePoint, distEyes, pixelSearch, frameWidth, frameHeight, frameData);
            Point headingPoint = new Point((firstEyePoint.X + secondEyePoint.X) / 2, (firstEyePoint.Y + secondEyePoint.Y) / 2);
            Point tailBasePoint = CalculateNextPoint(0, 360, numTailBaseAngles, headingPoint, distTailBase, pixelSearch, frameWidth, frameHeight, frameData);
            double tailAngle = Math.Atan2((tailBasePoint.X - headingPoint.X), (tailBasePoint.Y - headingPoint.Y)) * 180 / Math.PI;

            points[0] = tailBasePoint;

            for (int i = 0; i < numTailPoints; i++)
            {
                tailAngle = i > 0 ? Math.Atan2((points[i].X - points[i - 1].X), (points[i].Y - points[i - 1].Y)) * 180 / Math.PI : tailAngle;
                tailAngle = tailAngle > 360 ? tailAngle - 360 : tailAngle < 0 ? tailAngle + 360 : tailAngle;
                points[i + 1] = CalculateNextPoint(tailAngle, rangeTailPointAngles, numTailPointAngles, points[i], distTailPoints, pixelSearch, frameWidth, frameHeight, frameData);
            }

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X += x_offset;
                points[i].Y += y_offset;
            }

            return points;
        }

        public static Point[] CalculateTailPointsUsingSeededTailBasePoint(int numTailPoints, double rangeTailPointAngles, int numTailPointAngles, int distTailPoints, Point tailBasePoint, double headingAngle, PixelSearch pixelSearch, int frameWidth, int frameHeight, byte[] frameData, int x_offset, int y_offset)
        {
            Point[] points = new Point[numTailPoints + 1];
            double tailAngle = headingAngle <= 180 ? headingAngle + 180 : headingAngle - 180;

            points[0] = new Point (tailBasePoint.X - x_offset, tailBasePoint.Y - y_offset);

            for (int i = 0; i < numTailPoints; i++)
            {
                tailAngle = i > 0 ? Math.Atan2((points[i].X - points[i - 1].X), (points[i].Y - points[i - 1].Y)) * 180 / Math.PI : tailAngle;
                tailAngle = tailAngle > 360 ? tailAngle - 360 : tailAngle < 0 ? tailAngle + 360 : tailAngle;
                points[i + 1] = CalculateNextPoint(tailAngle, rangeTailPointAngles, numTailPointAngles, points[i], distTailPoints, pixelSearch, frameWidth, frameHeight, frameData);
            }

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X += x_offset;
                points[i].Y += y_offset;
            }

            return points;
        }

        public static Point[] CalculateTailPointsUsingCentroid(int numTailPoints, double rangeTailPointAngles, int numTailPointAngles, int distTailPoints, int frameHeight, int frameWidth, byte[] frameData, int x_offset, int y_offset, ThresholdType thresholdType, int distTailBase, int numTailBaseAngles, PixelSearch pixelSearch, double thresholdValue)
        {
            Point[] points = new Point[numTailPoints + 1];
            RawMoments moments = CalculateRawMoments(frameData, thresholdType, thresholdValue, frameWidth, frameHeight);
            Point centroid = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00));
            Point tailBasePoint = CalculateNextPoint(0, 360, numTailBaseAngles, centroid, distTailBase, pixelSearch, frameWidth, frameHeight, frameData);
            double tailAngle = Math.Atan2((tailBasePoint.X - centroid.X), (tailBasePoint.Y - centroid.Y)) * 180 / Math.PI;

            points[0] = tailBasePoint;

            for (int i = 0; i < numTailPoints; i++)
            {
                tailAngle = i > 0 ? Math.Atan2((points[i].X - points[i - 1].X), (points[i].Y - points[i - 1].Y)) * 180 / Math.PI : tailAngle;
                tailAngle = tailAngle > 360 ? tailAngle - 360 : tailAngle < 0 ? tailAngle + 360 : tailAngle;
                points[i + 1] = CalculateNextPoint(tailAngle, rangeTailPointAngles, numTailPointAngles, points[i], distTailPoints, pixelSearch, frameWidth, frameHeight, frameData);
            }

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X += x_offset;
                points[i].Y += y_offset;
            }

            return points;
        }

        public static RawMoments CalculateRawMoments(byte[] frameData, ThresholdType thresholdType, double thresholdValue, int frameWidth, int frameHeight)
        {
            RawMoments rawMoments = new RawMoments();
            rawMoments.Initialize();
            for (int i = 0; i < frameHeight; i++)
            {
                for (int j = 0; j < frameWidth; j++)
                {
                    double pixelValue = (thresholdType == ThresholdType.Binary && frameData[j + (i * frameWidth)] > thresholdValue) || (thresholdType == ThresholdType.BinaryInvert && frameData[j + (i * frameWidth)] < thresholdValue) ? 255 : 0;
                    rawMoments.M00 += pixelValue;
                    rawMoments.M01 += i * pixelValue;
                    rawMoments.M10 += j * pixelValue;
                }
            }
            return rawMoments;
        }

        public static Point2f CalculateCentroid(byte[] frameData, ThresholdType thresholdType, double thresholdValue, int frameWidth, int frameHeight)
        {
            RawMoments moments = CalculateRawMoments(frameData, thresholdType, thresholdValue, frameWidth, frameHeight);
            Point2f centroid = new Point2f((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
            return centroid;
        }

        public static Point2f[] AddOffsetToPoints(Point2f[] points, int offsetX, int offsetY)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point2f(points[i].X + offsetX, points[i].Y + offsetY);
            }
            return points;
        }

        public static Point2f RotatePoint(Point2f initPoint, Point2f origin, double angle)
        {
            return new Point2f((float)((initPoint.X - origin.X) * Math.Cos(angle) - (initPoint.Y - origin.Y) * Math.Sin(angle)), (float)((initPoint.X - origin.X) * Math.Sin(angle) + (initPoint.Y - origin.Y) * Math.Cos(angle)));
        }
    }
}
