using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using OpenCV.Net;


namespace Bonsai.TailTracking
{
    public class ExtractRawImageData : Transform<IplImage, Utilities.RawImageData>
    {
        public override IObservable<Utilities.RawImageData> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                byte[] imageData = new byte[value.WidthStep * value.Size.Height];
                Marshal.Copy(value.ImageData, imageData, 0, value.WidthStep * value.Size.Height);
                return new Utilities.RawImageData(imageData, value.Size.Width, value.Size.Height, value.WidthStep);
            });
        }
    }
}
