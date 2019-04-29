using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Drawing.Design;
using OpenCV.Net;


namespace Bonsai.TailTracking
{
    public class ExtractImageData : Transform<IplImage, Utilities.RawImageData>
    {
        public override IObservable<Utilities.RawImageData> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                Utilities.RawImageData RawImageData = new Utilities.RawImageData();
                int height = value.Size.Height;
                int widthStep = value.WidthStep;
                byte[] imageData = new byte[widthStep * height];
                Marshal.Copy(value.ImageData, imageData, 0, widthStep * height);
                RawImageData.ImageData = imageData;
                RawImageData.Height = height;
                RawImageData.WidthStep = widthStep;
                return RawImageData;
            });
        }
    }
}
