using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.Drawing.Design;

namespace Bonsai.TailTracking
{
    public class CalculateTailPoints : Transform<Tuple<Point2f, Utilities.RawImageData>, Point2f[]>
    {

        public CalculateTailPoints()
        {
            PixelSearch = Utilities.PixelSearch.Brightest;
            DistTailBase = 12;
            NumTailBaseAngles = 20;
            NumTailPoints = 7;
            DistTailPoints = 6;
            RangeTailPointAngles = 120;
            NumTailPointAngles = 10;
            OffsetX = 0;
            OffsetY = 0;
            ThresholdValue = 128;
            ThresholdType = Utilities.ThresholdType.Binary;
            NoiseThreshold = 0;
            TailCalculationMethod = Utilities.TailCalculationMethod.PixelSearch;
        }

        [Description("Distance between the eyes and the tail trunk in number of pixels. Only used for the EyeTracking method and Centroid method.")]
        public int DistTailBase { get; set; }

        [Description("Number of angles to use for searching for the tail trunk. Only used for the EyeTracking method and Centroid method.")]
        public int NumTailBaseAngles { get; set; }

        [Description("Number of tail points to draw.")]
        public int NumTailPoints { get; set; }

        [Description("Distance between tail points in number of pixels.")]
        public int DistTailPoints { get; set; }

        [Description("Range of angles in degrees for searching for points along the arc of the previous point and radius of the distance between tail points.")]
        public double RangeTailPointAngles { get; set; }

        [Description("Number of angles to use for searching for pixels along the arc.")]
        public int NumTailPointAngles { get; set; }

        [Description("Method to use when searching for Pixels. Darkest searches for darkest pixels in image whereas brightest searches for brightest pixels. Only used for the PixelSearch tail calculation method.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        [Description("Apply offset to X values.")]
        public int OffsetX { get; set; }

        [Description("Apply offset to Y values.")]
        public int OffsetY { get; set; }

        [Description("Threshold value to use for finding the centroid. Only used for the CenterOfMass tail calculation method.")]
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [Description("The type of threshold to apply to individual pixels. Only used for the CenterOfMass tail calculation method.")]
        public Utilities.ThresholdType ThresholdType { get; set; }

        [Description("Noise threshold that is used to check if the potential next point is within a certain radius of the previous point. Uses the previous point if it is within this redius. Value of 0 will always use the new point.")]
        public double NoiseThreshold { get; set; }

        [Description("The method used for calculating tail points.")]
        public Utilities.TailCalculationMethod TailCalculationMethod { get; set; }

        public override IObservable<Point2f[]> Process(IObservable<Tuple<Point2f, Utilities.RawImageData>> source)
        {
            int distTailBase = DistTailBase;
            int numTailBaseAngles = NumTailBaseAngles;
            int numTailPoints = NumTailPoints;
            int distTailPoints = DistTailPoints;
            double rangeTailPointAngles = RangeTailPointAngles;
            int numTailPointAngles = NumTailPointAngles;
            Utilities.PixelSearch pixelSearch = PixelSearch;
            int offsetX = OffsetX;
            int offsetY = OffsetY;
            double thresholdValue = ThresholdValue;
            Utilities.ThresholdType thresholdType = ThresholdType;
            double noiseThreshold = NoiseThreshold;

            Point2f[] potentialTailBasePoints = Utilities.GeneratePotentialPoints(distTailBase);
            Point2f[] potentialTailPoints = Utilities.GeneratePotentialPoints(distTailPoints);
            int nIterations = (int)(rangeTailPointAngles * potentialTailPoints.Length / 360);
            double rangeAngles = rangeTailPointAngles * Math.PI / 360;
            double twoPi = Math.PI * 2;
            Point2f[] previousPoints = new Point2f[0];

            Utilities.TailCalculationMethod tailCalculationMethod = TailCalculationMethod;

            if (tailCalculationMethod == Utilities.TailCalculationMethod.PixelSearch)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailPoints + 2];
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
                    points[1] = Utilities.CalculateNextPoint(0, newPotentialTailBasePoints.Length, newPotentialTailBasePoints, pixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                    for (int i = 1; i < NumTailPoints + 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + twoPi) * potentialTailPoints.Length / twoPi) : (int)(tailAngle * potentialTailPoints.Length / twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.CalculateNextPoint(startIteration, nIterations, newPotentialTailPoints, pixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                        points[i + 1] = nextPoint;
                    }

                    points = offsetX != 0 || offsetY != 0 ? Utilities.AddOffsetToPoints(points, offsetX, offsetY) : points;
                    for (int i = 0; i < previousPoints.Length; i++)
                    {
                        points[i] = points[i].X - previousPoints[i].X > -(noiseThreshold / 2) && points[i].X - previousPoints[i].X < (noiseThreshold / 2) && points[i].Y - previousPoints[i].Y > -(noiseThreshold / 2) && points[i].Y - previousPoints[i].Y < (noiseThreshold / 2) ? previousPoints[i] : points[i];
                    }
                    previousPoints = points;
                    return points;
                });
            }
            else if (tailCalculationMethod == Utilities.TailCalculationMethod.CenterOfMass)
            {
                return source.Select(value =>
                {
                    Point2f[] points = new Point2f[numTailPoints + 2];
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
                    points[1] = Utilities.FindCenterOfMassAlongArc(0, potentialTailBasePoints.Length, newPotentialTailBasePoints, thresholdType, thresholdValue, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                    for (int i = 1; i < NumTailPoints + 1; i++)
                    {
                        double tailAngle = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) - rangeAngles;
                        int startIteration = tailAngle < 0 ? (int)((tailAngle + twoPi) * potentialTailPoints.Length / twoPi) : (int)(tailAngle * potentialTailPoints.Length / twoPi);
                        Point2f[] newPotentialTailPoints = Utilities.AddOffsetToPoints(potentialTailPoints, (int)points[i].X, (int)points[i].Y);
                        Point2f nextPoint = Utilities.FindCenterOfMassAlongArc(startIteration, nIterations, newPotentialTailPoints, thresholdType, thresholdValue, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                        points[i + 1] = nextPoint;
                    }

                    points = offsetX != 0 || offsetY != 0 ? Utilities.AddOffsetToPoints(points, offsetX, offsetY) : points;
                    for (int i = 0; i < previousPoints.Length; i++)
                    {
                        points[i] = points[i].X - previousPoints[i].X > -(noiseThreshold / 2) && points[i].X - previousPoints[i].X < (noiseThreshold / 2) && points[i].Y - previousPoints[i].Y > -(noiseThreshold / 2) && points[i].Y - previousPoints[i].Y < (noiseThreshold / 2) ? previousPoints[i] : points[i];
                    }
                    previousPoints = points;
                    return points;
                });
            }
            return null;
        }
    }
}
