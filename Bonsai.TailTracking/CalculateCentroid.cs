using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class CalculateCentroid : Transform<Utilities.RawImageData, Tuple<Utilities.RawImageData, Point2f>>
    {
        [Description("Threshold value to use for finding the centroid. Only used for the Centroid method.")]
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [Description("The type of threshold to apply to individual pixels. Only used for the Centroid method.")]
        public Utilities.ThresholdType ThresholdType { get; set; }

        public override IObservable<Tuple<Utilities.RawImageData, Point2f>> Process(IObservable<Utilities.RawImageData> source)
        {

            return source.Select(value => {

                Point2f centroid = Utilities.CalculateCentroid(value.ImageData, ThresholdType, ThresholdValue, value.WidthStep, value.Height);
                return Tuple.Create(value, centroid);

            });
        }
        public IObservable<Point2f> Process(IObservable<IplImage> source)
        {

            return source.Select(value => {

                Moments moments = new Moments(value, true);
                Point2f centroid = new Point2f((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
                return centroid;

            });
        }
    }
}
