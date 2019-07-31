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

        private int x;
        [Description("X value to use for the offset of the crop.")]
        public int X { get { return x; } set { x = value; } }

        private int y;
        [Description("Y value to use for the offset of the crop.")]
        public int Y { get { return y; } set { y = value; } }

        private int width;
        [Description("Width of the crop.")]
        public int Width { get { return width; } set { width = value; } }

        private int height;
        [Description("Height of the crop.")]
        public int Height { get { return height; } set { height = value; } }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                Rect rect = new Rect(x, y, width, height);
                if (rect.Width > 0 && rect.Height > 0)
                {
                    return input.GetSubRect(rect);
                }
                return input;
            });
        }
    }
}
