using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class CalculateCentroid : Transform<IplImage, Point2f>
    {
        public override IObservable<Point2f> Process(IObservable<IplImage> source)
        {
            Point2f previousCentroid = new Point2f(0, 0);
            return source.Select(value => {

                Moments moments = new Moments(value, true);
                Point2f centroid = new Point2f((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
                centroid = centroid.X - previousCentroid.X > -1 && centroid.X - previousCentroid.X < 1 && centroid.Y - previousCentroid.Y > -1 && centroid.Y - previousCentroid.Y < 1 ? previousCentroid : centroid;
                previousCentroid = centroid;
                return centroid;

            });
        }
    }
}
