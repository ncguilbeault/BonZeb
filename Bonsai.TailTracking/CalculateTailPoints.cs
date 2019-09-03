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
            ThresholdValue = 128;
            ThresholdType = Utilities.ThresholdType.Binary;
            TailCalculationMethod = Utilities.TailCalculationMethod.PixelSearch;
        }

        private int distTailBase;
        private Point2f[] potentialTailBasePoints;
        [Description("Distance between the eyes and the tail trunk in number of pixels. Only used for the EyeTracking method and Centroid method.")]
        public int DistTailBase { get => distTailBase; set { distTailBase = value > 0 ? value : 0; potentialTailBasePoints = Utilities.GeneratePotentialPoints(value); } }

        private int headingDirection;
        [Description("Angle of heading direction. If value is -1, no heading direction will be used. Otherwise, the heading direction supplied will seed the tail tracking search algorithm.")]
        public int HeadingDirection { get => headingDirection; set => headingDirection = value < 0 ? -1 : value > 360 ? 360 : value; }

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

        [Description("Method to use when searching for Pixels. Darkest searches for darkest pixels in image whereas brightest searches for brightest pixels. Only used for the PixelSearch tail calculation method.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        [Description("Offset to apply to X values of tail points.")]
        public int OffsetX { get; set; }

        [Description("Offset to apply to Y values of tail points.")]
        public int OffsetY { get; set; }

        [Description("Threshold value to use for finding the centroid. Only used for the CenterOfMass tail calculation method.")]
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [Description("The type of threshold to apply to individual pixels. Only used for the CenterOfMass tail calculation method.")]
        public Utilities.ThresholdType ThresholdType { get; set; }

        [Description("The method used for calculating tail points.")]
        public Utilities.TailCalculationMethod TailCalculationMethod { get; set; }

        public override IObservable<Point2f[]> Process(IObservable<Tuple<Point2f, Utilities.RawImageData>> source)
        {
            if (TailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailSegments + 2];
                    if (value.Item1.X.Equals(float.NaN) || value.Item1.Y.Equals(float.NaN))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = value.Item1;
                        }
                        return points;
                    }
                    points[0] = value.Item1;
                    Point2f[] newPotentialTailBasePoints = Utilities.AddOffsetToPoints(potentialTailBasePoints, (int)value.Item1.X, (int)value.Item1.Y);
                    points[1] = headingDirection == -1 ? Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData) : Utilities.CalculateNextPoint((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.CalculateNextPoint(startIteration, nIterations, newPotentialTailPoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                        points[i + 1] = nextPoint;
                    }
                    points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                    return points;
                });
            }
            else if (TailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailSegments + 2];
                    if (value.Item1.X.Equals(float.NaN) || value.Item1.Y.Equals(float.NaN))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = value.Item1;
                        }
                        return points;
                    }
                    points[0] = value.Item1;
                    Point2f[] newPotentialTailBasePoints = Utilities.AddOffsetToPoints(potentialTailBasePoints, (int)value.Item1.X, (int)value.Item1.Y);
                    points[1] = headingDirection == -1 ? Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData) : Utilities.CalculateNextPoint((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.FindCenterOfMassAlongArc(startIteration, nIterations, newPotentialTailPoints, ThresholdType, ThresholdValue, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                        points[i + 1] = nextPoint;
                    }
                    points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                    return points;
                });
            }
            return null;
        }

        public IObservable<Point2f[]> Process(IObservable<Tuple<Utilities.RawImageData, Point2f>> source)
        {
            Point2f[] previousPoints = new Point2f[0];
            if (TailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailSegments + 2];
                    if (value.Item2.X.Equals(float.NaN) || value.Item2.Y.Equals(float.NaN))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = value.Item2;
                        }
                        return points;
                    }
                    points[0] = value.Item2;
                    Point2f[] newPotentialTailBasePoints = Utilities.AddOffsetToPoints(potentialTailBasePoints, (int)value.Item2.X, (int)value.Item2.Y);
                    points[1] = headingDirection == -1 ? Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData) : Utilities.CalculateNextPoint((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData);
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.CalculateNextPoint(startIteration, nIterations, newPotentialTailPoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData);
                        points[i + 1] = nextPoint;
                    }
                    points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                    return points;
                });
            }
            else if (TailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailSegments + 2];
                    if (value.Item2.X.Equals(float.NaN) || value.Item2.Y.Equals(float.NaN))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = value.Item2;
                        }
                        return points;
                    }
                    points[0] = value.Item2;
                    Point2f[] newPotentialTailBasePoints = Utilities.AddOffsetToPoints(potentialTailBasePoints, (int)value.Item2.X, (int)value.Item2.Y);
                    points[1] = headingDirection == -1 ? Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData) : Utilities.CalculateNextPoint((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData);
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.FindCenterOfMassAlongArc(startIteration, nIterations, newPotentialTailPoints, ThresholdType, ThresholdValue, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData);
                        points[i + 1] = nextPoint;
                    }
                    points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                    return points;
                });
            }
            return null;
        }
        public IObservable<Point2f[]> Process(IObservable<Tuple<Point2f, IplImage>> source)
        {
            Point2f[] previousPoints = new Point2f[0];
            if (TailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailSegments + 2];
                    if (value.Item1.X.Equals(float.NaN) || value.Item1.Y.Equals(float.NaN))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = value.Item1;
                        }
                        return points;
                    }
                    points[0] = value.Item1;
                    Point2f[] newPotentialTailBasePoints = Utilities.AddOffsetToPoints(potentialTailBasePoints, (int)value.Item1.X, (int)value.Item1.Y);
                    byte[] imageData = new byte[value.Item2.WidthStep * value.Item2.Height];
                    Marshal.Copy(value.Item2.ImageData, imageData, 0, value.Item2.WidthStep * value.Item2.Height);
                    points[1] = headingDirection == -1 ? Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, imageData) : Utilities.CalculateNextPoint((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, imageData); 
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.CalculateNextPoint(startIteration, nIterations, newPotentialTailPoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, imageData);
                        points[i + 1] = nextPoint;
                    }
                    points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                    return points;
                });
            }
            else if (TailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailSegments + 2];
                    if (value.Item1.X.Equals(float.NaN) || value.Item1.Y.Equals(float.NaN))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = value.Item1;
                        }
                        return points;
                    }
                    points[0] = value.Item1;
                    Point2f[] newPotentialTailBasePoints = Utilities.AddOffsetToPoints(potentialTailBasePoints, (int)value.Item1.X, (int)value.Item1.Y);
                    byte[] imageData = new byte[value.Item2.WidthStep * value.Item2.Height];
                    Marshal.Copy(value.Item2.ImageData, imageData, 0, value.Item2.WidthStep * value.Item2.Height);
                    points[1] = headingDirection == -1 ? Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, imageData) : Utilities.CalculateNextPoint((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, imageData); 
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.FindCenterOfMassAlongArc(startIteration, nIterations, newPotentialTailPoints, ThresholdType, ThresholdValue, value.Item2.WidthStep, value.Item2.Height, imageData);
                        points[i + 1] = nextPoint;
                    }
                    points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                    return points;
                });
            }
            return null;
        }

        public IObservable<Point2f[]> Process(IObservable<Tuple<IplImage, Point2f>> source)
        {
            Point2f[] previousPoints = new Point2f[0];
            if (TailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailSegments + 2];
                    if (value.Item2.X.Equals(float.NaN) || value.Item2.Y.Equals(float.NaN))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = value.Item2;
                        }
                        return points;
                    }
                    points[0] = value.Item2;
                    Point2f[] newPotentialTailBasePoints = Utilities.AddOffsetToPoints(potentialTailBasePoints, (int)value.Item2.X, (int)value.Item2.Y);
                    byte[] imageData = new byte[value.Item1.WidthStep * value.Item1.Height];
                    Marshal.Copy(value.Item1.ImageData, imageData, 0, value.Item1.WidthStep * value.Item1.Height);
                    points[1] = headingDirection == -1 ? Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, imageData) : Utilities.CalculateNextPoint((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, imageData); 
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.CalculateNextPoint(startIteration, nIterations, newPotentialTailPoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, imageData);
                        points[i + 1] = nextPoint;
                    }
                    points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                    return points;
                });
            }
            else if (TailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailSegments + 2];
                    if (value.Item2.X.Equals(float.NaN) || value.Item2.Y.Equals(float.NaN))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = value.Item2;
                        }
                        return points;
                    }
                    points[0] = value.Item2;
                    Point2f[] newPotentialTailBasePoints = Utilities.AddOffsetToPoints(potentialTailBasePoints, (int)value.Item2.X, (int)value.Item2.Y);
                    byte[] imageData = new byte[value.Item1.WidthStep * value.Item1.Height];
                    Marshal.Copy(value.Item1.ImageData, imageData, 0, value.Item1.WidthStep * value.Item1.Height);
                    points[1] = headingDirection == -1 ? Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, imageData) : Utilities.CalculateNextPoint((int)(headingDirection * newPotentialTailBasePoints.Length / Utilities.twoPi), 1, newPotentialTailBasePoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, imageData); 
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + Utilities.twoPi) * potentialTailPoints.Length / Utilities.twoPi) : (int)(tailAngle * potentialTailPoints.Length / Utilities.twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.FindCenterOfMassAlongArc(startIteration, nIterations, newPotentialTailPoints, ThresholdType, ThresholdValue, value.Item1.WidthStep, value.Item1.Height, imageData);
                        points[i + 1] = nextPoint;
                    }
                    points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                    return points;
                });
            }
            return null;
        }
    }
}
