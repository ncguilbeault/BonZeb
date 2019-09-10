using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using OpenCV.Net;
using System.ComponentModel;


namespace Bonsai.TailTracking
{

    [Description("Converts image data into byte array.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class ConvertToRawImageData : Transform<IplImage, Utilities.RawImageData>
    {
        public override IObservable<Utilities.RawImageData> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                byte[] imageData = Utilities.ConvertIplImageToByteArray(value);
                return new Utilities.RawImageData(imageData, value.Size.Width, value.Size.Height, value.WidthStep);
            });
        }
    }
}
