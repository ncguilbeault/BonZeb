using OpenCV.Net;

namespace BonZeb
{
    public class TailAngleData<T>
    {
        public T Angles { get; set; }
        public Point2f[] Points { get; set; }
        public IplImage Image { get; set; }
        public TailAngleData(T angles, Point2f[] points = null, IplImage image = null)
        {
            Angles = angles;
            Image = image;
            Points = points;
            Angles = angles;
        }
    }
}
