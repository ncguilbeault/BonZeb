using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the tail curvature by rotating an array of points around the angle between the first two points and calculating the angle between each pair of remaining points.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateTailAngles : Transform<Point2f[], TailAngles>
    {

        public override IObservable<TailAngles> Process(IObservable<Point2f[]> source)
        {
            return source.Select(value => CalculateTailAnglesFunc(value));
        }
        public IObservable<TailAngles> Process(IObservable<TailPoints> source)
        {
            return source.Select(value => CalculateTailAnglesFunc(value.Points, value.Image));
        }
        public static TailAngles CalculateTailAnglesFunc(Point2f[] points, IplImage image = null)
        {
            double headingAngle = -Math.Atan2(points[1].Y - points[0].Y, points[1].X - points[0].X);
            Point2f[] rotatedPoints = Utilities.RotatePoints(points, points[0], headingAngle);
            double[] tailAngles = new double[rotatedPoints.Length - 1];
            for (int i = 0; i < rotatedPoints.Length - 1; i++)
            {
                tailAngles[i] = Math.Atan2(rotatedPoints[i + 1].Y - rotatedPoints[i].Y, rotatedPoints[i + 1].X - rotatedPoints[i].X);
                if (i > 0)
                {
                    if (tailAngles[i] - tailAngles[i - 1] > Math.PI)
                    {
                        tailAngles[i] -= Math.PI * 2.0;
                    }
                    if (tailAngles[i] - tailAngles[i - 1] < -Math.PI)
                    {
                        tailAngles[i] += Math.PI * 2.0;
                    }
                }
            }
            return new TailAngles(tailAngles, points, image);
        }
    }
}
