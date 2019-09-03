using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;
using Bonsai.Vision;

namespace Bonsai.TailTracking
{

    [Description("Draws tracking points onto image.")]
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
        [Description("Thickness of tracking lines.")]
        public int LineLength { get => lineLength; set => lineLength = value < 1 ? 1 : value; }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, ConnectedComponentCollection>> source)
        {
            return source.Select(value =>
            {
                IplImage newImage;
                if (value.Item1.Channels == 1)
                {
                    newImage = new IplImage(new Size(value.Item1.Size.Width, value.Item1.Size.Height), value.Item1.Depth, 3);
                    CV.CvtColor(value.Item1, newImage, ColorConversion.Gray2Bgr);
                }
                else
                {
                    newImage = value.Item1.Clone();
                }
                for (int i = 0; i < value.Item2.Count; i++)
                {
                    CV.Line(newImage, new Point((int)(lineLength * Math.Cos(value.Item2[i].Orientation + Math.PI) + value.Item2[i].Centroid.X), (int)(lineLength * Math.Sin(value.Item2[i].Orientation + Math.PI) + value.Item2[i].Centroid.Y)), new Point((int)(lineLength * Math.Cos(value.Item2[i].Orientation) + value.Item2[i].Centroid.X), (int)(lineLength * Math.Sin(value.Item2[i].Orientation) + value.Item2[i].Centroid.Y)), Colour, thickness);
                }
                return newImage;
            });
        }
        public IObservable<IplImage> Process(IObservable<Tuple<ConnectedComponentCollection, IplImage>> source)
        {
            return source.Select(value =>
            {
                IplImage newImage;
                if (value.Item2.Channels == 1)
                {
                    newImage = new IplImage(new Size(value.Item2.Size.Width, value.Item2.Size.Height), value.Item2.Depth, 3);
                    CV.CvtColor(value.Item2, newImage, ColorConversion.Gray2Bgr);
                }
                else
                {
                    newImage = value.Item2.Clone();
                }
                for (int i = 0; i < value.Item1.Count; i++)
                {
                    CV.Line(newImage, new Point((int)(lineLength * Math.Cos(value.Item1[i].Orientation + Math.PI) + value.Item1[i].Centroid.X), (int)(lineLength * Math.Sin(value.Item1[i].Orientation + Math.PI) + value.Item1[i].Centroid.Y)), new Point((int)(lineLength * Math.Cos(value.Item1[i].Orientation) + value.Item1[i].Centroid.X), (int)(lineLength * Math.Sin(value.Item1[i].Orientation) + value.Item1[i].Centroid.Y)), Colour, thickness);
                }
                return newImage;
            });
        }
    }
}