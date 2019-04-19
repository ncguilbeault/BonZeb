using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates tail curvature.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class TrackTail : Transform<IplImage, Point[]>
    {

        public TrackTail()
        {
            TailTrackingMethod = Utilities.TailTrackingMethod.EyeTracking;
            PixelSearch = Utilities.PixelSearch.Brightest;
            DistEyes = 5;
            NumEyeAngles = 20;
            TailBasePoint = new Point(0, 0);
            HeadingAngle = 180;
            ThresholdValue = 128;
            MaxValue = 255;
            ThresholdType = ThresholdTypes.Binary;
            ContourRetrievalMode = ContourRetrieval.List;
            ContourApproximationMethod = ContourApproximation.ChainApproxNone;
            DistTailBase = 12;
            NumTailBaseAngles = 20;
            NumTailPoints = 7;
            DistTailPoints = 6;
            RangeTailPointAngles = 120;
            NumTailPointAngles = 10;
        }

        [Description("Method to use for tail tracking. The options are EyeTracking, SeededTailBasePoint, and Centroid. EyeTracking utilizes a method for calculating the eyes first, followed by tracking the tail base and tail points. SeededTailBasePoint utilizes a method where the tail is tracked after the tail base is seeded by the given TailBase point. Centroid utilizes a method for thresholding and finding the centroid first followed by tracking the tail.")]
        public Utilities.TailTrackingMethod TailTrackingMethod { get; set; }

        [Description("Method to use when searching for Pixels. Darkest searches for darkest pixels in image whereas brightest searches for brightest pixels.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        [Description("Distance between the two eyes in number of pixels. Only used for the EyeTracking method.")]
        public int DistEyes { get; set; }

        [Description("Number of angles to use for searching for the second eye. Only used for the EyeTracking method.")]
        public int NumEyeAngles { get; set; }

        [Description("Point coordinate of the tail base. Only used for the SeededTailBasePoint method.")]
        public Point TailBasePoint { get; set; }

        [Description("Heading angle in degrees. Only used for the SeededTailBasePoint method.")]
        public double HeadingAngle { get; set; }

        [Description("Threshold value to use for finding the centroid. Only used for the Centroid method.")]
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [Description("The maximum value assigned to pixels determined to be above the threshold. Only used for the Centroid method.")]
        public double MaxValue { get; set; }

        [Description("The type of threshold to apply to individual pixels. Only used for the Centroid method.")]
        public ThresholdTypes ThresholdType { get; set; }

        [Description("Specifies the contour retrieval strategy. Only used for the Centroid method.")]
        public ContourRetrieval ContourRetrievalMode { get; set; }

        [Description("The approximation method used to output the contours. Only used for the Centroid method.")]
        public ContourApproximation ContourApproximationMethod { get; set; }

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

        public override IObservable<Point[]> Process(IObservable<IplImage> source)
        {

            return source.Select(value => {

                Utilities.TailTrackingMethod tailTrackingMethod = TailTrackingMethod;
                Utilities.PixelSearch pixelSearch = PixelSearch;

                int numTailPoints = NumTailPoints;
                int distTailPoints = DistTailPoints;
                double rangeTailPointAngles = RangeTailPointAngles;
                int numTailPointAngles = NumTailPointAngles;

                Point[] points = new Point[numTailPoints + 1];
                int frameWidth = value.Size.Width;
                int frameHeight = value.Size.Height;
                byte[] byteArray = new byte[frameWidth * frameHeight];
                Marshal.Copy(value.ImageData, byteArray, 0, frameWidth * frameHeight);

                double tailAngle = 0;
                Point tailBasePoint = new Point();

                if (tailTrackingMethod == Utilities.TailTrackingMethod.EyeTracking || tailTrackingMethod == Utilities.TailTrackingMethod.Centroid)
                {
                    int distTailBase = DistTailBase;
                    int numTailBaseAngles = NumTailBaseAngles;

                    if (tailTrackingMethod == Utilities.TailTrackingMethod.EyeTracking)
                    {
                        int distEyes = DistEyes;
                        int numEyeAngles = NumEyeAngles;
                        int x = 0;

                        for (int i = 0; i < byteArray.Length; i++)
                        {
                            if (pixelSearch == Utilities.PixelSearch.Darkest && (int)byteArray[i] < (int)byteArray[x])
                            {
                                x = i;
                            }
                            else
                            {
                                if (pixelSearch == Utilities.PixelSearch.Brightest && (int)byteArray[i] > (int)byteArray[x])
                                {
                                    x = i;
                                }
                            }
                        }
                        Point firstEyePoint = new Point((int)(x - (((int)x / frameWidth) * frameWidth)), (int)(x / frameWidth));
                        Point secondEyePoint = Utilities.CalculateNextPoint(0, 360, numEyeAngles, firstEyePoint, distEyes, pixelSearch, frameWidth, frameHeight, byteArray);
                        Point headingPoint = new Point((firstEyePoint.X + secondEyePoint.X) / 2, (firstEyePoint.Y + secondEyePoint.Y) / 2);
                        tailBasePoint = Utilities.CalculateNextPoint(0, 360, numTailBaseAngles, headingPoint, distTailBase, pixelSearch, frameWidth, frameHeight, byteArray);
                        tailAngle = Math.Atan2((tailBasePoint.X - headingPoint.X), (tailBasePoint.Y - headingPoint.Y)) * 180 / Math.PI;
                    }
                    else
                    {
                        double thresholdValue = ThresholdValue;
                        double maxValue = MaxValue;
                        ContourRetrieval contourRetrievalMode = ContourRetrievalMode;
                        ContourApproximation contourApproximationMethod = ContourApproximationMethod;
                        ThresholdTypes thresholdType = ThresholdType;
                        IplImage thresholdedImage = new IplImage(value.Size, value.Depth, value.Channels);
                        CV.Threshold(value, thresholdedImage, thresholdValue, maxValue, thresholdType);
                        Moments moments = new Moments(thresholdedImage, true);
                        Point centroid = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00));
                        tailBasePoint = Utilities.CalculateNextPoint(0, 360, numTailBaseAngles, centroid, distTailBase, pixelSearch, frameWidth, frameHeight, byteArray);
                        tailAngle = Math.Atan2((tailBasePoint.X - centroid.X), (tailBasePoint.Y - centroid.Y)) * 180 / Math.PI;
                    }
                }
                else
                {
                    tailBasePoint = TailBasePoint;
                    double headingAngle = HeadingAngle;
                    double thresholdValue = ThresholdValue;

                    if (headingAngle <= 180)
                    {
                        tailAngle = headingAngle + 180;
                    }
                    else
                    {
                        tailAngle = headingAngle - 180;
                    }
                }

                points[0] = tailBasePoint;

                for (int i = 0; i < numTailPoints; i++)
                {
                    if (i > 0)
                    {
                        tailAngle = Math.Atan2((points[i].X - points[i - 1].X), (points[i].Y - points[i - 1].Y));
                    }

                    points[i + 1] = Utilities.CalculateNextPoint(tailAngle, rangeTailPointAngles, numTailPointAngles, points[i], distTailPoints, pixelSearch, frameWidth, frameHeight, byteArray);
                }

                return points;

            });

        }

    }
}
