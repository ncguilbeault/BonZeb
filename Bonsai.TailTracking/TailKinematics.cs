namespace Bonsai.TailTracking
{
<<<<<<< HEAD
    public struct TailKinematics
=======
    public class TailKinematics : TailAngles
>>>>>>> parent of d33a178... Added custom visualizers for tail angles and for centroid tracking.
    {
        // Class used for creating a data type which contains the amplitudes, frequency, and instances of bouts in tail curvature data.
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public bool Instance { get; set; }
<<<<<<< HEAD
        public TailKinematics(double frequency, double amplitude, bool instance)
=======
        public TailKinematics(double frequency, double amplitude, bool instance, double[] angles = null, Point2f[] points = null, IplImage image = null) : base(angles, points, image)
>>>>>>> parent of d33a178... Added custom visualizers for tail angles and for centroid tracking.
        {
            Frequency = frequency;
            Amplitude = amplitude;
            Instance = instance;
        }
    }
}
