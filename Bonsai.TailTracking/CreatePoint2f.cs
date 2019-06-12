using System;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.TailTracking
{

    [Description("Creates a point2f variable.")]
    [WorkflowElementCategory(ElementCategory.Source)]

    public class CreatePoint2f : Source<Point2f>
    {
        [Description("X value to use for the point2f variable.")]
        public float X { get; set; }

        [Description("Y value to use for the point2f variable.")]
        public float Y { get; set; }

        public override IObservable<Point2f> Generate()
        {
            float x = X;
            float y = Y;
            return Observable.Defer(() => Observable.Return(new Point2f(x, y)));
        }
    }
}
