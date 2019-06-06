using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class CalculateHeadingDirection : Transform<Point2f[], double>
    {
        public override IObservable<double> Process(IObservable<Point2f[]> source)
        {
            int count = 0;
            double? prevHeadingAngle = null;
            double? initHeadingAngle = null;
            return source.Select(value => 
            {
                double headingAngle = Math.Atan2(value[0].Y - value[1].Y, value[0].X - value[1].X);
                count = prevHeadingAngle != null && headingAngle - prevHeadingAngle > Math.PI ? count - 1 : prevHeadingAngle != null && headingAngle - prevHeadingAngle < -Math.PI ? count + 1 : count;
                initHeadingAngle = initHeadingAngle == null ? headingAngle : initHeadingAngle;
                prevHeadingAngle = headingAngle;
                return (headingAngle - (double)initHeadingAngle + count * 2 * Math.PI) * 180 / Math.PI;
            });
        }
    }
}