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

        public override IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<Point2f[], ConnectedComponentCollection>> source)
        {
            double[] prevEyeOrientations = { 0, 0 };
            return source.Select(value =>
            {
                if (value.Item2.Count < 2)
                {
                    return value.Item2;
                }
                double headingAngle = Math.Atan2(value.Item1[0].Y - value.Item1[1].Y, value.Item1[0].X - value.Item1[1].X);
                List<ConnectedComponent> sortedContours = value.Item2.OrderBy(contour => Math.Abs(headingAngle - Math.Atan2(contour.Centroid.Y - value.Item1[0].Y, contour.Centroid.X - value.Item1[0].X))).ThenBy(contour => Math.Abs(distEyes - (Math.Pow(contour.Centroid.X - value.Item1[0].X, 2) + Math.Pow(contour.Centroid.Y - value.Item1[0].Y, 2)))).ThenBy(contour => Math.Atan2(contour.Centroid.Y - value.Item1[0].Y, contour.Centroid.X - value.Item1[0].X)).ToList();
                List<ConnectedComponent> eyeContours = new List<ConnectedComponent> { sortedContours[0], sortedContours[1] }.OrderBy(contour => Math.Atan2(contour.Centroid.Y - value.Item1[0].Y, contour.Centroid.X - value.Item1[0].X) - headingAngle).ToList();
                for (int i = 0; i < 2; i++)
                {
                    double eyeOrientation = eyeContours[i].Orientation;
                    eyeOrientation = eyeOrientation - prevEyeOrientations[i] >= 0.8 * Math.PI ? eyeOrientation - Math.PI : eyeOrientation - prevEyeOrientations[i] <= - 0.8 * Math.PI ? eyeOrientation + Math.PI : eyeOrientation;
                    eyeContours[i].Orientation = eyeOrientation;
                    prevEyeOrientations[i] = eyeOrientation;
                }
                
                return new ConnectedComponentCollection(eyeContours, value.Item2.ImageSize);
            });
        }
    }
}
