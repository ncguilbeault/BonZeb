using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using Bonsai.Vision;

namespace Bonsai.TailTracking
{

    [Description("Calculates the eye angles by finding the angles of the minor axis of each circle enclosing the two largest binary thresholded regions that lie on the circumference of a circle centered around the centroid.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateEyeAngles : Transform<Tuple<Point2f[], ConnectedComponentCollection>, ConnectedComponentCollection>
    {
        private int distEyes;
        [Description("Distance between the eyes and the centroid.")]
        public int DistEyes { get => distEyes; set => distEyes = value > 0 ? value : 0; }

        private double[] prevEyeOrientations = { 0, 0 };

        public override IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<Point2f[], ConnectedComponentCollection>> source)
        {
            prevEyeOrientations = new double[] { 0, 0 };
            return source.Select(value => CalculateEyeAnglesFunc(value.Item2, value.Item1));
        }
        public IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<ConnectedComponentCollection, Point2f[]>> source)
        {
            prevEyeOrientations = new double[] { 0, 0 };
            return source.Select(value => CalculateEyeAnglesFunc(value.Item1, value.Item2));
        }
        ConnectedComponentCollection CalculateEyeAnglesFunc(ConnectedComponentCollection contours, Point2f[] points)
        {
            if (contours.Count < 2)
            {
                return contours;
            }
            double headingAngle = Math.Atan2(points[0].Y - points[1].Y, points[0].X - points[1].X);
            List<ConnectedComponent> sortedContours = contours.OrderBy(contour => Math.Abs(headingAngle - Math.Atan2(contour.Centroid.Y - points[0].Y, contour.Centroid.X - points[0].X))).ThenBy(contour => Math.Abs(distEyes - (Math.Pow(contour.Centroid.X - points[0].X, 2) + Math.Pow(contour.Centroid.Y - points[0].Y, 2)))).ThenBy(contour => Math.Atan2(contour.Centroid.Y - points[0].Y, contour.Centroid.X - points[0].X)).ToList();
            List<ConnectedComponent> eyeContours = new List<ConnectedComponent> { sortedContours[0], sortedContours[1] }.OrderBy(contour => Math.Atan2(contour.Centroid.Y - points[0].Y, contour.Centroid.X - points[0].X) - headingAngle).ToList();
            for (int i = 0; i < 2; i++)
            {
                double eyeOrientation = eyeContours[i].Orientation;
                eyeOrientation = eyeOrientation - prevEyeOrientations[i] >= 0.8 * Math.PI ? eyeOrientation - Math.PI : eyeOrientation - prevEyeOrientations[i] <= -0.8 * Math.PI ? eyeOrientation + Math.PI : eyeOrientation;
                eyeContours[i].Orientation = eyeOrientation;
                prevEyeOrientations[i] = eyeOrientation;
            }

            return new ConnectedComponentCollection(eyeContours, contours.ImageSize);
        }
    }
}
