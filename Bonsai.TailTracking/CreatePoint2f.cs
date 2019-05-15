using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class CreatePoint2f : Source<Point2f>
    {
        public float X { get; set; }
        public float Y { get; set; }

        public override IObservable<Point2f> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point2f(X, Y)));
        }
    }
}
