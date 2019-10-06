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

    public class FindEyeContours : Transform<Tuple<Point2f[], ConnectedComponentCollection>, ConnectedComponentCollection>
    {

        public FindEyeContours()
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

        public override IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<Point2f[], ConnectedComponentCollection>> source)
        {
            return source.Select(value => FindEyeContoursFunc(value.Item2, value.Item1));
        }

        public IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<ConnectedComponentCollection, Point2f[]>> source)
        {
            return source.Select(value => FindEyeContoursFunc(value.Item1, value.Item2));
        }

        public IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<Point2f[], IplImage>> source)
        {
            return source.Select(value => FindEyeContoursFromImageFunc(value.Item2, value.Item1));
        }

        public IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<IplImage, Point2f[]>> source)
        {
            return source.Select(value => FindEyeContoursFromImageFunc(value.Item1, value.Item2));
        }

        private ConnectedComponentCollection FindEyeContoursFunc(ConnectedComponentCollection contours, Point2f[] points)
        {
            if (contours.Count < 2)
            {
                return contours;
            }
            ConnectedComponent[] tempContours = null;
            contours.CopyTo(tempContours, 0);
            double headingAngle = Math.Atan2(points[0].Y - points[1].Y, points[0].X - points[1].X);
            List<ConnectedComponent> sortedContours = tempContours.OrderBy(contour => Math.Abs(Math.Atan2(Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).Y - points[0].Y, Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).X - points[0].X))).ToList();
            List<ConnectedComponent> eyeContours = new List<ConnectedComponent> { sortedContours[0], sortedContours[1] }.OrderBy(contour => Math.Atan2(Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).Y - points[0].Y, Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).X - points[0].X)).ToList();
            return new ConnectedComponentCollection(eyeContours, contours.ImageSize);
        }

        private ConnectedComponentCollection FindEyeContoursFromImageFunc(IplImage image, Point2f[] points)
        {
            MemStorage memStorage = new MemStorage();
            int contourCount = CV.FindContours(image, memStorage, out Seq seqContours);
            Contours contours = new Contours(seqContours, image.Size);
            Seq currentContour = contours.FirstContour;
            ConnectedComponentCollection connectedComponents = new ConnectedComponentCollection(contours.ImageSize);
            while (currentContour != null)
            {
                if (!minArea.HasValue && !maxArea.HasValue)
                {
                    connectedComponents.Add(ConnectedComponent.FromContour(currentContour));
                }
                else
                {
                    double contourArea = CV.ContourArea(currentContour, SeqSlice.WholeSeq);
                    if (minArea.HasValue && !maxArea.HasValue && contourArea >= minArea)
                    {
                        connectedComponents.Add(ConnectedComponent.FromContour(currentContour));
                    }
                    else
                    {
                        if (!minArea.HasValue && maxArea.HasValue && contourArea <= maxArea)
                        {
                            connectedComponents.Add(ConnectedComponent.FromContour(currentContour));
                        }
                        else
                        {
                            if (minArea.HasValue && maxArea.HasValue && contourArea >= minArea && contourArea <= maxArea)
                            {
                                connectedComponents.Add(ConnectedComponent.FromContour(currentContour));
                            }
                        }
                    }
                }
                currentContour = currentContour.HNext;
            }
            return FindEyeContoursFunc(connectedComponents, points);
        }
    }
}
