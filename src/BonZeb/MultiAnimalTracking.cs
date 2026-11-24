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
        [Description("The sampling rate of the input sequence in Hz. Used by the kalman filter to predict the positions of tracked objects.")]
        public double SamplingRate { get => samplingRate; set => samplingRate = value > 0 ? value : 1; }

        private double accelerationStdDev;
        [Description("The standard deviation of the acceleration of tracked objects in pixels per second squared. Larger values make the kalman filter follow the measured centroids more closely.")]
        public double AccelerationStdDev { get => accelerationStdDev; set => accelerationStdDev = value > 0 ? value : 0; }

        private Point2f[] prevCentroids;
        private KalmanFilter[] kalmanFilters;

        public MultiAnimalTracking()
        {
            ThresholdValue = 100;
            ThresholdType = ThresholdTypes.Binary;
            MaxThresholdValue = 255;
            MinArea = 0;
            MaxArea = null;
            NumCentroids = null;
            SamplingRate = 100;
            AccelerationStdDev = 1000;
        }

        public override IObservable<MultiAnimalTrackingData> Process(IObservable<BackgroundSubtractionData> source)
        {
            prevCentroids = null;
            kalmanFilters = null;
            return source.Select(value => 
            {
                return MultiAnimalCentroidsFromImage(value);
            });
        }

        public IObservable<MultiAnimalTrackingData> Process(IObservable<IplImage> source)
        {
            prevCentroids = null;
            kalmanFilters = null;
            return source.Select(value =>
            {
                return MultiAnimalCentroidsFromImage(new BackgroundSubtractionData(value));
            });
        }

        public IObservable<MultiAnimalTrackingData> Process(IObservable<ConnectedComponentCollection> source)
        {
            prevCentroids = null;
            kalmanFilters = null;
            return source.Select(value =>
            {
                return MultiAnimalCentroidsFunc(SelectCentroids(value));
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

            MultiAnimalTrackingData result = MultiAnimalCentroidsFunc(SelectCentroids(connectedComponents));
            return new MultiAnimalTrackingData(input.Image, result.UnorderedCentroids, result.UnorderedIdentities, result.OrderedCentroids, thresh, input.BackgroundSubtractedImage);

        }

        private Point2f[] SelectCentroids(ConnectedComponentCollection connectedComponents)
        {
            // Returns the centroids of the connected components in descending order of area, limited to the number of centroids to track.
            IEnumerable<ConnectedComponent> sortedComponents = connectedComponents.OrderByDescending(contour => contour.Area);
            if (numCentroids.HasValue)
            {
                sortedComponents = sortedComponents.Take(numCentroids.Value);
            }
            return sortedComponents.Select(contour => contour.Centroid).ToArray();
        }

        private MultiAnimalTrackingData MultiAnimalCentroidsFunc(Point2f[] centroids)
        {
            if (prevCentroids == null)
            {
                prevCentroids = new Point2f[0];
            }

            if (centroids.Length == 0)
            {
                return new MultiAnimalTrackingData(centroids, Enumerable.Repeat(-1, prevCentroids.Length).ToArray(), prevCentroids);
            }

            // Identities are matched against the previous centroids, or against the positions predicted by the kalman filters.
            bool useKalmanFilter = IdentityTrackingMethod == IdentityTrackingMethod.KalmanFilter;
            Point2f[] referencePoints = prevCentroids;
            if (useKalmanFilter)
            {
                if (kalmanFilters == null || kalmanFilters.Length != prevCentroids.Length)
                {
                    kalmanFilters = prevCentroids.Select(CreateKalmanFilter).ToArray();
                }
                referencePoints = new Point2f[kalmanFilters.Length];
                for (int i = 0; i < kalmanFilters.Length; i++)
                {
                    kalmanFilters[i].Predict();
                    referencePoints[i] = new Point2f((float)kalmanFilters[i].X, (float)kalmanFilters[i].Y);
                }
            }
            else
            {
                kalmanFilters = null;
            }

            List<int> identities = AssignIdentities(centroids, referencePoints).ToList();

            // Centroids that were not matched to an existing identity are given new identities.
            int maxIdentities = numCentroids ?? int.MaxValue;
            for (int i = 0; i < centroids.Length && identities.Count < maxIdentities; i++)
            {
                if (!identities.Contains(i))
                {
                    identities.Add(i);
                }
            }

            Point2f[] orderedCentroids = identities.Select(index => centroids[index]).ToArray();

            if (useKalmanFilter)
            {
                KalmanFilter[] updatedFilters = new KalmanFilter[orderedCentroids.Length];
                for (int i = 0; i < orderedCentroids.Length; i++)
                {
                    if (i < kalmanFilters.Length)
                    {
                        updatedFilters[i] = kalmanFilters[i];
                        updatedFilters[i].Update(orderedCentroids[i].X, orderedCentroids[i].Y);
                    }
                    else
                    {
                        updatedFilters[i] = CreateKalmanFilter(orderedCentroids[i]);
                    }
                }
                kalmanFilters = updatedFilters;
            }

            prevCentroids = orderedCentroids;
            return new MultiAnimalTrackingData(centroids, identities.ToArray(), orderedCentroids);
        }

        private KalmanFilter CreateKalmanFilter(Point2f position)
        {
            return new KalmanFilter(samplingRate, 0, accelerationStdDev) { X = position.X, Y = position.Y };
        }

        private static int[] AssignIdentities(Point2f[] centroids, Point2f[] referencePoints)
        {
            // Returns the index of the centroid assigned to each reference point, minimizing the total distance between them.
            int[] assignment = new int[referencePoints.Length];
            if (referencePoints.Length == 0)
            {
                return assignment;
            }

            double[,] cost = Hungarian.cDist(centroids, referencePoints);
            int[] rowAssignment = Hungarian.LinearSumAssignment(cost)[1];
            if (centroids.Length >= referencePoints.Length)
            {
                return rowAssignment;
            }

            for (int i = 0; i < assignment.Length; i++)
            {
                assignment[i] = -1;
            }
            for (int i = 0; i < rowAssignment.Length; i++)
            {
                assignment[rowAssignment[i]] = i;
            }

            // Reference points left without a centroid, such as when two animals merge into one contour, take the nearest centroid.
            for (int i = 0; i < assignment.Length; i++)
            {
                if (assignment[i] == -1)
                {
                    int nearest = 0;
                    for (int j = 1; j < centroids.Length; j++)
                    {
                        if (cost[j, i] < cost[nearest, i])
                        {
                            nearest = j;
                        }
                    }
                    assignment[i] = nearest;
                }
            }
            return assignment;
        }
    }
}
