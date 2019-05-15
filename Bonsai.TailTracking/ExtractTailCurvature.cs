using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Extracts tail curvature from tail tracking.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class ExtractTailCurvature : Transform<Point[], double[]>
    {
        public ExtractTailCurvature()
        {
            ExtractAllSegments = true;
            LocationOfTailCurvature = Utilities.LocationOfTailCurvature.EndOfTail;
        }

        [Description("Determines whether to extract curvature along all segments of the tail or whether to extract the curvature only from a specific location.")]
        public bool ExtractAllSegments { get; set; }

        [Description("Location to extract tail curvature data. Can be near the start of the tail, the middle of the tail, or the end of the tail.")]
        public Utilities.LocationOfTailCurvature LocationOfTailCurvature { get; set; }

        public override IObservable<double[]> Process(IObservable<Point[]> source)
        {

            return source.Select(value => 
            {

                Point[] tailPoints = value;
                int numTailPoints = tailPoints.Length;
                bool extractAllSegments = ExtractAllSegments;
                double[] tailCurvature = new double[0];
                if (!extractAllSegments)
                {
                    Utilities.LocationOfTailCurvature locationOfTailCurvature = LocationOfTailCurvature;
                    int tailPoint = locationOfTailCurvature == Utilities.LocationOfTailCurvature.StartOfTail ? 1 : locationOfTailCurvature == Utilities.LocationOfTailCurvature.MiddleOfTail ? (int)Math.Floor((double)(numTailPoints / 2)) : numTailPoints - 2;
                    tailCurvature = new double[1];
                    tailCurvature[0] = (Math.Atan2(tailPoints[tailPoint + 1].X - tailPoints[tailPoint].X, tailPoints[tailPoint + 1].Y - tailPoints[tailPoint].Y) - Math.Atan2(tailPoints[1].X - tailPoints[0].X, tailPoints[1].Y - tailPoints[0].Y)) * 180 / Math.PI;
                }
                else
                {
                    tailCurvature = new double[numTailPoints - 1];
                    for (int i = 0; i < numTailPoints - 1; i++)
                    {
                        tailCurvature[i] = (Math.Atan2(tailPoints[i + 1].X - tailPoints[i].X, tailPoints[i + 1].Y - tailPoints[i].Y) - Math.Atan2(tailPoints[1].X - tailPoints[0].X, tailPoints[1].Y - tailPoints[0].Y)) * 180 / Math.PI;
                    }
                }
                return tailCurvature;
            });

        }
        //public IObservable<double[]> Process(IObservable<Point2f[]> source)
        //{
        //    double[] prevTailCurvature = new double[0];
        //    return source.Select(value =>
        //    {
        //        Point2f[] points = new Point2f[value.Length];
        //        double rotationAngle = -Math.Atan2(value[1].Y - value[0].Y, value[1].X - value[0].X);
        //        for (int i = 0; i < points.Length; i++)
        //        {
        //            points[i] = Utilities.RotatePoint(value[i], value[0], rotationAngle);
        //        }
        //        double[] tailCurvature = new double[points.Length - 1];
        //        for (int i = 0; i < value.Length - 1; i++)
        //        {
        //            tailCurvature[i] = Math.Atan2(points[i + 1].Y - points[i].Y, points[i + 1].X - points[i].X) * 180 / Math.PI;
        //            if (prevTailCurvature.Length > 0)
        //            {
        //                tailCurvature[i] = tailCurvature[i] - prevTailCurvature[i] > Math.PI ? tailCurvature[i] - Math.PI * 2 : tailCurvature[i] - prevTailCurvature[i]  < -Math.PI ? tailCurvature[i] + Math.PI * 2 : tailCurvature[i];
        //            }
        //        }
        //        prevTailCurvature = tailCurvature;
        //        return tailCurvature;
        //    });
        //}
        public IObservable<double> Process(IObservable<Point2f[]> source)
        {
            double? prevTailCurvature = null;
            return source.Select(value =>
            {
                double rotationAngle = -Math.Atan2(value[1].Y - value[0].Y, value[1].X - value[0].X);
                Point2f[] points = Utilities.RotatePoints(value, value[0], rotationAngle);
                double tailCurvature = 0;
                for (int i = value.Length - 4; i < value.Length - 1; i++)
                {
                    tailCurvature = points[i + 1].Equals(points[i]) ? tailCurvature : tailCurvature + Math.Atan2(points[i + 1].Y - points[i].Y, points[i + 1].X - points[i].X) * 180 / Math.PI;
                }
                tailCurvature /= 3;
                if (prevTailCurvature != null)
                {
                    tailCurvature = tailCurvature - prevTailCurvature > Math.PI ? tailCurvature - Math.PI * 2 : tailCurvature - prevTailCurvature < -Math.PI ? tailCurvature + Math.PI * 2 : tailCurvature;
                }
                prevTailCurvature = tailCurvature;
                return tailCurvature;
            });
        }
    }

}
