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

    public class CalculateCumulativeTailAngle : Transform<TailPointData, TailAngleData<double>>
    {
        public override IObservable<TailAngleData<double>> Process(IObservable<TailPointData> source)
        {
            return source.Select(value => new TailAngleData<double>(Utilities.CalculateSum(Utilities.CalculateTailAngle(value.Points)), value.Points, value.Image));
        }

        public IObservable<TailAngleData<double>> Process(IObservable<Point2f[]> source)
        {
            return source.Select(value => new TailAngleData<double>(Utilities.CalculateSum(Utilities.CalculateTailAngle(value)), value));
        }

        public IObservable<TailAngleData<double>> Process(IObservable<TailAngleData<double[]>> source)
        {
            return source.Select(value => new TailAngleData<double>(Utilities.CalculateSum(value.Angles), value.Points, value.Image));
        }

        public IObservable<TailAngleData<double>> Process(IObservable<double[]> source)
        {
            return source.Select(value => new TailAngleData<double>(Utilities.CalculateSum(value)));
        }
    }
}
