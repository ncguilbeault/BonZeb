using OpenCV.Net;
using Bonsai.Vision;

namespace Bonsai.TailTracking
{
    public class CentroidData
    {
        // Class used for creating a data type which contains the calculated tail points.
        public Point2f Centroid { get; set; }
        public IplImage Image { get; set; }
        public IplImage ThresholdImage { get; set; }
        public IplImage Contours { get; set; }
        public IplImage Background { get; set; }
        public IplImage BackgroundSubtractedImage { get; set; }
        public CentroidData(Point2f centroid, IplImage image, IplImage thresholdImage, IplImage contours = null, IplImage background = null, IplImage backgroundSubtractedImage = null)
        {
            Centroid = centroid;
            Image = image;
            ThresholdImage = thresholdImage;
            Contours = contours;
            Background = background;
            BackgroundSubtractedImage = backgroundSubtractedImage;
        }
    }
}