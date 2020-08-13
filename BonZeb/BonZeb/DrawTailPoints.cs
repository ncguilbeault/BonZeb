using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using System.Collections.Generic;
using OpenCV.Net;
using Bonsai;

namespace BonZeb
{

    [Description("Draws tracking points onto image.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DrawTailPoints : Transform<Tuple<IplImage, Point2f[]>, IplImage>
    {
        public DrawTailPoints()
        {
            Colour = new Scalar(255, 0, 0, 255);
            Radius = 1;
            Thickness = 1;
            Fill = true;
        }

        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Colour used for overlaying tracking points onto image.")]
        public Scalar Colour { get; set; }

        private int radius;
        [Description("Radius used for drawing tracking points.")]
        public int Radius { get => radius; set => radius = value > 1 ? value : 1; }

        private int thickness;
        [Description("Thickness of tracking point border.")]
        public int Thickness { get => thickness; set => thickness = fill ? -1 : value < 1 ? 1 : value; }

        private bool fill;
        [Description("Fills tracking points with colour.")]
        public bool Fill { get => fill; set {fill = value; thickness = value ? -1 : 1;} }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f[]>> source)
        {
            return source.Select(value => DrawTailPointsFunc(value.Item1, value.Item2));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<Point2f[], IplImage>> source)
        {
            return source.Select(value => DrawTailPointsFunc(value.Item2, value.Item1));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f>> source)
        {
            return source.Select(value => DrawTailPointFunc(value.Item1, value.Item2));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<Point2f, IplImage>> source)
        {
            return source.Select(value => DrawTailPointFunc(value.Item2, value.Item1));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f[][]>> source)
        {
            return source.Select(value => DrawTailPointsArrayFunc(value.Item1, value.Item2));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<Point2f[][], IplImage>> source)
        {
            return source.Select(value => DrawTailPointsArrayFunc(value.Item2, value.Item1));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IList<Point2f>>> source)
        {
            return source.Select(value => DrawTailPointsFunc(value.Item1, value.Item2.ToArray()));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IList<Point2f>, IplImage>> source)
        {
            return source.Select(value => DrawTailPointsFunc(value.Item2, value.Item1.ToArray()));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IList<Point2f[]>>> source)
        {
            return source.Select(value => DrawTailPointsArrayFunc(value.Item1, value.Item2.ToArray()));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IList<Point2f[]>, IplImage>> source)
        {
            return source.Select(value => DrawTailPointsArrayFunc(value.Item2, value.Item1.ToArray()));
        }

        private IplImage DrawTailPointFunc(IplImage image, Point2f point)
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
            CV.Circle(newImage, new Point((int)point.X, (int)point.Y), radius, Colour, thickness);
            return newImage;
        }

        private IplImage DrawTailPointsFunc(IplImage image, Point2f[] points)
        {
            if (points.Length == 0)
            {
                return image;
            }
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
            foreach (Point2f point in points)
            {
                CV.Circle(newImage, new Point((int)point.X, (int)point.Y), radius, Colour, thickness);
            }
            return newImage;
        }

        private IplImage DrawTailPointsArrayFunc(IplImage image, Point2f[][] pointsArray)
        {
            if (pointsArray.Length == 0)
            {
                return image;
            }
            else if (pointsArray.Length == 1)
            {
                return DrawTailPointsFunc(image, pointsArray[0]);
            }
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
            foreach (Point2f[] points in pointsArray)
            {
                foreach (Point2f point in points)
                {
                    CV.Circle(newImage, new Point((int)point.X, (int)point.Y), radius, Colour, thickness);
                }
            }
            return newImage;
        }
    }
}
