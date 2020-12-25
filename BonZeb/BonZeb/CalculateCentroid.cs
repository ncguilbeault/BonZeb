using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;
using Bonsai.Vision;
using Bonsai;
using System.Collections.Generic;

namespace BonZeb
{

    [Description("Calculates the centroid of a binarized image using the first-order raw image moments.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateCentroid : Transform<BackgroundSubtractionData, CentroidData>
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

        [Description("The method for performing centroid tracking.")]
        public CentroidTrackingMethod CentroidTrackingMethod { get; set; }

        private double minArea;
        [Description("The minimum area for individual contours to be accepted.")]
        public double MinArea { get => minArea; set => minArea = value > 1 && (!maxArea.HasValue || value <= maxArea) ? value : 1; }

        private double? maxArea;
        [Description("The maximum area for individual contours to be accepted.")]
        public double? MaxArea { get => maxArea; set => maxArea = value.HasValue && value >= minArea ? value : null; }

        public CalculateCentroid()
        {
            ThresholdValue = 100;
            ThresholdType = ThresholdTypes.Binary;
            MaxThresholdValue = 255;
            MinArea = 0;
            MaxArea = null;
        }

        public override IObservable<CentroidData> Process(IObservable<BackgroundSubtractionData> source)
        {
            return source.Select(value => CalculateCentroidFunc(value));
        }
        public IObservable<CentroidData> Process(IObservable<IplImage> source)
        {
            return source.Select(value => CalculateCentroidFunc(new BackgroundSubtractionData(value)));
        }
        private CentroidData CalculateCentroidFunc(BackgroundSubtractionData input)
        {
            IplImage image = input.Image;
            IplImage thresh = new IplImage(image.Size, image.Depth, image.Channels);
            CV.Threshold(image, thresh, ThresholdValue, maxValue, ThresholdType);
            Moments moments = new Moments(thresh, true);
            Point2f centroid = new Point2f((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
            if (CentroidTrackingMethod == CentroidTrackingMethod.LargestBinaryRegion)
            {
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
                if (connectedComponents.Count != 0)
                {
                    List<ConnectedComponent> sortedComponents = connectedComponents.OrderByDescending(contour => contour.Area).ToList();
                    centroid = sortedComponents[0].Centroid;
                    IplImage contourImage = new IplImage(temp.Size, IplDepth.U8, 3);
                    contourImage.SetZero();


                    CV.DrawContours(contourImage, sortedComponents[0].Contour, Scalar.All(255), Scalar.All(0), 0, -1);
                    CV.DrawContours(contourImage, sortedComponents[0].Contour, new Scalar(0, 0, 255), Scalar.All(0), 0, 1);

                    return new CentroidData(centroid, image, thresh, contourImage);
                }
            }
            return new CentroidData(centroid, image, thresh);
        }
    }
}
