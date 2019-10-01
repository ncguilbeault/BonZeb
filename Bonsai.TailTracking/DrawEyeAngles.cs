using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;
using Bonsai.Vision;

namespace Bonsai.TailTracking
{

    [Description("Draws eye angles onto eyes in image.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DrawEyeAngles : Transform<Tuple<IplImage, ConnectedComponentCollection>, IplImage>
    {
        public DrawEyeAngles()
        {
            Colour = new Scalar(255, 0, 0, 255);
            Thickness = 1;
            LineLength = 5;
        }

        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Colour used for overlaying eye angles onto image.")]
        public Scalar Colour { get; set; }

        private int thickness;
        [Description("Thickness of tracking lines.")]
        public int Thickness { get => thickness; set => thickness = value < 1 ? 1 : value; }

        private int lineLength;
        [Description("Langth of tracking lines.")]
        public int LineLength { get => lineLength; set => lineLength = value < 1 ? 1 : value; }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, ConnectedComponentCollection>> source)
        {
            return source.Select(value => DrawEyeAnglesFunc(value.Item2, value.Item1));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<ConnectedComponentCollection, IplImage>> source)
        {
            return source.Select(value => DrawEyeAnglesFunc(value.Item1, value.Item2));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, ConnectedComponent>> source)
        {
            return source.Select(value => DrawEyeAngleFunc(value.Item2, value.Item1));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<ConnectedComponent, IplImage>> source)
        {
            return source.Select(value => DrawEyeAngleFunc(value.Item1, value.Item2));
        }
        private IplImage DrawEyeAngleFunc(ConnectedComponent eye, IplImage image)
        {
            IplImage newImage;
            if (image.Channels == 1)
            {
                newImage = new IplImage(new Size(image.Width, image.Height), image.Depth, 3);
                CV.CvtColor(image, newImage, ColorConversion.Gray2Bgr);
            }
            else
            {
                newImage = image.Clone();
            }
            CV.Line(newImage, new Point((int)(lineLength * Math.Cos(eye.Orientation + Math.PI) + eye.Centroid.X), (int)(lineLength * Math.Sin(eye.Orientation + Math.PI) + eye.Centroid.Y)), new Point((int)(lineLength * Math.Cos(eye.Orientation) + eye.Centroid.X), (int)(lineLength * Math.Sin(eye.Orientation) + eye.Centroid.Y)), Colour, thickness);
            return newImage;
        }
        private IplImage DrawEyeAnglesFunc(ConnectedComponentCollection eyes, IplImage image)
        {
            IplImage newImage;
            if (image.Channels == 1)
            {
                newImage = new IplImage(new Size(image.Width, image.Height), image.Depth, 3);
                CV.CvtColor(image, newImage, ColorConversion.Gray2Bgr);
            }
            else
            {
                newImage = image.Clone();
            }
            foreach (ConnectedComponent eye in eyes)
            {
                CV.Line(newImage, new Point((int)(lineLength * Math.Cos(eye.Orientation + Math.PI) + eye.Centroid.X), (int)(lineLength * Math.Sin(eye.Orientation + Math.PI) + eye.Centroid.Y)), new Point((int)(lineLength * Math.Cos(eye.Orientation) + eye.Centroid.X), (int)(lineLength * Math.Sin(eye.Orientation) + eye.Centroid.Y)), Colour, thickness);
            }
            return newImage;
        }
    }
}