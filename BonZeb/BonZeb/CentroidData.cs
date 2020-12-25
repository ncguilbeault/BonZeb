using OpenCV.Net;
using Bonsai.Vision;

namespace BonZeb
{
    public class CentroidData
    {
        // Class used for creating a data type which contains the calculated tail points.
        public Point2f Centroid { get; set; }
        public IplImage Image { get; set; }
        public IplImage ThresholdImage { get; set; }
        public IplImage Contours { get; set; }
        public CentroidData(Point2f centroid, IplImage image, IplImage thresholdImage, IplImage contours = null)
        {
            Centroid = centroid;
            Image = image;
            ThresholdImage = thresholdImage;
            Contours = contours;
        }
    }
}