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

        public CalculateEyeAngles()
        {
            Mode = ContourRetrieval.External;
            Method = ContourApproximation.ChainApproxNone;
            MinArea = 1;
            MaxArea = null;
        }

        [Description("Specifies the contour retrieval strategy.")]
        public ContourRetrieval Mode { get; set; }

        [Description("The approximation method used to output the contours.")]
        public ContourApproximation Method { get; set; }

        private double? minArea;
        [Description("The minimum area for individual contours to be accepted.")]
        public double? MinArea { get => minArea; set => minArea = value == null || value > 1 ? value : 1; }

        private double? maxArea;
        [Description("The maximum area for individual contours to be accepted.")]
        public double? MaxArea { get => maxArea; set => maxArea = value == null || value >= minArea ? value : minArea; }

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
        public IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<Point2f[], IplImage>> source)
        {
            prevEyeOrientations = new double[] { 0, 0 };
            return source.Select(value => CalculateEyeAnglesFromImageFunc(value.Item2, value.Item1));
        }
        public IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<IplImage, Point2f[]>> source)
        {
            prevEyeOrientations = new double[] { 0, 0 };
            return source.Select(value => CalculateEyeAnglesFromImageFunc(value.Item1, value.Item2));
        }
        ConnectedComponentCollection CalculateEyeAnglesFunc(ConnectedComponentCollection contours, Point2f[] points)
        {
            if (contours.Count < 2)
            {
                return contours;
            }
            double headingAngle = Math.Atan2(points[0].Y - points[1].Y, points[0].X - points[1].X);
            List<ConnectedComponent> sortedContours = contours.OrderBy(contour => Math.Abs(Math.Atan2(Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).Y - points[0].Y, Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).X - points[0].X))).ToList();
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
        ConnectedComponentCollection CalculateEyeAnglesFromImageFunc(IplImage image, Point2f[] points)
        {
            Seq seqContours;
            MemStorage memStorage = new MemStorage();
            int contourCount = CV.FindContours(image, memStorage, out seqContours);
            Contours contours = new Contours(seqContours, image.Size);
            Seq newCurrentContour = contours.FirstContour;
            ConnectedComponentCollection connectedComponents = new ConnectedComponentCollection(contours.ImageSize);
            while (newCurrentContour != null)
            {
                if (!minArea.HasValue && !maxArea.HasValue)
                {
                    connectedComponents.Add(ConnectedComponent.FromContour(newCurrentContour));
                }
                else
                {
                    double contourArea = CV.ContourArea(newCurrentContour, SeqSlice.WholeSeq);
                    if (minArea.HasValue && !maxArea.HasValue && contourArea >= minArea)
                    {
                        connectedComponents.Add(ConnectedComponent.FromContour(newCurrentContour));
                    }
                    else
                    {
                        if (!minArea.HasValue && maxArea.HasValue && contourArea <= maxArea)
                        {
                            connectedComponents.Add(ConnectedComponent.FromContour(newCurrentContour));
                        }
                        else
                        {
                            if (minArea.HasValue && maxArea.HasValue && contourArea >= minArea && contourArea <= maxArea)
                            {
                                connectedComponents.Add(ConnectedComponent.FromContour(newCurrentContour));
                            }
                        }
                    }
                }
                newCurrentContour = newCurrentContour.HNext;
            }
            return CalculateEyeAnglesFunc(connectedComponents, points);
        }
    }
}
