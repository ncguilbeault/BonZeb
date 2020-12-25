using OpenCV.Net;

namespace BonZeb
{
    public class TailAngles<T>
    {
        public T Angles { get; set; }
        public Point2f[] Points { get; set; }
        public IplImage Image { get; set; }
        public TailAngles(T angles, Point2f[] points = null, IplImage image = null)
        {
            Image = image;
            Points = points;
            Angles = angles;
        }
    }
}
