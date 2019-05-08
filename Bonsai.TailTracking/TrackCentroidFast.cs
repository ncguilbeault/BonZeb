using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    //public class TrackCentroidFast : Transform<Tuple<IntPtr, int, int>, Tuple<Point, IntPtr, int, int>>
    public class TrackCentroidFast : Transform<Tuple<IntPtr, int, int>, Point>
    {
        [Description("The region of interest inside the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect RegionOfInterest { get; set; }

        [Description("Threshold value to use for finding the centroid. Only used for the Centroid method.")]
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [Description("The type of threshold to apply to individual pixels. Only used for the Centroid method.")]
        public Utilities.ThresholdType ThresholdType { get; set; }

        //public override IObservable<Tuple<Point, IntPtr, int, int>> Process(IObservable<Tuple<IntPtr, int, int>> source)
        public override IObservable<Point> Process(IObservable<Tuple<IntPtr, int, int>> source)
        {

            return source.Select(value =>
            {
                Point centroid;
                unsafe
                {
                    int M00 = 0, M01 = 0, M10 = 0;
                    int widthStep;
                    if (RegionOfInterest.Width > 0 && RegionOfInterest.Height > 0)
                    {
                        widthStep = RegionOfInterest.Width % 4 == 0 ? RegionOfInterest.Width : (int)Math.Ceiling((decimal)RegionOfInterest.Width / 4) * 4;
                        byte* frameData = (byte*)value.Item1.ToPointer();
                        for (int i = 0; i < RegionOfInterest.Height; i++)
                        {
                            for (int j = 0; j < widthStep; j++)
                            {
                                int pixelValue = (ThresholdType == Utilities.ThresholdType.Binary && frameData[(RegionOfInterest.Y * value.Item2) + (i * value.Item2) + RegionOfInterest.X + j] > ThresholdValue) || (ThresholdType == Utilities.ThresholdType.BinaryInvert && frameData[(RegionOfInterest.Y * value.Item2) + (i * value.Item2) + RegionOfInterest.X + j] < ThresholdValue) ? (byte)255 : (byte)0;
                                M00 += pixelValue;
                                M01 += i * pixelValue;
                                M10 += j * pixelValue;
                            }
                        }
                        centroid = M00 > 0 ? new Point((M10 / M00), (M01 / M00)) : new Point(0, 0);
                    }
                    else
                    {
                        widthStep = value.Item2;
                        byte* frameData = (byte*)value.Item1.ToPointer();
                        for (int i = 0; i < value.Item3; i++)
                        {
                            for (int j = 0; j < value.Item2; j++)
                            {
                                int pixelValue = (ThresholdType == Utilities.ThresholdType.Binary && frameData[(i * value.Item2) + j] > ThresholdValue) || (ThresholdType == Utilities.ThresholdType.BinaryInvert && frameData[(i * value.Item2) + j] < ThresholdValue) ? (byte)255 : (byte)0;
                                M00 += pixelValue;
                                M01 += i * pixelValue;
                                M10 += j * pixelValue;
                            }
                        }
                        centroid = M00 > 0 ? new Point((M10 / M00), (M01 / M00)) : new Point(0, 0);
                    }
                }
                //return Tuple.Create(centroid, value.Item1, value.Item2, value.Item3);
                return centroid;
            });
        }

        public IObservable<Point> Process(IObservable<Tuple<Tuple<IntPtr, int, int>, IntPtr>> source)
        {

            return source.Select(value =>
            {
                Point centroid;
                unsafe
                {
                    int M00 = 0, M01 = 0, M10 = 0;
                    int widthStep;
                    if (RegionOfInterest.Width > 0 && RegionOfInterest.Height > 0)
                    {
                        widthStep = RegionOfInterest.Width % 4 == 0 ? RegionOfInterest.Width : (int)Math.Ceiling((decimal)RegionOfInterest.Width / 4) * 4;
                        byte* frameData = (byte*)value.Item1.Item1.ToPointer();
                        byte* backgroundData = (byte*)value.Item2.ToPointer();
                        for (int i = 0; i < RegionOfInterest.Height; i++)
                        {
                            for (int j = 0; j < widthStep; j++)
                            {
                                int pixelValue = Math.Abs(frameData[(RegionOfInterest.Y * value.Item1.Item2) + (i * value.Item1.Item2) + RegionOfInterest.X + j] - backgroundData[(RegionOfInterest.Y * value.Item1.Item2) + (i * value.Item1.Item2) + RegionOfInterest.X + j]);
                                int newPixelValue = (ThresholdType == Utilities.ThresholdType.Binary && pixelValue > ThresholdValue) || (ThresholdType == Utilities.ThresholdType.BinaryInvert && pixelValue < ThresholdValue) ? (byte)255 : (byte)0;
                                M00 += newPixelValue;
                                M01 += i * newPixelValue;
                                M10 += j * newPixelValue;
                            }
                        }
                        centroid = M00 > 0 ? new Point((M10 / M00), (M01 / M00)) : new Point(0, 0); 
                    }
                    else
                    {
                        widthStep = value.Item1.Item2;
                        byte* frameData = (byte*)value.Item1.Item1.ToPointer();
                        byte* backgroundData = (byte*)value.Item2.ToPointer();
                        for (int i = 0; i < value.Item1.Item3; i++)
                        {
                            for (int j = 0; j < value.Item1.Item2; j++)
                            {
                                int pixelValue = Math.Abs(frameData[(i * value.Item1.Item2) + j] - backgroundData[(i * value.Item1.Item2) + j]);
                                int newPixelValue = (ThresholdType == Utilities.ThresholdType.Binary && pixelValue > ThresholdValue) || (ThresholdType == Utilities.ThresholdType.BinaryInvert && pixelValue < ThresholdValue) ? (byte)255 : (byte)0;
                                M00 += newPixelValue;
                                M01 += i * newPixelValue;
                                M10 += j * newPixelValue;
                            }
                        }
                        centroid = M00 > 0 ? new Point((M10 / M00), (M01 / M00)) : new Point(0, 0);
                    }
                }
                //return Tuple.Create(centroid, value.Item1, value.Item2, value.Item3);
                return centroid;
            });
        }
    }
}
