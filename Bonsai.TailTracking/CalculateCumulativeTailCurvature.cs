using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the tail curvature by rotating an array of points around the angle between the first two points and calculating the angle between each pair of remaining points.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateCumulativeTailCurvature : Transform<double[], double>
    {
        public override IObservable<double> Process(IObservable<double[]> source)
        {
            return source.Select(value => Utilities.CalculateSum(value));
        }

        public IObservable<double> Process(IObservable<Point2f[]> source)
        {
            return source.Select(value => Utilities.CalculateSum(Utilities.CalculateTailCurvature(value)));
        }
    }
}
