using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;


namespace Bonsai.TailTracking
{

    [Description("Converts image data into byte array.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class ConvertToRawImageData : Transform<IplImage, RawImageData>
    {
        public override IObservable<RawImageData> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                byte[] imageData = Utilities.ConvertIplImageToByteArray(value);
                return new RawImageData(imageData, value.Size.Width, value.Size.Height, value.WidthStep);
            });
        }
    }
}
