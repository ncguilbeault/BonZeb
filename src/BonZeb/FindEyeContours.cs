using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using Bonsai.Vision;
using Bonsai;
using Bonsai.Reactive;

namespace BonZeb
{

    [Description("Calculates the eye angles by finding the angles of the minor axis of each circle enclosing the two binary thresholded regions that lie on the circumference of a circle centered around the centroid.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class FindEyeContours : Transform<Tuple<Point2f[], ConnectedComponentCollection>, ConnectedComponentCollection>
    {

        public FindEyeContours()
        {
            Mode = ContourRetrieval.External;
            Method = ContourApproximation.ChainApproxNone;
            MinArea = 1;
            MaxArea = null;
            MinDistance = null;
            MaxDistance = null;
            DiscardRegionContainingCentroid = false;
            AngleRangeForEyeSearch = null;
        }

        [Description("Specifies the contour retrieval strategy.")]
        public ContourRetrieval Mode { get; set; }

        [Description("The approximation method used to output the contours.")]
        public ContourApproximation Method { get; set; }

        private double minArea;
        [Description("The minimum area for individual contours to be accepted.")]
        public double MinArea { get => minArea; set => minArea = value > 1 && (!maxArea.HasValue || value <= maxArea) ? value : 1; }

        private double? maxArea;
        [Description("The maximum area for individual contours to be accepted.")]
        public double? MaxArea { get => maxArea; set => maxArea = value.HasValue && value >= minArea ? value : null; }

        private double? minDistance;
        [Description("Minimum distance between centroid of eyes and body centroid.")]
        public double? MinDistance { get => minDistance; set => minDistance = value.HasValue && (!maxDistance.HasValue || (value <= maxDistance && value >= 0)) ? value : null; }

        private double? maxDistance;
        [Description("Minimum distance between centroid of eyes and body centroid.")]
        public double? MaxDistance { get => maxDistance; set => maxDistance = value.HasValue && (!minDistance.HasValue || value >= minDistance) ? value : null; }

        [Description("Determines whether the region containing the centroid will be removed.")]
        public bool DiscardRegionContainingCentroid { get; set; }

        private double? angleRangeForEyeSearch;
        [Description("The range of angles in degrees around the expected heading angle for which to search for the eyes.")]
        public double? AngleRangeForEyeSearch { get => angleRangeForEyeSearch; set => angleRangeForEyeSearch = value.HasValue && value >= 0 ? value : null; }

        [Description("Determines whether or not to do a final step of fitting the eyes with ellipses.")]
        public bool FitEllipsesToEyes { get; set; }


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

        private ConnectedComponentCollection FindEyeContoursFunc(ConnectedComponentCollection contours, Point2f[] points, IplDepth depth = IplDepth.U8, int channels = 1)
        {
            if (contours.Count < 2)
            {
                return new ConnectedComponentCollection(new List<ConnectedComponent> { new ConnectedComponent(), new ConnectedComponent() }, contours.ImageSize);
            }

            double headingAngle = Math.Atan2(points[0].Y - points[1].Y, points[0].X - points[1].X);

            List<ConnectedComponent> sortedContours = contours.OrderBy(contour => Math.Abs(Math.Atan2(Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).Y - points[0].Y, Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).X - points[0].X))).ToList();

            if (angleRangeForEyeSearch.HasValue || DiscardRegionContainingCentroid || minDistance.HasValue || maxDistance.HasValue)
            {
                for (int i = sortedContours.Count - 1; i >= 0; i--)
                {
                    if (angleRangeForEyeSearch.HasValue)
                    {
                        Point2f rotatedCentroid = Utilities.RotatePoint(sortedContours[i].Centroid, points[0], -headingAngle);
                        double angleToContour = Math.Atan2(rotatedCentroid.Y - points[0].Y, rotatedCentroid.X - points[0].X);
                        if (angleToContour < -Utilities.ConvertDegreesToRadians(angleRangeForEyeSearch.Value / 2) || angleToContour > Utilities.ConvertDegreesToRadians(angleRangeForEyeSearch.Value / 2))
                        {
                            sortedContours.Remove(sortedContours[i]);
                            continue;
                        }
                    }
                    if (DiscardRegionContainingCentroid && CV.PointPolygonTest(sortedContours[i].Contour, points[0], false) != -1)
                    {
                        sortedContours.Remove(sortedContours[i]);
                        continue;
                    }
                    double dist = Math.Sqrt(Math.Pow(sortedContours[i].Centroid.X - points[0].X, 2) + Math.Pow(sortedContours[i].Centroid.Y - points[0].Y, 2));
                    if (minDistance.HasValue && dist < minDistance)
                    {
                        sortedContours.Remove(sortedContours[i]);
                        continue;
                    }
                    if (maxDistance.HasValue && dist > maxDistance)
                    {
                        sortedContours.Remove(sortedContours[i]);
                    }
                }
            }

            if (sortedContours.Count < 2)
            {
                return new ConnectedComponentCollection(new List<ConnectedComponent> { new ConnectedComponent(), new ConnectedComponent() }, contours.ImageSize);
            }

            List<ConnectedComponent> eyeContours = new List<ConnectedComponent> { sortedContours[0], sortedContours[1] }.OrderBy(contour => Math.Atan2(Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).Y - points[0].Y, Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).X - points[0].X)).ToList();
            
