using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class TailPoints
    {
        // Class used for creating a data type which contains the calculated tail points.
        public Point2f[] Points { get; set; }
        public IplImage Image { get; set; }
        public TailPoints(Point2f[] points, IplImage image)
        {
            Points = points;
            Image = image;
        }
    }
}
