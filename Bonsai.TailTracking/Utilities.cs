using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public sealed class Utilities
    {

        public static Point2f[] OffsetPoints(Point2f[] points, float offsetX, float offsetY)
        {
            // Function that applies an offset to an array of points.
            Point2f[] Points = new Point2f[points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new Point2f(points[i].X + offsetX, points[i].Y + offsetY);
            }
            return Points;
        }

        public static Point2f OffsetPoints(Point2f point, float offsetX, float offsetY)
        {
            // Function that applies an offset to a point.
            Point2f[] Points = new Point2f[0];
            Points[0] = new Point2f(point.X + offsetX, point.Y + offsetY);
            return Points[0];
        }

        public static Point2f[] RotatePoints(Point2f[] points, Point2f origin, double angle)
        {
            // Function that rotates a set of points around an origin by a given angle.
            Point2f[] Points = new Point2f[points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new Point2f((float)(((points[i].X - origin.X) * Math.Cos(angle) - (points[i].Y - origin.Y) * Math.Sin(angle)) + origin.X), (float)(((points[i].X - origin.X) * Math.Sin(angle) + (points[i].Y - origin.Y) * Math.Cos(angle)) + origin.Y));
            }
            return Points;
        }

        public static Point2f[] RotatePoints(Point2f[] points, double angle)
        {
            // Function that rotates a set of points by a given angle.
            Point2f[] Points = new Point2f[points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new Point2f((float)(points[i].X * Math.Cos(angle) - points[i].Y * Math.Sin(angle)), (float)(points[i].X * Math.Sin(angle) + points[i].Y * Math.Cos(angle)));
            }
            return Points;
        }
        public static Point2f RotatePoint(Point2f point, Point2f origin, double angle)
        {
            // Function that rotates a single point around an origin by a given angle.
            Point2f[] Points = new Point2f[0];
            Points[0] = new Point2f((float)(((point.X - origin.X) * Math.Cos(angle) - (point.Y - origin.Y) * Math.Sin(angle)) + origin.X), (float)(((point.X - origin.X) * Math.Sin(angle) + (point.Y - origin.Y) * Math.Cos(angle)) + origin.Y));
            return Points[0];
        }

        public static Point2f RotatePoint(Point2f point, double angle)
        {
            // Function that rotates a single point by a given angle.
            Point2f[] Points = new Point2f[0];
            Points[0] = new Point2f((float)(point.X * Math.Cos(angle) - point.Y * Math.Sin(angle)), (float)(point.X * Math.Sin(angle) + point.Y * Math.Cos(angle)));
            return Points[0];
        }

        public static Point2f[] GeneratePointsArray(int radius)
        {
            // Function that generates an array of points that lie on the circumference of a circle with a given radius and an origin at 0,0.
            List<Point2f> PointsList = new List<Point2f>();
            int X = radius;
            int Y = 0;
            PointsList.Add(new Point2f(X, Y));
            while (X >= Y)
            {
                Y++;
                if (Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2)) > radius)
                {
                    X--;
                }
                PointsList.Add(new Point2f(X, Y));
            }
            for (int i = PointsList.Count - 1; i >= 0; i--)
            { 
                if (PointsList[i].Y != PointsList[i].X)
                {
                    PointsList.Add(new Point2f(PointsList[i].Y, PointsList[i].X));
                }              
            }
            for (int i = PointsList.Count - 2; i >= 0; i--)
            {
                PointsList.Add(new Point2f(-PointsList[i].X, PointsList[i].Y));
            }
            for (int i = PointsList.Count - 2; i >= 1; i--)
            {
                PointsList.Add(new Point2f(PointsList[i].X, -PointsList[i].Y));
            }
            return PointsList.ToArray();
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

        public static double[] CalculateTailCurvature(Point2f[] points)
        {
            double headingAngle = -Math.Atan2(points[1].Y - points[0].Y, points[1].X - points[0].X);
            Point2f[] rotatedPoints = RotatePoints(points, points[0], headingAngle);
            double[] tailCurvature = new double[rotatedPoints.Length - 1];
            for (int i = 0; i < rotatedPoints.Length - 1; i++)
            {
                tailCurvature[i] = Math.Atan2(rotatedPoints[i + 1].Y - rotatedPoints[i].Y, rotatedPoints[i + 1].X - rotatedPoints[i].X);
                if (i > 0)
                {
                    if (tailCurvature[i] - tailCurvature[i - 1] > Math.PI)
                    {
                        tailCurvature[i] -= Math.PI * 2.0;
                    }
                    if (tailCurvature[i] - tailCurvature[i - 1] < -Math.PI)
                    {
                        tailCurvature[i] += Math.PI * 2.0;
                    }
                }
            }
            return tailCurvature;
        }

        public static double CalculateMean(double[] values)
        {
            return CalculateSum(values) / values.Length;
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
    }
}
