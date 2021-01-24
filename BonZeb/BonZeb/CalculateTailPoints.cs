using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using OpenCV.Net;
using Bonsai;

namespace BonZeb
{

    [Description("Calculates the tail points using the tail calculation method. Distances are measured in number of pixels in the image.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateTailPoints : Transform<CentroidData, TailPointData>
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
        [Description("Distance between tail points in number of pixels. Value must be greater than or equal to 5.")]
        public int DistTailPoints { get => distTailPoints; set { distTailPoints = value > 5 ? value : 5; potentialTailPoints = Utilities.GeneratePointsArray(value); } }

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

        private int? tailTipContrastThreshold;
        [Description("Optional threshold to stop the tail point calculation method when the contrast detected is too low.")]
        public int? TailTipContrastThreshold { get => tailTipContrastThreshold; set => tailTipContrastThreshold = value.HasValue ? value : null; }

        [Description("The method used for calculating tail points.")]
        public TailPointCalculationMethod TailPointCalculationMethod { get; set; }

        public override IObservable<TailPointData> Process(IObservable<CentroidData> source)
        {
            return source.Select(value => GetTailPoints(value));
        }

        public IObservable<TailPointData> Process(IObservable<Tuple<Point2f, IplImage>> source)
        {
            return source.Select(value => GetTailPoints(value.Item1, value.Item2));
        }

        public IObservable<TailPointData> Process(IObservable<Tuple<IplImage, Point2f>> source)
        {
            return source.Select(value => GetTailPoints(value.Item2, value.Item1));
        }

        public IObservable<TailPointData> Process(IObservable<Tuple<Point2f, RawImageData>> source)
        {
            return source.Select(value => GetTailPoints(value.Item1, value.Item2));
        }

        public IObservable<TailPointData> Process(IObservable<Tuple<RawImageData, Point2f>> source)
        {
            return source.Select(value => GetTailPoints(value.Item2, value.Item1));
        }

        public TailPointData GetTailPoints(CentroidData data)
        {
            int imageWidthStep = data.Image.WidthStep;
            int imageHeight = data.Image.Height;
            byte[] imageData = data.BackgroundSubtractedImage == null ? Utilities.ConvertIplImageToByteArray(data.Image) : Utilities.ConvertIplImageToByteArray(data.BackgroundSubtractedImage);
            Point2f[] points = CalculateTailPointsFunc(data.Centroid, imageWidthStep, imageHeight, imageData);
            return new TailPointData(points, data.Image);
        }

        public TailPointData GetTailPoints(Point2f centroid, IplImage image)
        {
            int imageWidthStep = image.WidthStep;
            int imageHeight = image.Height;
            byte[] imageData = Utilities.ConvertIplImageToByteArray(image);
            Point2f[] points = CalculateTailPointsFunc(centroid, imageWidthStep, imageHeight, imageData);
            Console.WriteLine(points.Length);
            return new TailPointData(points, image);
        }

        public TailPointData GetTailPoints(Point2f centroid, RawImageData image)
        {
            int imageWidthStep = image.WidthStep;
            int imageHeight = image.Height;
            byte[] imageData = image.ImageData;
            Point2f[] points = CalculateTailPointsFunc(centroid, imageWidthStep, imageHeight, imageData);
            return new TailPointData(points, null);
        }

        private Point2f[] CalculateTailPointsFunc(Point2f centroid, int imageWidthStep, int imageHeight, byte[] imageData)
        {
            if (centroid.X.Equals(float.NaN) || centroid.Y.Equals(float.NaN))
            {
                return new Point2f[] { new Point2f(float.NaN, float.NaN) };
            }
            Point2f[] points = new Point2f[numTailSegments + 2];
            points[0] = centroid;
            Point2f[] newPotentialTailBasePoints = Utilities.OffsetPoints(potentialTailBasePoints, (int)centroid.X, (int)centroid.Y);
            bool success;
            Point2f point;
            (points[1], success) = headingDirection == -1 ? FindNextPointWithPixelSearch(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData, true) : FindNextPointWithPixelSearch((int)(Utilities.ConvertDegreesToRadians(headingDirection) * newPotentialTailBasePoints.Length / (Math.PI * 2.0)), 1, newPotentialTailBasePoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData, true);
            Console.WriteLine(success);
            for (int i = 1; i < points.Length - 1; i++)
            {
                double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - (rangeAngles / 2);
                int startIteration = tailAngle < 0 ? (int)((tailAngle + (Math.PI * 2.0)) * potentialTailPoints.Length / (Math.PI * 2.0)) : (int)(tailAngle * potentialTailPoints.Length / (Math.PI * 2.0));
                Point2f[] newPotentialTailPoints = Utilities.OffsetPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                if (TailPointCalculationMethod == TailPointCalculationMethod.PixelSearch)
                {
                    (point, success) = FindNextPointWithPixelSearch(startIteration, nIterations, newPotentialTailPoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData);
                }
                else if (TailPointCalculationMethod == TailPointCalculationMethod.CenterOfMass)
                {
                    (point, success) = FindNextPointWithCenterOfMass(startIteration, nIterations, newPotentialTailPoints, points[i], PixelSearchMethod, imageWidthStep, imageHeight, imageData);
                }
                else
                {
                    (point, success) = FindNextPointWithWeightedMedian(startIteration, nIterations, newPotentialTailPoints, PixelSearchMethod, imageWidthStep, imageHeight, imageData);
                }
                if (!success)
                {
                    Array.Resize(ref points, i + 1);
                    break;
                }
                points[i + 1] = point;
            }
            if (OffsetX != 0 || OffsetY != 0)
            {
                points = Utilities.OffsetPoints(points, OffsetX, OffsetY);
            }
            
            return points;
        }

        private (Point2f, bool) FindNextPointWithPixelSearch(int startIteration, int nIterations, Point2f[] potentialPoints, PixelSearchMethod method, int frameWidth, int frameHeight, byte[] byteArray, bool tailBase = false)
        {

            /*Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array.*/

            float minValue = float.PositiveInfinity, maxValue = float.NegativeInfinity;
            float[] potentialXArray = new float[nIterations], potentialYArray = new float[nIterations], pixelValueArray = new float[nIterations];
            Point2f point = new Point2f(float.NaN, float.NaN);

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                float potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                float potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                float pixelValue = byteArray[(int)potentialY * frameWidth + (int)potentialX];
                potentialXArray[i] = potentialX;
                potentialYArray[i] = potentialY;
                pixelValueArray[i] = pixelValue;
                minValue = Math.Min(pixelValue, minValue);
                maxValue = Math.Max(pixelValue, maxValue);
            }

            if (!tailBase && tailTipContrastThreshold.HasValue && maxValue - minValue < tailTipContrastThreshold.Value)
            {
                return (point, false);
            }

            float XCoord = float.NaN, YCoord = float.NaN;

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                float potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                float potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                if (float.IsNaN(XCoord) || float.IsNaN(YCoord))
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

            return (new Point2f(XCoord, YCoord), true);
        }

        private (Point2f, bool) FindNextPointWithCenterOfMass(int startIteration, int nIterations, Point2f[] potentialPoints, Point2f origin, PixelSearchMethod method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /*Function that returns the center of mass along an arc of known length from an initial point in an image.
            Point is found with the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array.*/

            float minValue = float.PositiveInfinity, maxValue = float.NegativeInfinity;
            float[] potentialXArray = new float[nIterations], potentialYArray = new float[nIterations], pixelValueArray = new float[nIterations];
            Point2f point = new Point2f(float.NaN, float.NaN);

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                float potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                float potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                float pixelValue = byteArray[(int)potentialY * frameWidth + (int)potentialX];
                potentialXArray[i] = potentialX;
                potentialYArray[i] = potentialY;
                pixelValueArray[i] = pixelValue;
                minValue = Math.Min(pixelValue, minValue);
                maxValue = Math.Max(pixelValue, maxValue);
            }

            if (tailTipContrastThreshold.HasValue && maxValue - minValue < tailTipContrastThreshold.Value)
            {
                return (point, false);
            }

            float basePixelValue = float.NaN;

            for (int i = 0; i < pixelValueArray.Length; i++)
            {
                if (float.IsNaN(basePixelValue))
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

            float M00 = 0, M01 = 0, M10 = 0;

            for (int i = 0; i < pixelValueArray.Length; i++)
            {
                int weightedPixelValue = (int)Math.Abs(pixelValueArray[i] - basePixelValue);
                M00 += weightedPixelValue;
                M10 += weightedPixelValue * potentialXArray[i];
                M01 += weightedPixelValue * potentialYArray[i];
            }

            if (M00 == 0)
            {
                return (point, false);
            }

            float COMX = M10 / M00;
            float COMY = M01 / M00;
            double prevDist = double.PositiveInfinity;
            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                double currDist = Math.Sqrt(Math.Pow(potentialPoints[index].X - COMX, 2) + Math.Pow(potentialPoints[index].Y - COMY, 2));
                if (currDist < prevDist)
                {
                    point = new Point2f(potentialPoints[index].X, potentialPoints[index].Y);
                    prevDist = currDist;
                }
            }
            return (point, true);
        }

