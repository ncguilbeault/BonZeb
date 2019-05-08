using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.Drawing.Design;

namespace Bonsai.TailTracking
{
    public class CalculateTailPoints : Transform<Tuple<Utilities.RawImageData, Point2f>, Point2f[]>
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

        [Description("Method to use when searching for Pixels. Darkest searches for darkest pixels in image whereas brightest searches for brightest pixels.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        [Description("Apply offset to X values.")]
        public int OffsetX { get; set; }

        [Description("Apply offset to Y values.")]
        public int OffsetY { get; set; }

        [Description("Threshold value to use for finding the centroid. Only used for the Centroid method.")]
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [Description("The type of threshold to apply to individual pixels. Only used for the Centroid method.")]
        public Utilities.ThresholdType ThresholdType { get; set; }

        public override IObservable<Point2f[]> Process(IObservable<Tuple<Utilities.RawImageData, Point2f>> source)
        {

            return source.Select(value => {

                Point2f[] points = new Point2f[NumTailPoints + 1];
                Point2f tailBasePoint = Utilities.CalculateNextPoint(0, 360, NumTailBaseAngles, value.Item2, DistTailBase, PixelSearch, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData);
                double tailAngle = Math.Atan2((tailBasePoint.X - value.Item2.X), (tailBasePoint.Y - value.Item2.Y)) * 180 / Math.PI;

                points[0] = tailBasePoint;

                for (int i = 0; i < NumTailPoints; i++)
                {
                    tailAngle = i > 0 ? Math.Atan2((points[i].X - points[i - 1].X), (points[i].Y - points[i - 1].Y)) * 180 / Math.PI : tailAngle;
                    tailAngle = tailAngle > 360 ? tailAngle - 360 : tailAngle < 0 ? tailAngle + 360 : tailAngle;
                    points[i + 1] = Utilities.CalculateNextPoint(tailAngle, RangeTailPointAngles, NumTailPointAngles, points[i], DistTailPoints, PixelSearch, value.Item1.WidthStep, value.Item1.Height, value.Item1.ImageData);
                }
                points = Utilities.AddOffsetToPoints(points, OffsetX, OffsetY);
                return points;

            });
        }
        public IObservable<Point2f[]> Process(IObservable<Tuple<Point2f, Utilities.RawImageData>> source)
        {
            Point[] potentialTailBasePoints = Utilities.GeneratePotentialPoints(DistTailBase);
            //Console.WriteLine("Potential tailbase point 0 : {0}.\nPotential tailbase point 5 : {1}.\nPotential tailbase point 10 : {2}.\n", potentialTailBasePoints[0], potentialTailBasePoints[5], potentialTailBasePoints[10]);
            Point[] potentialTailPoints = Utilities.GeneratePotentialPoints(DistTailPoints);
            //Console.WriteLine("Potential tailpoint 0 : {0}.\nPotential tailpoint 5 : {1}.\nPotential tailpoint 10 : {2}.\n", potentialTailPoints[0], potentialTailPoints[5], potentialTailPoints[10]);
            return source.Select(value =>
            {

                Point2f[] points = new Point2f[NumTailPoints + 1];
                //points[0] = Utilities.CalculateNextPoint(0, 2 * Math.PI, NumTailBaseAngles, value.Item1, DistTailBase, PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                points[0] = Utilities.CalculateNextPoint(0, 2 * Math.PI, potentialTailBasePoints, DistTailBase, value.Item1, PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);

                for (int i = 0; i < NumTailPoints; i++)
                {
                    double tailAngle = i > 0 ? Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) : Math.Atan2(points[i].Y - value.Item1.Y, points[i].X - value.Item1.X);
                    //points[i + 1] = Utilities.CalculateNextPoint(tailAngle, RangeTailPointAngles * Math.PI / 180, NumTailPointAngles, points[i], DistTailPoints, PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                    points[i + 1] = Utilities.CalculateNextPoint(tailAngle, RangeTailPointAngles * Math.PI / 180, potentialTailPoints, DistTailPoints, points[i], PixelSearch, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                    //points[i + 1] = Utilities.FindCenterOfMassAlongArc(tailAngle, RangeTailPointAngles * Math.PI / 180, NumTailPointAngles, points[i], DistTailPoints, ThresholdType, ThresholdValue, value.Item2.WidthStep, value.Item2.Height, value.Item2.ImageData);
                }
                points = OffsetX != 0 || OffsetY != 0 ? Utilities.AddOffsetToPoints(points, OffsetX, OffsetY) : points;
                return points;

            });
        }
    }
}
