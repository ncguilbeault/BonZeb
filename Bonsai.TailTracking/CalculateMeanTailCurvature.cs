using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.TailTracking
{

    [Description("Calculates the tail curvature by rotating an array of points around the angle between the first two points and calculating the angle between each pair of remaining points.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateMeanTailCurvature : Transform<double[], double>
    {
        public override IObservable<double> Process(IObservable<double[]> source)
        {
            return source.Select(value =>
            {
                double headingAngle = -Math.Atan2(value[1].Y - value[0].Y, value[1].X - value[0].X);
                Point2f[] points = Utilities.RotatePoints(value, value[0], headingAngle);
                double[] tailCurvature = new double[points.Length - 1];
                for (int i = 0; i < value.Length - 1; i++)
                {
                    tailCurvature[i] = Math.Atan2(points[i + 1].Y - points[i].Y, points[i + 1].X - points[i].X);
                    tailCurvature[i] = i > 0 && tailCurvature[i] - tailCurvature[i - 1] > Math.PI ? tailCurvature[i] - Math.PI * 2 : i > 0 && tailCurvature[i] - tailCurvature[i - 1] < -Math.PI ? tailCurvature[i] + Math.PI * 2 : tailCurvature[i];
                }
                return tailCurvature;
            });
        }
    }

}