        private (Point2f, bool) FindNextPointWithWeightedMedian(int startIteration, int nIterations, Point2f[] potentialPoints, PixelSearchMethod method, int frameWidth, int frameHeight, byte[] byteArray)
        {

            /*Function that returns a point that exists along an arc of known length from an initial point in an image.
            Point is found by taking the median of the distribution of coordinates weighted by the difference of each coordinate from the value of the pixel with the brightest or darkest value specified by the pixel search method.
            Requires the initial angle, range of angles, number of angles, initial point, radius, method, frame width, frame height, and frame data in the byte array.*/

            float minValue = float.PositiveInfinity, maxValue = float.NegativeInfinity;
            float[] potentialXArray = new float[nIterations], potentialYArray = new float[nIterations], pixelValueArray = new float[nIterations];
            Point2f point = new Point2f(float.NaN, float.NaN);

            for (int i = 0; i < nIterations; i++)
            {
                int index = (i + startIteration) < potentialPoints.Length ? i + startIteration : i + startIteration - potentialPoints.Length;
                float potentialX = Math.Min(Math.Max(potentialPoints[index].X, 0), frameWidth - 1);
                float potentialY = Math.Min(Math.Max(potentialPoints[index].Y, 0), frameHeight - 1);
                float pixelValue = byteArray[(int)potentialY * frameWidth + (int)potentialX];
                potentialXArray[i] = potentialX;
                potentialYArray[i] = potentialY;
                pixelValueArray[i] = pixelValue;
                minValue = Math.Min(pixelValue, minValue);
                maxValue = Math.Max(pixelValue, maxValue);
            }

            if (tailTipContrastThreshold.HasValue && maxValue - minValue < tailTipContrastThreshold.Value)
            {
                return (point, false);
            }

            float basePixelValue = float.NaN;

            for (int i = 0; i < pixelValueArray.Length; i++)
            {
                if (float.IsNaN(basePixelValue))
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

            List<float> weightedXList = new List<float>(), weightedYList = new List<float>();

            for (int i = 0; i < pixelValueArray.Length; i++)
            {
                for (int j = 0; j < (int)Math.Abs(pixelValueArray[i] - basePixelValue); j++)
                {
                    weightedXList.Add(potentialXArray[i]);
                    weightedYList.Add(potentialYArray[i]);
                }
            }

            if (weightedXList.Count == 0 || weightedYList.Count == 0)
            {
                return (point, false);
            }

            point = new Point2f(weightedXList[weightedXList.Count / 2], weightedYList[weightedYList.Count / 2]);

            return (point, true);
        }
    }
}
