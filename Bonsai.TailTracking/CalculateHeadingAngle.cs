using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the continuous heading angle using the first two elements in an array of points.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateHeadingAngle : Transform<Point2f[], double>
    {

        private bool initZeroAngle;
        [Description("Number of tail segments to calculate.")]
        public bool InitializeZeroAngle { get { return initZeroAngle; } set { initZeroAngle = value; } }

        public override IObservable<double> Process(IObservable<Point2f[]> source)
        {
            int count = 0;
            double? prevHeadingAngle = null;
            double? initHeadingAngle = null;
            return source.Select(value => 
            {
                double headingAngle = Math.Atan2(value[0].Y - value[1].Y, value[0].X - value[1].X);
                initHeadingAngle = initHeadingAngle == null && initZeroAngle ? headingAngle : 0;
                count = prevHeadingAngle != null && headingAngle - prevHeadingAngle > Math.PI ? count - 1 : prevHeadingAngle != null && headingAngle - prevHeadingAngle < -Math.PI ? count + 1 : count;
                prevHeadingAngle = headingAngle;
                return (headingAngle - (double)initHeadingAngle + count * 2 * Math.PI) * 180 / Math.PI;
            });
        }
    }
}