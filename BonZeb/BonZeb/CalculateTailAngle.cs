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

    public class CalculateTailAngle : Transform<TailPointData, TailAngleData<double[]>>
    {

        [Description("Normalize the tail angles to the heading angle, taken as the angle between the tail base point and the centroid.")]
        public bool NormalizeToHeading { get; set; }

        public CalculateTailAngle()
        {
            NormalizeToHeading = true;
        }

        public override IObservable<TailAngleData<double[]>> Process(IObservable<TailPointData> source)
        {
            return source.Select(value => new TailAngleData<double[]>(Utilities.CalculateTailAngle(value.Points, NormalizeToHeading), value.Points, value.Image));
        }
        public IObservable<TailAngleData<double[]>> Process(IObservable<Point2f[]> source)
        {
            return source.Select(value => new TailAngleData<double[]>(Utilities.CalculateTailAngle(value, NormalizeToHeading)));
        }
    }
}
