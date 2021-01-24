using OpenCV.Net;

namespace BonZeb
{
    public class TailPointData
    {
        // Class used for creating a data type which contains the calculated tail points.
        public Point2f[] Points { get; set; }
        public IplImage Image { get; set; }
        public TailPointData(Point2f[] points, IplImage image)
        {
            Points = points;
            Image = image;
        }
    }
}
