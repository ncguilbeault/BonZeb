using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using Bonsai;

namespace BonZeb
{

    [Description("Calculates the tail curvature by rotating an array of points around the angle between the first two points and calculating the angle between each pair of remaining points.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateTailAngles : Transform<TailPoints, TailAngles<double[]>>
    {

        public override IObservable<TailAngles<double[]>> Process(IObservable<TailPoints> source)
        {
            return source.Select(value => new TailAngles<double[]>(Utilities.CalculateTailAngle(value.Points)));
        }
        public IObservable<double[]> Process(IObservable<Point2f[]> source)
        {
            return source.Select(value => Utilities.CalculateTailAngle(value));
        }
    }
}
