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

    public class CalculateCumulativeTailAngle : Transform<TailPoints, TailAngles<double>>
    {
        public override IObservable<TailAngles<double>> Process(IObservable<TailPoints> source)
        {
            return source.Select(value => new TailAngles<double>(Utilities.CalculateSum(Utilities.CalculateTailAngle(value.Points)), value.Points, value.Image));
        }

        public IObservable<TailAngles<double>> Process(IObservable<Point2f[]> source)
        {
            return source.Select(value => new TailAngles<double>(Utilities.CalculateSum(Utilities.CalculateTailAngle(value)), value));
        }

        public IObservable<TailAngles<double>> Process(IObservable<TailAngles<double[]>> source)
        {
            return source.Select(value => new TailAngles<double>(Utilities.CalculateSum(value.Angles), value.Points, value.Image));
        }

        public IObservable<TailAngles<double>> Process(IObservable<double[]> source)
        {
            return source.Select(value => new TailAngles<double>(Utilities.CalculateSum(value)));
        }
    }
}
