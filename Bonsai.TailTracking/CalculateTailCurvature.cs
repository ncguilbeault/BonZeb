using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the tail curvature by rotating an array of points around the angle between the first two points and calculating the angle between each pair of remaining points.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateTailCurvature : Transform<Point2f[], double[]>
    {
        //private double[] prevTailCurvature = new double[0];
        public override IObservable<double[]> Process(IObservable<Point2f[]> source)
        {
            //prevTailCurvature = new double[0];
            return source.Select(value =>
            {
                double headingAngle = -Math.Atan2(value[1].Y - value[0].Y, value[1].X - value[0].X);
                Point2f[] points = Utilities.RotatePoints(value, value[0], headingAngle);
                double[] tailCurvature = new double[points.Length - 1];
                for (int i = 0; i < value.Length - 1; i++)
                {
                    tailCurvature[i] = Math.Atan2(points[i + 1].Y - points[i].Y, points[i + 1].X - points[i].X);
                    //if (prevTailCurvature.Length > 0)
                    //{
                    //    tailCurvature[i] = tailCurvature[i] - prevTailCurvature[i] > Math.PI ? tailCurvature[i] - Math.PI * 2 : tailCurvature[i] - prevTailCurvature[i] < -Math.PI ? tailCurvature[i] + Math.PI * 2 : tailCurvature[i];
                    //}
                }
                //prevTailCurvature = tailCurvature;
                return tailCurvature;
            });
        }
    }

}
