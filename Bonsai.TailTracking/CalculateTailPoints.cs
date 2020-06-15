using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using OpenCV.Net;
using System.Runtime.InteropServices;

namespace Bonsai.TailTracking
{

    [Description("Calculates the tail points using the tail calculation method. Distances are measured in number of pixels in the image.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateTailPoints : Transform<CentroidData, TailPoints>
    {

        public CalculateTailPoints()
        {
            PixelSearchMethod = PixelSearchMethod.Brightest;
            DistTailBase = 12;
            HeadingDirection = -1;
            NumTailSegments = 7;
            DistTailPoints = 6;
            RangeTailPointAngles = 120;
            OffsetX = 0;
            OffsetY = 0;
            TailPointCalculationMethod = TailPointCalculationMethod.PixelSearch;
        }

        private int distTailBase;
        private Point2f[] potentialTailBasePoints;
        [Description("Distance between the eyes and the tail trunk in number of pixels. Only used for the EyeTracking method and Centroid method.")]
        public int DistTailBase { get => distTailBase; set { distTailBase = value > 0 ? value : 0; potentialTailBasePoints = Utilities.GeneratePointsArray(value); } }

        private double headingDirection;
        [Description("Angle of heading direction in degrees. If value is -1, no heading direction will be used. Otherwise, the heading direction supplied will seed the tail tracking search algorithm.")]
        public double HeadingDirection { get => headingDirection; set => headingDirection = value < 0 ? -1 : value > 360 ? 360 : value; }

        private int numTailSegments;
        [Description("Number of tail segments to calculate.")]
        public int NumTailSegments { get => numTailSegments; set => numTailSegments = value > 0 ? value : 0; }

        private int distTailPoints;
        private Point2f[] potentialTailPoints;
        [Description("Distance between tail points in number of pixels.")]
        public int DistTailPoints { get => distTailPoints; set { distTailPoints = value > 0 ? value : 0; potentialTailPoints = Utilities.GeneratePointsArray(value); } }

        private double rangeTailPointAngles;
        private double rangeAngles;
        private int nIterations;
        [Description("Range of angles in degrees for searching for points along the arc of the previous point and radius of the distance between tail points.")]
        public double RangeTailPointAngles { get => rangeTailPointAngles; set { rangeTailPointAngles = value < 0 ? 0 : value > 360 ? 360 : value; nIterations = (int)(value * potentialTailPoints.Length / 360); rangeAngles = value * Math.PI / 360; } }

        [Description("Method to use when searching for comparing pixel values. Darkest searches for darkest pixels in image whereas brightest searches for brightest pixels.")]
        public PixelSearchMethod PixelSearchMethod { get; set; }

        [Description("Offset to apply to X values of tail points.")]
        public int OffsetX { get; set; }

        [Description("Offset to apply to Y values of tail points.")]
        public int OffsetY { get; set; }

        [Description("The method used for calculating tail points.")]
        public TailPointCalculationMethod TailPointCalculationMethod { get; set; }
        public override IObservable<TailPoints> Process(IObservable<CentroidData> source)
        {
            return source.Select(value => CalculateTailPointsFunc(value.Centroid, value.Image));
        }

        public IObservable<TailPoints> Process(IObservable<Tuple<Point2f, IplImage>> source)
        {
            return source.Select(value => CalculateTailPointsFunc(value.Item1, value.Item2));
        }

        public IObservable<TailPoints> Process(IObservable<Tuple<IplImage, Point2f>> source)
        {
            return source.Select(value => CalculateTailPointsFunc(value.Item2, value.Item1));
        }

        public TailPoints CalculateTailPointsFunc(Point2f centroid, IplImage image)
        {
            int imageWidthStep = image.WidthStep;
            int imageHeight = image.Height;
            byte[] imageData = Utilities.ConvertIplImageToByteArray(image);

            Point2f[] points = new Point2f[numTailSegments + 2];
            if (centroid.X.Equals(float.NaN) || centroid.Y.Equals(float.NaN))
            {
                for (int i = 0; i < numTailSegments + 2; i++)
                {
                    points[i] = centroid;
                }
                return new TailPoints(points, image);
            }
            points[0] = centroid;
            Point2f[] newPotentialTailBasePoints = Utilities.OffsetPoints(potentialTailBasePoints, (int)centroid.X, (int)centroid.Y);
            points[1] = headingDirection == -1 ? FindNextPointWithPixelSearch(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData) : FindNextPointWithPixelSearch((int)(Utilities.ConvertDegreesToRadians(headingDirection) * newPotentialTailBasePoints.Length / (Math.PI * 2.0)), 1, newPotentialTailBasePoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData);
            for (int i = 1; i < numTailSegments + 1; i++)
            {
                double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                int startIteration = tailAngle < 0 ? (int)((tailAngle + (Math.PI * 2.0)) * potentialTailPoints.Length / (Math.PI * 2.0)) : (int)(tailAngle * potentialTailPoints.Length / (Math.PI * 2.0));
                Point2f[] newPotentialTailPoints = Utilities.OffsetPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                if (TailPointCalculationMethod == TailPointCalculationMethod.PixelSearch)
                {
                    points[i + 1] = FindNextPointWithPixelSearch(startIteration, nIterations, newPotentialTailPoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData);
                }
                else if (TailPointCalculationMethod == TailPointCalculationMethod.CenterOfMass)
                {
                    points[i + 1] = FindNextPointWithCenterOfMass(startIteration, nIterations, newPotentialTailPoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData);
                }
                else
                {
                    points[i + 1] = FindNextPointWithWeightedMedian(startIteration, nIterations, newPotentialTailPoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData);
                }
            }
            if (OffsetX != 0 || OffsetY != 0)
            {
                points = Utilities.OffsetPoints(points, OffsetX, OffsetY);
            }
            return new TailPoints(points, image);
        }

        public static Point2f FindNextPointWithPixelSearch(int startIteration, int nIterations, Point2f[] potentialPoints, PixelSearchMethod method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /*Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array.*/

            double XCoord = double.NaN;
            double YCoord = double.NaN;

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                float potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                float potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                if (double.IsNaN(XCoord) || double.IsNaN(YCoord))
                {
                    XCoord = potentialX;
                    YCoord = potentialY;
                }
                else if (method == PixelSearchMethod.Darkest)
                {
                    if (byteArray[(int)potentialY * frameWidth + (int)potentialX] < byteArray[(int)YCoord * frameWidth + (int)XCoord])
                    {
                        XCoord = potentialX;
                        YCoord = potentialY;
                    }
                }
                else
                {
                    if (byteArray[(int)potentialY * frameWidth + (int)potentialX] > byteArray[(int)YCoord * frameWidth + (int)XCoord])
                    {
                        XCoord = potentialX;
                        YCoord = potentialY;
                    }
                }
            }

            return new Point2f((float)XCoord, (float)YCoord);
        }

        public static Point2f FindNextPointWithCenterOfMass(int startIteration, int nIterations, Point2f[] potentialPoints, PixelSearchMethod method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /*Function that returns the center of mass along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array.*/

            double[] potentialXArray = new double[nIterations];
            double[] potentialYArray = new double[nIterations];
            double[] pixelValueArray = new double[nIterations];

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                double potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                double potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                double pixelValue = byteArray[(int)potentialY * frameWidth + (int)potentialX];
                potentialXArray[i] = potentialX;
                potentialYArray[i] = potentialY;
                pixelValueArray[i] = pixelValue;
            }
            double basePixelValue = double.NaN;
            for (int i = 0; i < pixelValueArray.Length; i++)
            {
                if (double.IsNaN(basePixelValue))
                {
                    basePixelValue = pixelValueArray[i];
                }
                else if (method == PixelSearchMethod.Darkest)
                {
                    if (basePixelValue < pixelValueArray[i])
                    {
                        basePixelValue = pixelValueArray[i];
                    }
                }
                else
                {
                    if (basePixelValue > pixelValueArray[i])
                    {
                        basePixelValue = pixelValueArray[i];
                    }
                }
            }
            double M00 = 0, M01 = 0, M10 = 0;
            for (int i = 0; i < pixelValueArray.Length; i++)
            {
                int weightedPixelValue = (int)Math.Abs(pixelValueArray[i] - basePixelValue);
                M00 += weightedPixelValue;
                M10 += weightedPixelValue * potentialXArray[i];
                M01 += weightedPixelValue * potentialYArray[i];
            }
            Point2f outputPoint = new Point2f(0, 0);
            if (M00 == 0)
            {
                return outputPoint;
            }
            //double angleCOM = Math.Atan2((M01 / M00) - origin.Y, (M10 / M00) - origin.X);
            double COMX = M10 / M00;
            double COMY = M01 / M00;
            double prevDist = double.PositiveInfinity;
            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                double currDist = Math.Sqrt(Math.Pow(potentialPoints[index].X - COMX, 2) + Math.Pow(potentialPoints[index].Y - COMY, 2));
                if (currDist < prevDist)
                {
                    outputPoint = new Point2f(potentialPoints[index].X, potentialPoints[index].Y);
                    prevDist = currDist;
                }
            }
            return outputPoint;
        }

