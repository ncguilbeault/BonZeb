using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class BackgroundSubtractionData
    {
        public IplImage Image { get; set; }
        public IplImage Background { get; set; }
        public IplImage BackgroundSubtractedImage { get; set; }
        public BackgroundSubtractionData(IplImage image, IplImage background = null, IplImage backgroundSubtractedImage = null)
        {
            Image = image;
            Background = background;
            BackgroundSubtractedImage = backgroundSubtractedImage;
        }
    }
}
