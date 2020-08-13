using OpenCV.Net;

namespace BonZeb
{
    public struct CalibrationParameters
    {
        // Class used for creating a draw parameters type.
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public double XRange { get; set; }
        public double YRange { get; set; }
        public Scalar Colour { get; set; }
        public CalibrationParameters(double xOffset, double yOffset, double xRange, double yRange, Scalar colour)
        {
            XOffset = xOffset;
            YOffset = yOffset;
            XRange = xRange;
            YRange = yRange;
            Colour = colour;
        }
    }
}
