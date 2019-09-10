using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;
using Bonsai.Vision;

namespace Bonsai.TailTracking
{
    public class DrawHeadingAngle : Transform<Tuple<IplImage, Point2f[]>, IplImage>
    {
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Colour used for overlaying heading angle onto image.")]
        public Scalar Colour { get; set; }

        private int thickness;
        [Description("Thickness of line.")]
        public int Thickness { get => thickness; set => thickness = value < 1 ? 1 : value; }

        private int lineLength;
        [Description("Length of line.")]
        public int LineLength { get => lineLength; set => lineLength = value < 1 ? 1 : value; }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f[]>> source)
        { 
            return source.Select(value =>
            {
                return DrawHeadingAngleFunc(value.Item1, value.Item2);
            });
        }
        public IObservable<IplImage> Process(IObservable<Tuple<Point2f[], IplImage>> source)
        {
            return source.Select(value =>
            {
                return DrawHeadingAngleFunc(value.Item2, value.Item1);
            });
        }
        IplImage DrawHeadingAngleFunc(IplImage image, Point2f[] points)
        {
            IplImage newImage;
            if (image.Channels == 1)
            {
                newImage = new IplImage(new Size(image.Size.Width, image.Size.Height), image.Depth, 3);
                CV.CvtColor(image, newImage, ColorConversion.Gray2Bgr);
            }
            else
            {
                newImage = image.Clone();
            }
            double headingAngle = Math.Atan2(points[0].Y - points[1].Y, points[0].X - points[1].X);
            CV.Line(newImage, new Point((int)(0.5 * lineLength * Math.Cos(headingAngle) + points[0].X), (int)(0.5 * lineLength * Math.Sin(headingAngle) + points[0].Y)), new Point((int)(1.5 * lineLength * Math.Cos(headingAngle) + points[0].X), (int)(1.5 * lineLength * Math.Sin(headingAngle) + points[0].Y)), Colour, thickness);
            return newImage;
        }
    }
}
