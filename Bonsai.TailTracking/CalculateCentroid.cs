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

    public class CalculateCentroid : Transform<IplImage, Point2f>
    {
        public override IObservable<Point2f> Process(IObservable<IplImage> source)
        {
            return source.Select(value => {
                Moments moments = new Moments(value, true);
                Point2f centroid = new Point2f((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
                return centroid;
            });
        }
    }
}
