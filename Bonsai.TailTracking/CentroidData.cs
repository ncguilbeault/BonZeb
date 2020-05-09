using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class CentroidData
    {
        // Class used for creating a data type which contains the calculated tail points.
        public Point2f Centroid { get; set; }
        public IplImage Image { get; set; }
        public IplImage ThresholdImage { get; set; }
        public CentroidData(Point2f centroid, IplImage image, IplImage thresholdImage)
        {
            Centroid = centroid;
            Image = image;
            ThresholdImage = thresholdImage;
        }
    }
}