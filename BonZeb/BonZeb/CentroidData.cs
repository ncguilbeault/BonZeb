using OpenCV.Net;
using Bonsai.Vision;

namespace BonZeb
{
    public class CentroidData
    {
        // Class used for creating a data type which contains the centroid data.
        public Point2f Centroid { get; set; }
        public IplImage Image { get; set; }
        public IplImage BackgroundSubtractedImage { get; set; }
        public IplImage ThresholdImage { get; set; }
        public ConnectedComponent LargestContour { get; set; }
        public CentroidData(Point2f centroid, IplImage image, IplImage thresholdImage = null, IplImage backgroundSubtractedImage = null, ConnectedComponent largestContour = null)
        {
            Centroid = centroid;
            Image = image;
            ThresholdImage = thresholdImage;
            BackgroundSubtractedImage = backgroundSubtractedImage;
            LargestContour = largestContour;
        }
    }
}