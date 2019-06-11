using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using System.Collections.Generic;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Draws tracking points onto image.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DrawTailPoints : Transform<Tuple<IplImage, Point2f[]>, IplImage>
    {
        public DrawTailPoints()
        {
            Colour = new Scalar(255, 0, 0, 255);
            Radius = 1;
            Thickness = 0;
            Fill = true;
        }

        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Colour used for overlaying tracking points onto image.")]
        public Scalar Colour { get; set; }

        [Description("Radius used for drawing tracking points.")]
        public int Radius { get; set; }

        [Description("Thickness of tracking point border.")]
        public int Thickness { get; set; }

        [Description("Fills tracking points with colour.")]
        public bool Fill { get; set; }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f[]>> source)
        {
            return source.Select(value =>
            {
                IplImage image = value.Item1;
                Point2f[] points = value.Item2;
                Scalar colour = Colour;
                int radius = Radius;
                int thickness = Thickness;
                bool fill = Fill;
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
                for (int i = 0; i < points.Length; i++)
                {
                    if (!fill)
                    {
                        CV.Circle(newImage, new Point((int)points[i].X, (int)points[i].Y), radius, colour, thickness);
                    }
                    else
                    {
                        CV.Circle(newImage, new Point((int)points[i].X, (int)points[i].Y), radius, colour, -1);
                    }
                }
                return newImage;
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<Point2f[], IplImage>> source)
        {
            return source.Select(value =>
            {
                Point2f[] points = value.Item1;
                IplImage image = value.Item2;
                Scalar colour = Colour;
                int radius = Radius;
                int thickness = Thickness;
                bool fill = Fill;
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
                for (int i = 0; i < points.Length; i++)
                {
                    if (!fill)
                    {
                        CV.Circle(newImage, new Point((int)points[i].X, (int)points[i].Y), radius, colour, thickness);
                    }
                    else
                    {
                        CV.Circle(newImage, new Point((int)points[i].X, (int)points[i].Y), radius, colour, -1);
                    }
                }
                return newImage;
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<Point2f[][], IplImage>> source)
        {
            return source.Select(value =>
            {
                Point2f[][] points = value.Item1;
                IplImage image = value.Item2;
                Scalar colour = Colour;
                int radius = Radius;
                int thickness = Thickness;
                bool fill = Fill;
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
                for (int i = 0; i < points.Length; i++)
                {
                    for (int j = 0; j < points[i].Length; j++)
                    {
                        if (!fill)
                        {
                            CV.Circle(newImage, new Point((int)points[i][j].X, (int)points[i][j].Y), radius, colour, thickness);
                        }
                        else
                        {
                            CV.Circle(newImage, new Point((int)points[i][j].X, (int)points[i][j].Y), radius, colour, -1);
                        }
                    }
                }
                return newImage;
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f[][]>> source)
        {
            return source.Select(value =>
            {
                IplImage image = value.Item1;
                Point2f[][] points = value.Item2;
                Scalar colour = Colour;
                int radius = Radius;
                int thickness = Thickness;
                bool fill = Fill;
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
                for (int i = 0; i < points.Length; i++)
                {
                    for (int j = 0; j < points[i].Length; j++)
                    {
                        if (!fill)
                        {
                            CV.Circle(newImage, new Point((int)points[i][j].X, (int)points[i][j].Y), radius, colour, thickness);
                        }
                        else
                        {
                            CV.Circle(newImage, new Point((int)points[i][j].X, (int)points[i][j].Y), radius, colour, -1);
                        }
                    }
                }
                return newImage;
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IList<Point2f>>> source)
        {
            return source.Select(value =>
            {
                IplImage image = value.Item1;
                IList<Point2f> points = value.Item2;
                Scalar colour = Colour;
                int radius = Radius;
                int thickness = Thickness;
                bool fill = Fill;
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
                for (int i = 0; i < points.Count; i++)
                {
                    if (!fill)
                    {
                        CV.Circle(newImage, new Point((int)points[i].X, (int)points[i].Y), radius, colour, thickness);
                    }
                    else
                    {
                        CV.Circle(newImage, new Point((int)points[i].X, (int)points[i].Y), radius, colour, -1);
                    }
                }
                return newImage;
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IList<Point2f>, IplImage>> source)
        {
            return source.Select(value =>
            {
                IList<Point2f> points = value.Item1;
                IplImage image = value.Item2;
                Scalar colour = Colour;
                int radius = Radius;
                int thickness = Thickness;
                bool fill = Fill;
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
                for (int i = 0; i < points.Count; i++)
                {
                    if (!fill)
                    {
                        CV.Circle(newImage, new Point((int)points[i].X, (int)points[i].Y), radius, colour, thickness);
                    }
                    else
                    {
                        CV.Circle(newImage, new Point((int)points[i].X, (int)points[i].Y), radius, colour, -1);
                    }
                }
                return newImage;
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IList<Point2f[]>>> source)
        {
            return source.Select(value =>
            {
                IplImage image = value.Item1;
                IList<Point2f[]> points = value.Item2;
                Scalar colour = Colour;
                int radius = Radius;
                int thickness = Thickness;
                bool fill = Fill;
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
                for (int i = 0; i < points.Count; i++)
                {
                    for (int j = 0; j < points[i].Length; j++)
                    {
                        if (!fill)
                        {
                            CV.Circle(newImage, new Point((int)points[i][j].X, (int)points[i][j].Y), radius, colour, thickness);
                        }
                        else
                        {
                            CV.Circle(newImage, new Point((int)points[i][j].X, (int)points[i][j].Y), radius, colour, -1);
                        }
                    }
                }
                return newImage;
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IList<Point2f[]>, IplImage>> source)
        {
            return source.Select(value =>
            {
                IList<Point2f[]> points = value.Item1;
                IplImage image = value.Item2;
                Scalar colour = Colour;
                int radius = Radius;
                int thickness = Thickness;
                bool fill = Fill;
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
                for (int i = 0; i < points.Count; i++)
                {
                    for (int j = 0; j < points[i].Length; j++)
                    {
                        if (!fill)
                        {
                            CV.Circle(newImage, new Point((int)points[i][j].X, (int)points[i][j].Y), radius, colour, thickness);
                        }
                        else
                        {
                            CV.Circle(newImage, new Point((int)points[i][j].X, (int)points[i][j].Y), radius, colour, -1);
                        }
                    }
                }
                return newImage;
            });
        }
    }
}
