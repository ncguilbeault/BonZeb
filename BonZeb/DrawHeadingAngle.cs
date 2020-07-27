using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;
using Bonsai;

namespace BonZeb
{
    public class DrawHeadingAngle : Transform<Tuple<IplImage, Point2f[]>, IplImage>
    {

        public DrawHeadingAngle()
        {
            Colour = new Scalar(255, 0, 0, 255);
            Thickness = 1;
            LineLength = 5;
            CapSize = 2;
            LineOffset = 1;
        }

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

        private int capSize;
        [Description("Size of the cap.")]
        public int CapSize { get => capSize; set => capSize = value < 1 ? 1 : value; }

        [Description("Distance to offset the line from the centroid.")]
        public double LineOffset { get; set; }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f[]>> source)
        { 
            return source.Select(value => DrawHeadingAngleWithPointsFunc(value.Item1, value.Item2));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<Point2f[], IplImage>> source)
        {
            return source.Select(value => DrawHeadingAngleWithPointsFunc(value.Item2, value.Item1));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<Point2f[], IplImage, double>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item2, value.Item1[0], value.Item3));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<Point2f[], double, IplImage>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item3, value.Item1[0], value.Item2));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<double, IplImage, Point2f[]>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item2, value.Item3[0], value.Item1));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<double, Point2f[], IplImage>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item3, value.Item2[0], value.Item1));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, double, Point2f[]>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item1, value.Item3[0], value.Item2));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f[], double>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item1, value.Item2[0], value.Item3));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<Point2f, IplImage, double>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item2, value.Item1, value.Item3));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<Point2f, double, IplImage>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item3, value.Item1, value.Item2));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<double, IplImage, Point2f>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item2, value.Item3, value.Item1));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<double, Point2f, IplImage>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item3, value.Item2, value.Item1));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, double, Point2f>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item1, value.Item3, value.Item2));
        }
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f, double>> source)
        {
            return source.Select(value => DrawHeadingAngleWithAngleFunc(value.Item1, value.Item2, value.Item3));
        }
        private IplImage DrawHeadingAngleWithPointsFunc(IplImage image, Point2f[] points)
        {
            if (points.Length <= 0)
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
            double headingAngle = Math.Atan2(points[0].Y - points[1].Y, points[0].X - points[1].X);
            Point startPoint = new Point((int)(LineOffset * Math.Cos(headingAngle) + points[0].X), (int)(LineOffset * Math.Sin(headingAngle) + points[0].Y));
            Point endPoint = new Point((int)(lineLength * Math.Cos(headingAngle) + startPoint.X), (int)(lineLength * Math.Sin(headingAngle) + startPoint.Y));
            Point firstCapPoint = new Point((int)(capSize * Math.Cos(headingAngle + 3 * Math.PI / 4) + endPoint.X), (int)(capSize * Math.Sin(headingAngle + 3 * Math.PI / 4) + endPoint.Y));
            Point secondCapPoint = new Point((int)(capSize * Math.Cos(headingAngle + 5 * Math.PI / 4) + endPoint.X), (int)(capSize * Math.Sin(headingAngle + 5 * Math.PI / 4) + endPoint.Y));
            CV.Line(newImage, startPoint, endPoint, Colour, thickness);
            CV.Line(newImage, endPoint, firstCapPoint, Colour, thickness);
            CV.Line(newImage, endPoint, secondCapPoint, Colour, thickness);
            return newImage;
        }
        private IplImage DrawHeadingAngleWithAngleFunc(IplImage image, Point2f point, double headingAngle)
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
            Point startPoint = new Point((int)(LineOffset * Math.Cos(headingAngle) + point.X), (int)(LineOffset * Math.Sin(headingAngle) + point.Y));
            Point endPoint = new Point((int)(lineLength * Math.Cos(headingAngle) + startPoint.X), (int)(lineLength * Math.Sin(headingAngle) + startPoint.Y));
            Point firstCapPoint = new Point((int)(capSize * Math.Cos(headingAngle + 3 * Math.PI / 4) + endPoint.X), (int)(capSize * Math.Sin(headingAngle + 3 * Math.PI / 4) + endPoint.Y));
            Point secondCapPoint = new Point((int)(capSize * Math.Cos(headingAngle + 5 * Math.PI / 4) + endPoint.X), (int)(capSize * Math.Sin(headingAngle + 5 * Math.PI / 4) + endPoint.Y));
            CV.Line(newImage, startPoint, endPoint, Colour, thickness);
            CV.Line(newImage, endPoint, firstCapPoint, Colour, thickness);
            CV.Line(newImage, endPoint, secondCapPoint, Colour, thickness);
            return newImage;
        }
    }
}
