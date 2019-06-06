using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Extracts tail curvature from tail tracking.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class ExtractTailCurvature : Transform<Point2f[], double[]>
    {
        public ExtractTailCurvature()
        {
            prevTailCurvature = new double[0];
        }

        private double[] prevTailCurvature { get; set; }

        public override IObservable<double[]> Process(IObservable<Point2f[]> source)
        {
            return source.Select(value =>
            {
                double rotationAngle = -Math.Atan2(value[1].Y - value[0].Y, value[1].X - value[0].X);
                Point2f[] points = Utilities.RotatePoints(value, value[0], rotationAngle);
                double[] tailCurvature = new double[points.Length - 1];
                for (int i = 0; i < value.Length - 1; i++)
                {
                    tailCurvature[i] = Math.Atan2(points[i + 1].Y - points[i].Y, points[i + 1].X - points[i].X) * 180 / Math.PI;
                    if (prevTailCurvature.Length > 0)
                    {
                        tailCurvature[i] = tailCurvature[i] - prevTailCurvature[i] > Math.PI ? tailCurvature[i] - Math.PI * 2 : tailCurvature[i] - prevTailCurvature[i] < -Math.PI ? tailCurvature[i] + Math.PI * 2 : tailCurvature[i];
                    }
                }
                prevTailCurvature = tailCurvature;
                return tailCurvature;
            });
        }
    }

}
