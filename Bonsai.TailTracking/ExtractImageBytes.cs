using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Drawing.Design;
using OpenCV.Net;


namespace Bonsai.TailTracking
{
    public class ExtractImageData : Transform<IplImage, Tuple<byte[], int, int>>
    {
        public override IObservable<Tuple<byte[], int, int>> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                int height = value.Size.Height;
                int widthStep = value.WidthStep;
                byte[] imageData = new byte[widthStep * height];
                Marshal.Copy(value.ImageData, imageData, 0, widthStep * height);
                return new Tuple<byte[], int, int>(imageData, widthStep, height);
            });
        }
    }
}
