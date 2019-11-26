using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using Bonsai.Vision;

namespace Bonsai.TailTracking
{

    [Description("Calculates the eye angles by finding the angles of the minor axis of each circle enclosing the two largest binary thresholded regions that lie on the circumference of a circle centered around the centroid.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateEyeAngles : Transform<Tuple<Point2f[], ConnectedComponentCollection>, double[]>
    {

        public override IObservable<double[]> Process(IObservable<Tuple<Point2f[], ConnectedComponentCollection>> source)
        {
            return source.Select(value => CalculateEyeAnglesWithTailPointsFunc(value.Item2, value.Item1));
        }
        public IObservable<double[]> Process(IObservable<Tuple<ConnectedComponentCollection, Point2f[]>> source)
        {
            return source.Select(value => CalculateEyeAnglesWithTailPointsFunc(value.Item1, value.Item2));
        }
        public IObservable<double[]> Process(IObservable<Tuple<double, ConnectedComponentCollection>> source)
        {
            return source.Select(value => CalculateEyeAnglesWithHeadingAngleFunc(value.Item2, value.Item1));
        }
        public IObservable<double[]> Process(IObservable<Tuple<ConnectedComponentCollection, double>> source)
        {
            return source.Select(value => CalculateEyeAnglesWithHeadingAngleFunc(value.Item1, value.Item2));
        }
        private double[] CalculateEyeAnglesWithTailPointsFunc(ConnectedComponentCollection contours, Point2f[] points)
        {
            double headingAngle = Math.Atan2(points[0].Y - points[1].Y, points[0].X - points[1].X);
            return CalculateEyeAnglesWithHeadingAngleFunc(contours, headingAngle);
        }
        private double[] CalculateEyeAnglesWithHeadingAngleFunc(ConnectedComponentCollection contours, double headingAngle)
        {
            double[] eyeAngles = { double.NaN, double.NaN };
            if (contours.Count != 2)
            {
                return eyeAngles;
            }
            for (int i = 0; i < contours.Count; i++)
            {
                Point2f newPoint = Utilities.RotatePoint(new Point2f((float)(contours[i].MajorAxisLength * Math.Cos(contours[i].Orientation)), (float)(contours[i].MajorAxisLength * Math.Sin(contours[i].Orientation))), -headingAngle);
                eyeAngles[i] = Math.Atan2(newPoint.Y, newPoint.X);
                if (eyeAngles[i] < -Math.PI / 2.0)
                {
                    eyeAngles[i] += Math.PI;
                }
                else if (eyeAngles[i] > Math.PI / 2.0)
                {
                    eyeAngles[i] -= Math.PI;
                }
            }
            return eyeAngles;
        }
    }
}
