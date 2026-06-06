using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;
using Bonsai.Vision;
using Bonsai;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace BonZeb
{

    [Description("Calculates the centroid of a binarized image using the first-order raw image moments.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class MultiAnimalTracking : Transform<BackgroundSubtractionData, MultiAnimalTrackingData>
    {
        [Description("Threshold value to use for comparing pixel values.")]
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [Description("The type of threshold to apply to pixels.")]
        public ThresholdTypes ThresholdType { get; set; }

        private double maxValue;
        [Description("The value to set the pixels above the threshold value.")]
        public double MaxThresholdValue { get => maxValue; set => maxValue = value < 0 ? 0 : value > 255 ? 255 : value; }

        private double minArea;
        [Description("The minimum area for individual contours to be accepted.")]
        public double MinArea { get => minArea; set => minArea = value > 1 && (!maxArea.HasValue || value <= maxArea) ? value : 1; }

        private double? maxArea;
        [Description("The maximum area for individual contours to be accepted.")]
        public double? MaxArea { get => maxArea; set => maxArea = value.HasValue && value >= minArea ? value : null; }

        private int? numCentroids;
        [Description("The number of centroids to track.")]
        public int? NumCentroids { get => numCentroids; set => numCentroids = value.HasValue && value > 0 ? value : null; }

        [Description("The method to use to assign identities to tracked objects over time.")]
        public IdentityTrackingMethod IdentityTrackingMethod { get; set; }

        private double samplingRate;
        [Description("The kalman filter sampling rate.")]
        public double SamplingRate { get => samplingRate; set => samplingRate = value; }

        private Point2f[] prevCentroids;
        private Point2f[] prevCentroids2;
        private KalmanFilter[] kalmanFilters;

        public MultiAnimalTracking()
        {
            ThresholdValue = 100;
            ThresholdType = ThresholdTypes.Binary;
            MaxThresholdValue = 255;
            MinArea = 0;
            MaxArea = null;
            NumCentroids = null;
        }

        public override IObservable<MultiAnimalTrackingData> Process(IObservable<BackgroundSubtractionData> source)
        {
            prevCentroids = null;
            return source.Select(value => 
            {
                return MultiAnimalCentroidsFromImage(value);
            });
        }

        public IObservable<MultiAnimalTrackingData> Process(IObservable<IplImage> source)
        {
            prevCentroids = null;
            return source.Select(value =>
            {
                return MultiAnimalCentroidsFromImage(new BackgroundSubtractionData(value));
            });
        }

        public IObservable<MultiAnimalTrackingData> Process(IObservable<ConnectedComponentCollection> source)
        {
            prevCentroids = null;
            return source.Select(value =>
            {
                ConnectedComponentCollection connectedComponents = value;
                Point2f[] centroids = numCentroids.HasValue ? new Point2f[numCentroids.Value] : new Point2f[connectedComponents.Count];
                if (connectedComponents.Count != 0)
                {
                    List<ConnectedComponent> sortedComponents = connectedComponents.OrderByDescending(contour => contour.Area).ToList();
                    for (int i = 0; i < connectedComponents.Count; i++)
                    {
                        centroids[i] = sortedComponents[i].Centroid;
                    }
                }

                return MultiAnimalCentroidsFunc(centroids);
            });
        }

        private MultiAnimalTrackingData MultiAnimalCentroidsFromImage(BackgroundSubtractionData input)
        {
            IplImage image = new IplImage(input.Image.Size, input.Image.Depth, 1);

            if (input.Image.Channels != 1)
            {
                CV.CvtColor(input.Image, image, ColorConversion.Bgr2Gray);
            }
            else
            {
                image = input.Image.Clone();
            }

            IplImage backgroundSubtractedImage = input.BackgroundSubtractedImage;
            IplImage thresh = new IplImage(image.Size, image.Depth, image.Channels);

            if (backgroundSubtractedImage == null)
            {
                CV.Threshold(image, thresh, ThresholdValue, maxValue, ThresholdType);
            }
            else
            {
                CV.Threshold(backgroundSubtractedImage, thresh, ThresholdValue, maxValue, ThresholdType);
            }

            Moments moments = new Moments(thresh, true);

            IplImage temp = thresh.Clone();
            MemStorage memStorage = new MemStorage();
            CV.FindContours(temp, memStorage, out Seq seqContours);
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

            Point2f[] centroids = numCentroids.HasValue ? new Point2f[numCentroids.Value] : new Point2f[connectedComponents.Count];

            if (connectedComponents.Count != 0)
            {
                List<ConnectedComponent> sortedComponents = connectedComponents.OrderByDescending(contour => contour.Area).ToList();
                for (int i = 0; i < connectedComponents.Count; i++)
                {
                    centroids[i] = sortedComponents[i].Centroid;
                }
            }

            if (centroids.Length == 0)
            {
                return new MultiAnimalTrackingData(input.Image, thresh, input.BackgroundSubtractedImage);
            }
            MultiAnimalTrackingData result = MultiAnimalCentroidsFunc(centroids);
            return new MultiAnimalTrackingData(input.Image, result.UnorderedCentroids, result.UnorderedIdentities, result.OrderedCentroids, thresh, input.BackgroundSubtractedImage);

        }

        private MultiAnimalTrackingData MultiAnimalCentroidsFunc(Point2f[] centroids)
        { 

            List<Point2f> updatedCentroids = new List<Point2f>();
            List<int[]> result = new List<int[]>();
            List<Point2f> tempCentroids = new List<Point2f>();

            if (IdentityTrackingMethod == IdentityTrackingMethod.Hungarian)
            {
                (updatedCentroids, result) = ReorderCentroidsForIdentitiesUsingHungarian(centroids.ToList());
                for (int i = 0; i < result[1].Length; i++)
                {
                    if (result[1][i] != -1)
                    {
                        tempCentroids.Add(updatedCentroids[result[1][i]]);
                    }
                }
            }
            else
            {
                (updatedCentroids, result) = ReorderCentroidsForIdentitiesUsingKalmanFilter(centroids.ToList());
                for (int i = 0; i < result[1].Length; i++)
                {
                    if (result[1][i] != -1)
                    {
                        tempCentroids.Add(updatedCentroids[result[1][i]]);
                    }
                }
            }

            Point2f[] orderedCentroids = tempCentroids.ToArray();
            prevCentroids = orderedCentroids;
            return new MultiAnimalTrackingData(centroids, result[1], orderedCentroids);

        }

        private Tuple<List<Point2f>, List<int[]>> ReorderCentroidsForIdentitiesUsingHungarian(List<Point2f> centroids)
        {
            if (prevCentroids == null)
            {
                prevCentroids = new Point2f[] { new Point2f(0, 0) };
            }
            double[,] cost = Hungarian.cDist(centroids.ToArray(), prevCentroids);
            List<int[]> result = Hungarian.LinearSumAssignment(cost);
            int[] rowIndices = result[0];
            int[] colIndices = result[1];
            List<Point2f> updatedCentroids = new List<Point2f>();
            for (int i = 0; i < centroids.Count; i++)
            {
                updatedCentroids.Add(centroids[i]);
            }
            if (prevCentroids.Length > centroids.Count)
            {
                int missedIndex = 0;
                for (int i = 0; i < colIndices.Length; i++)
                {
                    if (colIndices.Contains(i) == false)
                    {
                        missedIndex = i;
                    }
                }
                double minCost = cost[0, missedIndex];
                int mergedIndex = 0;
                for (int i = 1; i < cost.GetLength(0); i++)
                {
                    if (minCost > cost[i, missedIndex])
                    {
                        minCost = cost[i, missedIndex];
                        mergedIndex = i;
                    }
                }
                updatedCentroids.Add(centroids[mergedIndex]);
                (updatedCentroids, result) = ReorderCentroidsForIdentitiesUsingHungarian(updatedCentroids);
            }
            return Tuple.Create(updatedCentroids, result);
        }

        private Tuple<List<Point2f>, List<int[]>> ReorderCentroidsForIdentitiesUsingKalmanFilter(List<Point2f> centroids)
        {
            if (prevCentroids == null)
            {
                prevCentroids = new Point2f[] { new Point2f(0, 0) };
            }
            for (int i = 0; i < centroids.Count; i++)
            {
                KalmanFilter km = new KalmanFilter();
                kalmanFilters.Append(km);
            }    
            List<int[]> result = Hungarian.LinearSumAssignment(cost);
            int[] rowIndices = result[0];
            int[] colIndices = result[1];
            List<Point2f> updatedCentroids = new List<Point2f>();
            for (int i = 0; i < centroids.Count; i++)
            {
                updatedCentroids.Add(centroids[i]);
            }
            if (prevCentroids.Length > centroids.Count)
            {
                int missedIndex = 0;
                for (int i = 0; i < colIndices.Length; i++)
                {
                    if (colIndices.Contains(i) == false)
                    {
                        missedIndex = i;
                    }
                }
                double minCost = cost[0, missedIndex];
                int mergedIndex = 0;
                for (int i = 1; i < cost.GetLength(0); i++)
                {
                    if (minCost > cost[i, missedIndex])
                    {
                        minCost = cost[i, missedIndex];
                        mergedIndex = i;
                    }
                }
                updatedCentroids.Add(centroids[mergedIndex]);
                (updatedCentroids, result) = ReorderCentroidsForIdentitiesUsingHungarian(updatedCentroids);
            }
            return Tuple.Create(updatedCentroids, result);
        }
    }
}
