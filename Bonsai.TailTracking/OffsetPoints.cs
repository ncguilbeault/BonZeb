using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class OffsetPoints
    {

        Point2f[] Points;
        Point2f Point;

        public OffsetPoints(Point2f[] points, float offsetX, float offsetY)
        {
            // Function that applies an offset to an array of points.
            Points = new Point2f[points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new Point2f(points[i].X + offsetX, points[i].Y + offsetY);
            }
        }

        public OffsetPoints(Point2f point, float offsetX, float offsetY)
        {
            // Function that applies an offset to a point.
            Point = new Point2f(point.X + offsetX, point.Y + offsetY);
        }
    }
}
