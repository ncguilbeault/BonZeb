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
        private float x;
        [Description("X value to use for the point2f variable.")]
        public float X { get { return x; } set { x = value; } }

        private float y;
        [Description("Y value to use for the point2f variable.")]
        public float Y { get { return y; } set { y = value; } }

        public override IObservable<Point2f> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point2f(x, y)));
        }
    }
}
