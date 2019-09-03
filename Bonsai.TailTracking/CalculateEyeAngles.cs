using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using Bonsai.Vision;

namespace Bonsai.TailTracking
{

    [Description("Calculates the eye angles by finding the angles of the minor axis of each circle enclosing the two largest binary thresholded regions that lie on the circumference of a circle centered around the centroid.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateEyeAngles : Transform<Tuple<Point2f[], ConnectedComponentCollection>, ConnectedComponentCollection>
    {
        private int distEyes;
        [Description("Distance between the eyes and the centroid.")]
        public int DistEyes { get { return distEyes; } set { distEyes = value > 0 ? value : 0; } }

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
                List<ConnectedComponent> eyeContours = new List<ConnectedComponent> { sortedContours[0], sortedContours[1] }.OrderBy(contour => headingAngle - Math.Atan2(contour.Centroid.Y - value.Item1[0].Y, contour.Centroid.X - value.Item1[0].X)).ToList();
                //if (eyeContours[0].Orientation - eyeContours[1].Orientation > )
                headingAngle = headingAngle * 180 / Math.PI;
                for (int i = 0; i < 2; i++)
                {
                    if (eyeContours[i].Contour.Count >= 5)
                    {
                        //double eyeOrientation = 180 - Math.Abs(Math.Abs(CV.FitEllipse2(eyeContours[i].Contour).Angle - headingAngle) - 180);
                        double eyeOrientation = 180 - Math.Abs(Math.Abs((eyeContours[i].Orientation * 180 / Math.PI) - headingAngle) - 180);
                        //eyeContours[i].
                        eyeOrientation = eyeOrientation - prevEyeOrientations[i] > 90 ? eyeOrientation + 180 : eyeOrientation - prevEyeOrientations[i] < -90 ? eyeOrientation - 180 : eyeOrientation;
                        //Console.WriteLine("Eye Orientation: " + eyeOrientation);
                        //Console.WriteLine("Heading Angle: " + headingAngle);
                        //eyeContours[i].Orientation = eyeOrientation - headingAngle >= -90 && eyeOrientation - headingAngle <= 90 ? eyeOrientation - headingAngle : eyeOrientation - headingAngle < -90 ? eyeOrientation - headingAngle - 360 : eyeOrientation - headingAngle + 360;
                        eyeContours[i].Orientation = eyeOrientation;
                        prevEyeOrientations[i] = eyeOrientation;
                    }
                    //Point2f[] newEyeContours = { new Point2f(eyeContours[i].Contour.Rect.X, eyeContours[i].Contour.Rect.Y), new Point2f(eyeContours[i].Contour.Rect.X + eyeContours[i].Contour.Rect.Width, eyeContours[i].Contour.Rect.Y), new Point2f(eyeContours[i].Contour.Rect.X, eyeContours[i].Contour.Rect.Y + eyeContours[i].Contour.Rect.Height), new Point2f(eyeContours[i].Contour.Rect.X + eyeContours[i].Contour.Rect.Width, eyeContours[i].Contour.Rect.Y + eyeContours[i].Contour.Rect.Height) };
                    //Point2f[] rotatedEyeContours = Utilities.RotatePoints(newEyeContours, value.Item1[0], -headingAngle);
                    //eyeContours[i].Orientation = eyeContours[i].Orientation - prevEyeOrientations[i] > Math.PI * 2 / 3 ? eyeContours[i].Orientation - Math.PI : eyeContours[i].Orientation - prevEyeOrientations[i] < -Math.PI * 2 / 3 ? eyeContours[i].Orientation + Math.PI : eyeContours[i].Orientation;
                    //eyeContours[i].Orientation = eyeContours[i].Contour.Count >= 5 && headingAngle - CV.FitEllipse2(eyeContours[i].Contour).Angle > -90 && headingAngle - CV.FitEllipse2(eyeContours[i].Contour).Angle < 90 ? CV.FitEllipse2(eyeContours[i].Contour).Angle : CV.FitEllipse2(eyeContours[i].Contour).Angle;
                    //eyeContours[i].Orientation = eyeContours[i].Orientation 
                    //prevEyeOrientations[i] = eyeContours[i].Orientation;
                }
                
                return new ConnectedComponentCollection(eyeContours, value.Item2.ImageSize);
            });
        }
    }
}
