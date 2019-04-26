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
            ThresholdType = Utilities.ThresholdType.Binary;
            DistTailBase = 12;
            NumTailBaseAngles = 20;
            NumTailPoints = 7;
            DistTailPoints = 6;
            RangeTailPointAngles = 120;
            NumTailPointAngles = 10;
        }

        [Description("The region of interest inside the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect RegionOfInterest { get; set; }

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

        [Description("The type of threshold to apply to individual pixels. Only used for the Centroid method.")]
        public Utilities.ThresholdType ThresholdType { get; set; }

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

                Point[] points = new Point[NumTailPoints + 1];
                int frameHeight = value.Size.Height;
                int frameWidthStep = value.WidthStep;
                byte[] frameData = new byte[frameWidthStep * frameHeight];
                Marshal.Copy(value.ImageData, frameData, 0, frameWidthStep * frameHeight);

                if (RegionOfInterest.Width > 0 && RegionOfInterest.Height > 0)
                {
                    int widthStep = RegionOfInterest.Width % 4 == 0 ? RegionOfInterest.Width : (int)Math.Ceiling((decimal)RegionOfInterest.Width / 4) * 4;
                    byte[] newFrameData = new byte[widthStep * RegionOfInterest.Height];
                    for (int i = 0; i < RegionOfInterest.Height; i++)
                    {
                        for (int j = 0; j < widthStep; j++)
                        {
                            newFrameData[j + (i * widthStep)] = frameData[(RegionOfInterest.Y * frameWidthStep) + (i * frameWidthStep) + RegionOfInterest.X + j];
                        }
                    }
                    if (TailTrackingMethod == Utilities.TailTrackingMethod.EyeTracking)
                    {
                        points = Utilities.CalculateTailPointsUsingEyeTracking(NumTailPoints, PixelSearch, RegionOfInterest.Height, widthStep, newFrameData, RegionOfInterest.X, RegionOfInterest.Y, NumEyeAngles, DistEyes, NumTailBaseAngles, DistTailBase, RangeTailPointAngles, NumTailPointAngles, DistTailPoints);
                    }
                    else if (TailTrackingMethod == Utilities.TailTrackingMethod.Centroid)
                    {
                        points = Utilities.CalculateTailPointsUsingCentroid(NumTailPoints, RangeTailPointAngles, NumTailPointAngles, DistTailPoints, RegionOfInterest.Height, widthStep, newFrameData, RegionOfInterest.X, RegionOfInterest.Y, ThresholdType, DistTailBase, NumTailBaseAngles, PixelSearch, ThresholdValue);
                    }
                    else
                    {
                        points = Utilities.CalculateTailPointsUsingSeededTailBasePoint(NumTailPoints, RangeTailPointAngles, NumTailPointAngles, DistTailPoints, TailBasePoint, HeadingAngle, PixelSearch, widthStep, RegionOfInterest.Height, newFrameData, RegionOfInterest.X, RegionOfInterest.Y);
                    }
                }
                else
                {
                    if (TailTrackingMethod == Utilities.TailTrackingMethod.EyeTracking)
                    {
                        points = Utilities.CalculateTailPointsUsingEyeTracking(NumTailPoints, PixelSearch, frameHeight, frameWidthStep, frameData, 0, 0, NumEyeAngles, DistEyes, NumTailBaseAngles, DistTailBase, RangeTailPointAngles, NumTailPointAngles, DistTailPoints);
                    }
                    else if (TailTrackingMethod == Utilities.TailTrackingMethod.Centroid)
                    {
                        points = Utilities.CalculateTailPointsUsingCentroid(NumTailPoints, RangeTailPointAngles, NumTailPointAngles, DistTailPoints, frameHeight, frameWidthStep, frameData, 0, 0, ThresholdType, DistTailBase, NumTailBaseAngles, PixelSearch, ThresholdValue);
                    }
                    else
                    {
                        points = Utilities.CalculateTailPointsUsingSeededTailBasePoint(NumTailPoints, RangeTailPointAngles, NumTailPointAngles, DistTailPoints, TailBasePoint, HeadingAngle, PixelSearch, frameWidthStep, frameHeight, frameData, 0, 0);
                    }
                }

                return points;

            });
        }
    }
}
