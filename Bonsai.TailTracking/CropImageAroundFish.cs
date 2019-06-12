using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.TailTracking
{

    [Description("Creates a dynamic crop window that can be used to crop an image around a tracked object.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CropImageAroundFish : Transform<IplImage, IplImage>
    {

        [Description("X value to use for the offset of the crop.")]
        public int X { get; set; }

        [Description("Y value to use for the offset of the crop.")]
        public int Y { get; set; }

        [Description("Width of the crop.")]
        public int Width { get; set; }

        [Description("Height of the crop.")]
        public int Height { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            Rect rect = new Rect(X, Y, Width, Height);
            return source.Select(input =>
            {
                if (rect.Width > 0 && rect.Height > 0)
                {
                    return input.GetSubRect(rect);
                }
                return input;
            });
        }
    }
}
