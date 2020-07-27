using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using Bonsai.Vision;
using Bonsai;

namespace BonZeb
{

    [Description("Calculates the continuous heading angle using the first two elements in an array of points.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateHeadingAngle : Transform<Point2f[], double>
    {
        [Description("Determines whether or not to initialize the heading angle to zero.")]
        public bool InitializeHeadingAngleToZero { get; set; }

        private int turnCount;
        private double? prevHeadingAngle;
        private double? initHeadingAngle;

        public override IObservable<double> Process(IObservable<Point2f[]> source)
        {
            turnCount = 0;
            prevHeadingAngle = null;
            initHeadingAngle = null;
            return source.Select(value => CalculateHeadingAngleWithPointsFunc(value[0], value[1]));
        }

        public IObservable<double> Process(IObservable<Tuple<Point2f, Point2f>> source)
        {
            turnCount = 0;
            prevHeadingAngle = null;
            initHeadingAngle = null;
            return source.Select(value =>
            {
                return CalculateHeadingAngleWithPointsFunc(value.Item1, value.Item2);
            });
        }

        public IObservable<double> Process(IObservable<Tuple<ConnectedComponentCollection, Point2f[]>> source)
        {
            turnCount = 0;
            prevHeadingAngle = null;
            initHeadingAngle = null;
            return source.Select(value =>
            {
                if (value.Item1.Count == 2)
                {
                    return CalculateHeadingAngleWithEyesFunc(value.Item1, value.Item2[0]);
                }
                return CalculateHeadingAngleWithPointsFunc(value.Item2[0], value.Item2[1]);
            });
        }

        public IObservable<double> Process(IObservable<Tuple<Point2f[], ConnectedComponentCollection>> source)
        {
            turnCount = 0;
            prevHeadingAngle = null;
            initHeadingAngle = null;
            return source.Select(value =>
            {
                if (value.Item2.Count == 2)
                {
                    return CalculateHeadingAngleWithEyesFunc(value.Item2, value.Item1[0]);
                }
                return CalculateHeadingAngleWithPointsFunc(value.Item1[0], value.Item1[2]);
            });
        }

        private double CalculateHeadingAngleWithPointsFunc(Point2f headingPoint, Point2f centroid)
        {
            double headingAngle = Math.Atan2(headingPoint.Y - centroid.Y, headingPoint.X - centroid.X);
            if (initHeadingAngle == null)
            {
                initHeadingAngle = InitializeHeadingAngleToZero ? headingAngle : 0;
            }
            if (prevHeadingAngle != null)
            {
                if (headingAngle - prevHeadingAngle > Math.PI)
                {
                    turnCount -= 1;
                }
                else if (headingAngle - prevHeadingAngle < -Math.PI)
                {
                    turnCount += 1;
                }
            }
            prevHeadingAngle = headingAngle;
            return headingAngle - (double)initHeadingAngle + (turnCount * Math.PI * 2.0);
        }

        private double CalculateHeadingAngleWithEyesFunc(ConnectedComponentCollection eyes, Point2f centroid)
        {
            Point2f headingPoint = new Point2f((eyes[0].Centroid.X + eyes[1].Centroid.X) / 2, (eyes[0].Centroid.Y + eyes[1].Centroid.Y) / 2);
            return CalculateHeadingAngleWithPointsFunc(headingPoint, centroid);
        }
    }
}