using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class TailKinematics : TailAngles
    {
        // Class used for creating a data type which contains the amplitudes, frequency, and instances of bouts in tail curvature data.
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public bool Instance { get; set; }
        public TailKinematics(double frequency, double amplitude, bool instance, double[] angles = null, Point2f[] points = null, IplImage image = null) : base(angles, points, image)
        {
            Image = image;
            Points = points;
            Angles = angles;
            Frequency = frequency;
            Amplitude = amplitude;
            Instance = instance;
        }
    }
}
