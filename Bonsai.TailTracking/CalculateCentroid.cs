using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the centroid of a binarized image using the first-order raw image moments.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateCentroid : Transform<IplImage, CentroidData>
    {
        [Description("Threshold value to use for comparing pixel values.")]
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [Description("The type of threshold to apply to pixels.")]
        public ThresholdTypes ThresholdType { get; set; }

        private double maxValue;
        [Description("The value to set the pixels above the threshold value.")]
        public double MaxValue { get => maxValue; set => maxValue = value < 0 ? 0 : value > 255 ? 255 : value; }

        public override IObservable<CentroidData> Process(IObservable<IplImage> source)
        {
            return source.Select(value => 
            {
                IplImage thresh = new IplImage(value.Size, value.Depth, value.Channels);
                CV.Threshold(value, thresh, ThresholdValue, maxValue, ThresholdType);
                Moments moments = new Moments(thresh, true);
                Point2f centroid = new Point2f((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
                return new CentroidData(centroid, value, thresh);
            });
        }
    }
}
