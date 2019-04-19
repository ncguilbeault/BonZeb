using System;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    public sealed class Utilities
    {
        public enum TailTrackingMethod { EyeTracking = 0, SeededTailBasePoint = 1, Centroid = 2 }

        public enum PixelSearch { Darkest = 0, Brightest = 1 }

        public enum LocationOfTailCurvature { StartOfTail = 0, MiddleOfTail = 1, EndOfTail = 2 }

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

    }

}
