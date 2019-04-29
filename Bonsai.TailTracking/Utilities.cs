using System;
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

        public struct RawMoments
        {
            public double M00;
            public double M01;
            public double M10;
            public void Initialize()
            {
                M00 = 0;
                M01 = 0;
                M10 = 0;
            }
        }

        public struct RawImageData
        {
            public byte[] ImageData;
            public int WidthStep;
            public int Height;
        }

        private static double[] LinSpace(double a, double b, int length)
        {

            /* Function that returns an array of values with known length linearly interspaced between values a and b. */
            double[] linSpace = new double[length];
            double range = b - a;
            double step = range / length;

            for (int i = 0; i < length; i++)
            {
                linSpace[i] = (double)a + (step * i);
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

            try
            {

                double[] angles = LinSpace(angle - rangeAngles / 2, angle + rangeAngles / 2, nAngles);

                for (int i = 0; i < angles.Length; i++)
                {
                    angles[i] = angles[i] * Math.PI / 180;
                    potential_x = Math.Min(Math.Max((int)Math.Round(initPoint.X + (radius * Math.Sin(angles[i])), 0, MidpointRounding.AwayFromZero), 0), frameWidth - 1);
                    potential_y = Math.Min(Math.Max((int)Math.Round(initPoint.Y + (radius * Math.Cos(angles[i])), 0, MidpointRounding.AwayFromZero), 0), frameHeight - 1);
                    if (i == 0)
                    {
                        point = new Point(potential_x, potential_y);
                    }
                    else if ((int)method == 0 && (int)byteArray[potential_y * frameWidth + potential_x] < (int)byteArray[point.Y * frameWidth + point.X])
                    {
                        point = new Point(potential_x, potential_y);
                    }
                    else if ((int)method == 1 && (int)byteArray[potential_y * frameWidth + potential_x] > (int)byteArray[point.Y * frameWidth + point.X])
                    {
                        point = new Point(potential_x, potential_y);
                    }
                }
            }
            catch
            {
                point = new Point(0, 0);
            }

            return point;
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

        public static byte[] ThresholdArray(ThresholdType thresholdType, byte[] frameData, double thresholdValue)
        {
            byte[] newFrameData = new byte[frameData.Length];
            for (int i = 0; i < frameData.Length; i++)
            {
                newFrameData[i] = (thresholdType == ThresholdType.Binary && frameData[i] > thresholdValue) || (thresholdType == ThresholdType.BinaryInvert && frameData[i] < thresholdValue) ? (byte)255 : (byte)0;
            }
            return newFrameData;
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
    }
}
