using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the continuous heading angle using the first two elements in an array of points.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateHeadingAngle : Transform<Point2f[], double>
    {
        [Description("Number of tail segments to calculate.")]
        public bool InitializeZeroAngle { get; set; }

        public override IObservable<double> Process(IObservable<Point2f[]> source)
        {
            int count = 0;
            double? prevHeadingAngle = null;
            double? initHeadingAngle = null;
            return source.Select(value => 
            {
                double headingAngle = Math.Atan2(value[0].Y - value[1].Y, value[0].X - value[1].X);
                initHeadingAngle = initHeadingAngle == null && InitializeZeroAngle ? headingAngle : 0;
                count = prevHeadingAngle != null && headingAngle - prevHeadingAngle > Math.PI ? count - 1 : prevHeadingAngle != null && headingAngle - prevHeadingAngle < -Math.PI ? count + 1 : count;
                prevHeadingAngle = headingAngle;
                return Utilities.ToDegrees(headingAngle - (double)initHeadingAngle + count * Utilities.twoPi);
            });
        }
    }
}