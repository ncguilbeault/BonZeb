using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.Drawing.Design;
using System.Runtime.InteropServices;

namespace Bonsai.TailTracking
{

    [Description("Calculates the tail points using the tail calculation method. Distances are measured in number of pixels in the image.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateTailPoints : Transform<Tuple<Point2f, Utilities.RawImageData>, Point2f[]>
    {

        public CalculateTailPoints()
        {
            PixelSearch = Utilities.PixelSearch.Brightest;
            DistTailBase = 12;
            HeadingDirection = -1;
            NumTailSegments = 7;
            DistTailPoints = 6;
            RangeTailPointAngles = 120;
            OffsetX = 0;
            OffsetY = 0;
            TailCalculationMethod = Utilities.TailCalculationMethod.PixelSearch;
        }

        private int distTailBase;
        private Point2f[] potentialTailBasePoints;
        [Description("Distance between the eyes and the tail trunk in number of pixels. Only used for the EyeTracking method and Centroid method.")]
        public int DistTailBase { get => distTailBase; set { distTailBase = value > 0 ? value : 0; potentialTailBasePoints = Utilities.GeneratePotentialPoints(value); } }

        private double headingDirection;
        [Description("Angle of heading direction in degrees. If value is -1, no heading direction will be used. Otherwise, the heading direction supplied will seed the tail tracking search algorithm.")]
        public double HeadingDirection { get => headingDirection; set => headingDirection = value < 0 ? -1 : value > 360 ? 360 : value; }

        private int numTailSegments;
        [Description("Number of tail segments to calculate.")]
        public int NumTailSegments { get => numTailSegments; set => numTailSegments = value > 0 ? value : 0; }

        private int distTailPoints;
        private Point2f[] potentialTailPoints;
        [Description("Distance between tail points in number of pixels.")]
        public int DistTailPoints { get => distTailPoints; set { distTailPoints = value > 0 ? value : 0; potentialTailPoints = Utilities.GeneratePotentialPoints(value); } }

        private double rangeTailPointAngles;
        private double rangeAngles;
        private int nIterations;
        [Description("Range of angles in degrees for searching for points along the arc of the previous point and radius of the distance between tail points.")]
        public double RangeTailPointAngles { get => rangeTailPointAngles; set { rangeTailPointAngles = value < 0 ? 0 : value > 360 ? 360 : value; nIterations = (int)(value * potentialTailPoints.Length / 360); rangeAngles = value * Math.PI / 360; } }

        [Description("Method to use when searching for comparing pixel values. Darkest searches for darkest pixels in image whereas brightest searches for brightest pixels.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        [Description("Offset to apply to X values of tail points.")]
        public int OffsetX { get; set; }

        [Description("Offset to apply to Y values of tail points.")]
        public int OffsetY { get; set; }

        [Description("The method used for calculating tail points.")]
        public Utilities.TailCalculationMethod TailCalculationMethod { get; set; }

        public override IObservable<Point2f[]> Process(IObservable<Tuple<Point2f, Utilities.RawImageData>> source)
        {
            return source.Select(value => TailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch ? CalculateTailPointsByPixelSearchFunc(value.Item1, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData) : TailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass ? CalculateTailPointsByCenterOfMassFunc(value.Item1, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData) : CalculateTailPointsByWeightedMedianFunc(value.Item1, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData));
        }

        public IObservable<Point2f[]> Process(IObservable<Tuple<Utilities.RawImageData, Point2f>> source)
        {
            return source.Select(value => TailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch ? CalculateTailPointsByPixelSearchFunc(value.Item2, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData) : TailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass ? CalculateTailPointsByCenterOfMassFunc(value.Item2, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData) : CalculateTailPointsByWeightedMedianFunc(value.Item2, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData));
        }

        public IObservable<Point2f[]> Process(IObservable<Tuple<Point2f, IplImage>> source)
        {
            return source.Select(value => TailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch ? CalculateTailPointsByPixelSearchFunc(value.Item1, value.Item2.WidthStep, value.Item2.Height, Utilities.ConvertIplImageToByteArray(value.Item2)) : TailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass ? CalculateTailPointsByCenterOfMassFunc(value.Item1, value.Item2.WidthStep, value.Item2.Height, Utilities.ConvertIplImageToByteArray(value.Item2)) : CalculateTailPointsByWeightedMedianFunc(value.Item1, value.Item2.WidthStep, value.Item2.Height, Utilities.ConvertIplImageToByteArray(value.Item2)));
        }

        public IObservable<Point2f[]> Process(IObservable<Tuple<IplImage, Point2f>> source)
        {
            return source.Select(value => TailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch ? CalculateTailPointsByPixelSearchFunc(value.Item2, value.Item1.WidthStep, value.Item1.Height, Utilities.ConvertIplImageToByteArray(value.Item1)) : TailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass ? CalculateTailPointsByCenterOfMassFunc(value.Item2, value.Item1.WidthStep, value.Item1.Height, Utilities.ConvertIplImageToByteArray(value.Item1)) : CalculateTailPointsByWeightedMedianFunc(value.Item2, value.Item1.WidthStep, value.Item1.Height, Utilities.ConvertIplImageToByteArray(value.Item1)));
        }

        private Point2f[] CalculateTailPointsByPixelSearchFunc(Point2f centroid, int imageWidthStep, int imageHeight, byte[] imageData)
        {
            Point2f[] points = new Point2f[numTailSegments + 2];
            if (centroid.X.Equals(float.NaN) || centroid.Y.Equals(float.NaN))
            {
                for (int i = 0; i < numTailSegments + 2; i++)
                {
                    points[i] = centroid;
                }
                return points;
            }
            points[0] = centroid;
            Point2f[] newPotentialTailBasePoints = Utilities.OffsetPoints(potentialTailBasePoints, (int)centroid.X, (int)centroid.Y);
            points[1] = headingDirection == -1 ? Utilities.FindNextPointWithPixelSearch(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, imageWidthStep, imageHeight, imageData) : Utilities.FindNextPointWithPixelSearch((int)(Utilities.ConvertDegreesToRadians(headingDirection) * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, imageWidthStep, imageHeight, imageData);
            for (int i = 1; i < numTailSegments + 1; i++)
            {
                double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                Point2f[] newPotentialTailPoints = Utilities.OffsetPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                Point2f nextPoint = Utilities.FindNextPointWithPixelSearch(startIteration, nIterations, newPotentialTailPoints, PixelSearch, imageWidthStep, imageHeight, imageData);
                points[i + 1] = nextPoint;
            }
            points = OffsetX != 0 || OffsetY != 0 ? Utilities.OffsetPoints(points, OffsetX, OffsetY) : points;
            return points;
        }

        private Point2f[] CalculateTailPointsByCenterOfMassFunc(Point2f centroid, int imageWidthStep, int imageHeight, byte[] imageData)
        {
            Point2f[] points = new Point2f[numTailSegments + 2];
            if (centroid.X.Equals(float.NaN) || centroid.Y.Equals(float.NaN))
            {
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = centroid;
                }
                return points;
            }
            points[0] = centroid;
            Point2f[] newPotentialTailBasePoints = Utilities.OffsetPoints(potentialTailBasePoints, (int)centroid.X, (int)centroid.Y);
            points[1] = headingDirection == -1 ? Utilities.FindNextPointWithPixelSearch(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, imageWidthStep, imageHeight, imageData) : Utilities.FindNextPointWithPixelSearch((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, imageWidthStep, imageHeight, imageData);
            for (int i = 1; i < points.Length - 1; i++)
            {
                double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                Point2f[] newPotentialTailPoints = Utilities.OffsetPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                Point2f nextPoint = Utilities.FindNextPointWithCenterOfMass(startIteration, nIterations, newPotentialTailPoints, PixelSearch, imageWidthStep, imageHeight, imageData);
                points[i + 1] = nextPoint;
            }
            points = OffsetX != 0 || OffsetY != 0 ? Utilities.OffsetPoints(points, OffsetX, OffsetY) : points;
            return points;
        }

        private Point2f[] CalculateTailPointsByWeightedMedianFunc(Point2f centroid, int imageWidthStep, int imageHeight, byte[] imageData)
        {
            Point2f[] points = new Point2f[numTailSegments + 2];
            if (centroid.X.Equals(float.NaN) || centroid.Y.Equals(float.NaN))
            {
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = centroid;
                }
                return points;
            }
            points[0] = centroid;
            Point2f[] newPotentialTailBasePoints = Utilities.OffsetPoints(potentialTailBasePoints, (int)centroid.X, (int)centroid.Y);
            points[1] = headingDirection == -1 ? Utilities.FindNextPointWithPixelSearch(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, imageWidthStep, imageHeight, imageData) : Utilities.FindNextPointWithPixelSearch((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, imageWidthStep, imageHeight, imageData);
            for (int i = 1; i < points.Length - 1; i++)
            {
                double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                Point2f[] newPotentialTailPoints = Utilities.OffsetPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                Point2f nextPoint = Utilities.FindNextPointWithWeightedMedian(startIteration, nIterations, newPotentialTailPoints, PixelSearch, imageWidthStep, imageHeight, imageData);
                points[i + 1] = nextPoint;
            }
            points = OffsetX != 0 || OffsetY != 0 ? Utilities.OffsetPoints(points, OffsetX, OffsetY) : points;
            return points;
        }
    }
}
