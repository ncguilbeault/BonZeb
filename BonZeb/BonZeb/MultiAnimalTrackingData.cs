using OpenCV.Net;
using Bonsai.Vision;

namespace BonZeb
{
    public class MultiAnimalTrackingData
    {
        // Class used for creating a data type which contains multi animal tracking data.
        public Point2f[] UnorderedCentroids { get; set; }
        public int[] UnorderedIdentities { get; set; }
        public Point2f[] OrderedCentroids { get; set; }
        public IplImage Image { get; set; }
        public IplImage BackgroundSubtractedImage { get; set; }
        public IplImage ThresholdImage { get; set; }

        public MultiAnimalTrackingData(IplImage image, Point2f[] unorderedCentroids = null, int[] unorderedIdentities = null, Point2f[] orderedCentroids = null, IplImage thresholdImage = null, IplImage backgroundSubtractedImage = null, ConnectedComponent largestContour = null)
        {
            UnorderedCentroids = unorderedCentroids;
            UnorderedIdentities = unorderedIdentities;
            OrderedCentroids = orderedCentroids;
            Image = image;
            ThresholdImage = thresholdImage;
            BackgroundSubtractedImage = backgroundSubtractedImage;
        }

        public MultiAnimalTrackingData(IplImage image, IplImage thresholdImage = null, IplImage backgroundSubtractedImage = null)
        {
            UnorderedCentroids = null;
            UnorderedIdentities = null;
            OrderedCentroids = null;
            Image = image;
            ThresholdImage = thresholdImage;
            BackgroundSubtractedImage = backgroundSubtractedImage;
        }
    }
}