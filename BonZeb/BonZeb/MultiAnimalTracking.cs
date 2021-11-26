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

        private Point2f[] prevCentroids;

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
                //prevCentroids = null;
                return MultiAnimalCentroidsFunc(value);
            });
        }

        public IObservable<MultiAnimalTrackingData> Process(IObservable<IplImage> source)
        {
            prevCentroids = null;
            return source.Select(value =>
            {
                //prevCentroids = null;
                return MultiAnimalCentroidsFunc(new BackgroundSubtractionData(value));
            });
        }

        private MultiAnimalTrackingData MultiAnimalCentroidsFunc(BackgroundSubtractionData input)
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
                //Console.WriteLine("Centroids not found");
                return new MultiAnimalTrackingData(input.Image, thresh, input.BackgroundSubtractedImage);
            }

            List<Point2f> updatedCentroids = new List<Point2f>();
            List<int[]> result = new List<int[]>();
            (updatedCentroids, result) = ReorderCentroidsForIdentities(centroids.ToList());

            //Point2f[] orderedCentroids = new Point2f[updatedCentroids.Count];
            List<Point2f> tempCentroids = new List<Point2f>();
            for (int i = 0; i < result[1].Length; i++)
            {
                if (result[1][i] != -1)
                {
                    tempCentroids.Add(updatedCentroids[result[1][i]]);
                }
            }
            //Point2f[] orderedCentroids = new Point2f[tempCentroids.Count];
            Point2f[] orderedCentroids = tempCentroids.ToArray();
            string centroidsString = "";
            string updatedCentroidsString = "";
            string orderedCentroidsString = "";
            string prevCentroidsString = "";
            for (int i = 0; i < prevCentroids.Length; i++)
            {
                prevCentroidsString += $"{prevCentroids[i]},";
            }
            for (int i = 0; i < centroids.Length; i++)
            {
                centroidsString += $"{centroids[i]},";
            }
            for (int i = 0; i < updatedCentroids.Count; i++)
            {
                updatedCentroidsString += $"{updatedCentroids[i]},";
            }
            for (int i = 0; i < orderedCentroids.Length; i++)
            {
                orderedCentroidsString += $"{orderedCentroids[i]},";
            }
            Console.WriteLine($"Prev centroids: {prevCentroidsString}");
            Console.WriteLine($"Centroids: {centroidsString}");
            Console.WriteLine($"Updated centroids: {updatedCentroidsString}");
            Console.WriteLine($"Ordered centroids: {orderedCentroidsString}");
            prevCentroids = orderedCentroids;
            return new MultiAnimalTrackingData(input.Image, centroids, result[1], orderedCentroids, thresh, input.BackgroundSubtractedImage);

            //}
            //catch
            //{
            //    return new MultiAnimalTrackingData(input.Image, thresh, input.BackgroundSubtractedImage);
            //}

        }

        private Tuple<List<Point2f>, List<int[]>> ReorderCentroidsForIdentities(List<Point2f> centroids)
        {
            Console.WriteLine($"Centroids: {centroids.Count}");
            double[,] cost = prevCentroids != null ? cDist(centroids.ToArray(), prevCentroids) : cDist(centroids.ToArray());
            if (prevCentroids == null)
            {
                prevCentroids = new Point2f[] { new Point2f(0, 0) };
            }
            Console.WriteLine($"Cost: {cost.GetLength(0)}, {cost.GetLength(1)}");
            List<int[]> result = LinearSumAssignment(cost);
            Console.WriteLine($"Linear Assignment: {result.Count}, {result[0].Length}");
            int[] rowIndices = result[0];
            int[] colIndices = result[1];
            string rowIndicesString = "";
            string colIndicesString = "";
            for (int i = 0; i < rowIndices.Length; i++)
            {
                rowIndicesString += $"{rowIndices[i]},";
            }
            for (int i = 0; i < colIndices.Length; i++)
            {
                colIndicesString += $"{colIndices[i]},";
            }
            Console.WriteLine($"Row indices: {rowIndicesString}");
            Console.WriteLine($"Col indices: {colIndicesString}");
            List<Point2f> updatedCentroids = new List<Point2f>();
            for (int i = 0; i < centroids.Count; i++)
            {
                updatedCentroids.Add(centroids[i]);
            }
            if (prevCentroids.Length > centroids.Count)
            {
                Console.WriteLine("here1");
                int missedIndex = 0;
                for (int i = 0; i < colIndices.Length; i++)
                {
                    if (colIndices.Contains(i) == false)
                    {
                        missedIndex = i;
                    }
                }
                Console.WriteLine($"her2 {missedIndex}");
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
                (updatedCentroids, result) = ReorderCentroidsForIdentities(updatedCentroids);
            }
            return Tuple.Create(updatedCentroids, result);
        }

            private double distance(Point2f A, Point2f B)
        {
            return Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));
        }

        private double[,] cDist(Point2f[] XA, Point2f[] XB)
        {

            /* Parameters
				XA: array_like
					An mA by n array of mA original observations in an n-dimensional space. Inputs are converted to double type.
				XB: array_like
					An mB by n array of mB original observations in an n-dimensional space. Inputs are converted to double type.
			Returns
				Y: ndarray
				A mA by mB distance matrix is returned. For each i and j, the metric dist(u=XA[i], v=XB[j]) is computed and stored 
				in the ij th entry.
			*/
            int mA = XA.Length, mB = XB.Length;

            double[,] result = new double[mA, mB];

            for (int i = 0; i < mA; i++)
            {
                for (int j = 0; j < mB; j++)
                {
                    result[i, j] = distance(XA[i], XB[j]);
                }
            }
            //Transpose(ref result);
            return result;
        }

        private double[,] cDist(Point2f[] XA)
        {
            Point2f[] XB = new Point2f[XA.Length];
            for (int i = 0; i < XA.Length; i++)
{
                XB[i] = new Point2f(0, 0);
            }
            return cDist(XA, XB);
        }

        private static List<int[]> LinearSumAssignment(double[,] cost)
        {
            /*
				Parameters
				----------
				cost_matrix : array
					The cost matrix of the bipartite graph.
				Returns
				-------
				row_ind, col_ind : array
					An array of row indices and one of corresponding column indices giving
					the optimal assignment. The cost of the assignment can be computed
					as ``cost_matrix[row_ind, col_ind].sum()``. The row indices will be
					sorted; in the case of a square cost matrix they will be equal to
					``numpy.arange(cost_matrix.shape[0])``.
			*/

            var nr = cost.GetLength(0);
            var nc = cost.GetLength(1);

            if (nr >= nc)
            {
                var tmpCost = new double[nc, nr];
                for (var i = 0; i < nc; i++)
                    for (var j = 0; j < nr; j++)
                        tmpCost[i, j] = cost[j, i];
                cost = tmpCost;
                nr = cost.GetLength(0);
                nc = cost.GetLength(1);
            }

            // Initialize working arrays
            var u = new double[nr];
            var v = new double[nc];
            var shortestPathCosts = new double[nc];
            var path = new int[nc];
            var x = new int[nr];
            var y = new int[nc];
            var sr = new bool[nr];
            var sc = new bool[nc];

            for (int i = 0; i < nc; i++)
            {
                path[i] = -1;
                y[i] = -1;
            }

            for (int i = 0; i < nr; i++)
            {
                x[i] = -1;
            }

            // Find a matching one vertex at a time
            for (var curRow = 0; curRow < nr; curRow++)
            {
                double minVal = 0;
                var i = curRow;
                // Reset working arrays
                var remaining = new int[nc].ToList();
                var numRemaining = nc;
                for (var jp = 0; jp < nc; jp++)
                {
                    remaining[jp] = jp;
                    shortestPathCosts[jp] = double.PositiveInfinity;
                }
                Array.Clear(sr, 0, sr.Length);
                Array.Clear(sc, 0, sc.Length);

                // Start finding augmenting path
                var sink = -1;
                while (sink == -1)
                {
                    sr[i] = true;
                    var indexLowest = -1;
                    var lowest = double.PositiveInfinity;
                    for (var jk = 0; jk < numRemaining; jk++)
                    {
                        var jl = remaining[jk];
                        // Note that this is the main bottleneck of this method; looking up the cost array
                        // is costly. Some obvious attempts to improve performance include swapping rows and
                        // columns, and disabling CLR bounds checking by using pointers to access the elements
                        // instead. We do not seem to get any significant improvements over the simpler
                        // approach below though.
                        var r = minVal + cost[i, jl] - u[i] - v[jl];
                        if (r < shortestPathCosts[jl])
                        {
                            path[jl] = i;
                            shortestPathCosts[jl] = r;
                        }
                        // Console.WriteLine(lowest + " " + shortestPathCosts[jl]);

                        if (shortestPathCosts[jl] < lowest || shortestPathCosts[jl] == lowest && y[jl] == -1)
                        {
                            lowest = shortestPathCosts[jl];
                            indexLowest = jk;
                        }
                    }
                    minVal = lowest;
                    // Console.WriteLine(indexLowest);
                    var jp = remaining[indexLowest];
                    if (double.IsPositiveInfinity(minVal))
                        throw new InvalidOperationException("No feasible solution.");
                    if (y[jp] == -1)
                        sink = jp;
                    else
                        i = y[jp];

                    sc[jp] = true;
                    remaining[indexLowest] = remaining[--numRemaining];
                    remaining.RemoveAt(numRemaining);
                }
                if (sink < 0)
                    throw new InvalidOperationException("No feasible solution.");

                // Update dual variables
                u[curRow] += minVal;
                for (var ip = 0; ip < nr; ip++)
                    if (sr[ip] && ip != curRow)
                        u[ip] += minVal - shortestPathCosts[x[ip]];

                for (var jp = 0; jp < nc; jp++)
                    if (sc[jp])
                        v[jp] -= minVal - shortestPathCosts[jp];

                // Augment previous solution
                var j = sink;
                while (true)
                {
                    var ip = path[j];
                    y[j] = ip;
                    (j, x[ip]) = (x[ip], j);
                    if (ip == curRow)
                        break;
                }
            }
            var ret = new List<int[]>();
            Array.Sort(y);
            ret.Add(y);
            ret.Add(x);
            return ret;
        }

    }
}
