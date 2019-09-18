using System;
using System.ComponentModel;
using System.Collections.Generic;
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
            return source.Select(value => CalculateEyeAnglesFunc(value.Item2, value.Item1));
        }
        public IObservable<double[]> Process(IObservable<Tuple<ConnectedComponentCollection, Point2f[]>> source)
        {
            return source.Select(value => CalculateEyeAnglesFunc(value.Item1, value.Item2));
        }
        double[] CalculateEyeAnglesFunc(ConnectedComponentCollection contours, Point2f[] points)
        {
            double[] eyeAngles = { 0, 0 };
            if (contours.Count < 2)
            {
                return eyeAngles;
            }
            double headingAngle = Math.Atan2(points[0].Y - points[1].Y, points[0].X - points[1].X);
            for (int i = 0; i < contours.Count; i++)
            {
                Point2f newPoint = Utilities.RotatePoint(new Point2f((float)(contours[i].MajorAxisLength * Math.Cos(contours[i].Orientation)), (float)(contours[i].MajorAxisLength * Math.Sin(contours[i].Orientation))), -headingAngle);
                eyeAngles[i] = Math.Atan2(newPoint.Y, newPoint.X);
                eyeAngles[i] = eyeAngles[i] < -Math.PI / 2 ? eyeAngles[i] + Math.PI : eyeAngles[i] > Math.PI / 2 ? eyeAngles[i] - Math.PI : eyeAngles[i];
            }
            return eyeAngles;
        }
    }
}
