using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class CropImageAroundFish : Transform<IplImage, IplImage>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                Rect rect = new Rect(X, Y, Width, Height);
                if (rect.Width > 0 && rect.Height > 0)
                {
                    return input.GetSubRect(rect);
                }

                return input;
            });
        }
    }
}
