using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class BackgroundSubtractionData
    {
        public IplImage Image { get; set; }
        public IplImage Background { get; set; }
        public IplImage BackgroundSubtractedImage { get; set; }
        public BackgroundSubtractionData(IplImage backgroundSubtractedImage, IplImage background, IplImage image)
        {
            Image = image;
            Background = background;
            BackgroundSubtractedImage = backgroundSubtractedImage;
        }
    }
}
