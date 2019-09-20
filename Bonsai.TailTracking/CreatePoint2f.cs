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

        private Point2f point = new Point2f();
        event Action<Point2f> ValueChanged;

        void OnValueChanged(Point2f value)
        {
            ValueChanged?.Invoke(value);
        }

        [Description("X value to use for the point2f variable.")]
        public float X { get => point.X; set { point.X = value; OnValueChanged(point); } }

        [Description("Y value to use for the point2f variable.")]
        public float Y { get => point.Y; set { point.Y = value; OnValueChanged(point); } }

        public override IObservable<Point2f> Generate()
        {
            return Observable.Defer(() => Observable.Return(point)).Concat(Observable.FromEvent<Point2f>(handler => ValueChanged += handler, handler => ValueChanged -= handler));
        }

        public IObservable<Point2f> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => point);
        }
    }
}