            if (!FitEllipsesToEyes)
            {
                return new ConnectedComponentCollection(eyeContours, contours.ImageSize);
            }

            IplImage ellipseImage = new IplImage(contours.ImageSize, depth, channels);
            ellipseImage.SetZero();

            try
            {
                foreach (ConnectedComponent eyeContour in eyeContours)
                {
                    RotatedRect ellipse = CV.FitEllipse2(eyeContour.Contour);
                    CV.EllipseBox(ellipseImage, ellipse, new Scalar(255, 255, 255, 255), -1);
                }
            }
            catch
            {
                return new ConnectedComponentCollection(new List<ConnectedComponent> { new ConnectedComponent(), new ConnectedComponent() }, contours.ImageSize);
            }

            MemStorage ellipseMemStorage = new MemStorage();
            int contourCount = CV.FindContours(ellipseImage, ellipseMemStorage, out Seq seqEllipseContours);
            Contours ellipseContours = new Contours(seqEllipseContours, ellipseImage.Size);
            Seq currentEllipseContour = ellipseContours.FirstContour;
            ConnectedComponentCollection ellipseConnectedComponents = new ConnectedComponentCollection(ellipseContours.ImageSize);

            while (currentEllipseContour != null)
            {
                ellipseConnectedComponents.Add(ConnectedComponent.FromContour(currentEllipseContour));
                currentEllipseContour = currentEllipseContour.HNext;
            }

            List<ConnectedComponent> ellipses = ellipseConnectedComponents.OrderBy(contour => Math.Atan2(Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).Y - points[0].Y, Utilities.RotatePoint(contour.Centroid, points[0], -headingAngle).X - points[0].X)).ToList();

            if (ellipses.Count < 2)
            {
                return new ConnectedComponentCollection(new List<ConnectedComponent> { new ConnectedComponent(), new ConnectedComponent() }, contours.ImageSize);
            }

            return new ConnectedComponentCollection(ellipses, contours.ImageSize);
        }

        private ConnectedComponentCollection FindEyeContoursFromImageFunc(IplImage image, Point2f[] points)
        {

            IplImage temp = image.Clone();
            MemStorage memStorage = new MemStorage();
            int contourCount = CV.FindContours(temp, memStorage, out Seq seqContours);
            Contours contours = new Contours(seqContours, temp.Size);
            Seq currentContour = contours.FirstContour;
            ConnectedComponentCollection connectedComponents = new ConnectedComponentCollection(contours.ImageSize);
            while (currentContour != null)
            {
                double contourArea = CV.ContourArea(currentContour, SeqSlice.WholeSeq);
                if (maxArea == null)
                {
                    if (contourArea >= minArea)
                    {
                        connectedComponents.Add(ConnectedComponent.FromContour(currentContour));
                    }                  
                }
                else
                {
                    if (contourArea >= minArea && contourArea <= maxArea)
                    {
                        connectedComponents.Add(ConnectedComponent.FromContour(currentContour));
                    }
                }
                currentContour = currentContour.HNext;
            }
            return FindEyeContoursFunc(connectedComponents, points, image.Depth, image.Channels);
        }
    }
}