        public static Point2f FindNextPointWithWeightedMedian(int startIteration, int nIterations, Point2f[] potentialPoints, PixelSearchMethod method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /*Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found by taking the median of the distribution of coordinates weighted by the difference of each coordinate from the value of the pixel with the brightest or darkest value specified by the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array.*/

            float[] potentialXArray = new float[nIterations];
            float[] potentialYArray = new float[nIterations];
            double[] pixelValueArray = new double[nIterations];

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                float potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                float potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                double pixelValue = byteArray[(int)potentialY * frameWidth + (int)potentialX];
                potentialXArray[i] = potentialX;
                potentialYArray[i] = potentialY;
                pixelValueArray[i] = pixelValue;
            }
            double basePixelValue = double.NaN;
            for (int i = 0; i < pixelValueArray.Length; i++)
            {
                if (double.IsNaN(basePixelValue))
                {
                    basePixelValue = pixelValueArray[i];
                }
                else if (method == PixelSearchMethod.Darkest)
                {
                    if (basePixelValue < pixelValueArray[i])
                    {
                        basePixelValue = pixelValueArray[i];
                    }
                }
                else
                {
                    if (basePixelValue > pixelValueArray[i])
                    {
                        basePixelValue = pixelValueArray[i];
                    }
                }
            }
            List<float> weightedXList = new List<float>();
            List<float> weightedYList = new List<float>();
            for (int i = 0; i < pixelValueArray.Length; i++)
            {
                for (int j = 0; j < (int)Math.Abs(pixelValueArray[i] - basePixelValue); j++)
                {
                    weightedXList.Add(potentialXArray[i]);
                    weightedYList.Add(potentialYArray[i]);
                }
            }
            return weightedXList.Count == 0 || weightedYList.Count == 0 ? new Point2f(0, 0) : new Point2f(weightedXList[weightedXList.Count / 2], weightedYList[weightedYList.Count / 2]);
        }
    }
}
