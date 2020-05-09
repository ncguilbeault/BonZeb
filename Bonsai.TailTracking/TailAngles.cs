using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class TailAngles
    {
        public double[] Angles { get; set; }
        public Point2f[] Points { get; set; }
        public IplImage Image { get; set; }
        public TailAngles(double[] angles, Point2f[] points = null, IplImage image = null)
        {
            Image = image;
            Points = points;
            Angles = angles;
        }
    }
}
