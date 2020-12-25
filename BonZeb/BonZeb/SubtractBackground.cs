using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using Bonsai;

namespace BonZeb
{

    [Description("Calculates the background using a method of comparing individual pixel values over time and maintaining the pixel-wise extrema. Input image must contain only a single channel.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class SubtractBackground : Transform<IplImage, BackgroundSubtractionData>
    {

        [Description("Method to use for comparing pixels. Darkest maintains the darkest values for each pixel. Brightest maintains the brightest values for each pixel.")]
        public PixelSearchMethod PixelSearch { get; set; }

        private int noiseThreshold;
        [Description("Noise threshold used to check if current pixel value deviates far enough from the background value.")]
        public int NoiseThreshold { get => noiseThreshold; set => noiseThreshold = value > 0 ? value : 0; }

        public override IObservable<BackgroundSubtractionData> Process(IObservable<IplImage> source)
        {
            IplImage background = null;

            return source.Select(value =>
            {
                IplImage image = new IplImage(value.Size, value.Depth, 1);
                IplImage backgroundSubtractedImage = new IplImage(value.Size, value.Depth, 1);
                IplImage mask = new IplImage(value.Size, value.Depth, 1);
                if (value.Channels != 1)
                {
                    CV.CvtColor(value, image, ColorConversion.Bgr2Gray);
                }
                else
                {
                    CV.Copy(value, image);
                }
                if (background == null)
                {
                    background = image.Clone();
                }
                else
                {
                    if (PixelSearch == PixelSearchMethod.Brightest)
                    {
                        CV.Sub(image, background, backgroundSubtractedImage);
                        if (noiseThreshold != 0)
                        {
                            CV.SubS(backgroundSubtractedImage, new Scalar(noiseThreshold), mask);
                            CV.Add(backgroundSubtractedImage, background, background, mask);
                        }
                        else
                        {
                            CV.Add(backgroundSubtractedImage, background, background, backgroundSubtractedImage);
                        }
                    }
                    else
                    {
                        CV.Sub(background, image, backgroundSubtractedImage);
                        if (noiseThreshold != 0)
                        {
                            CV.SubS(backgroundSubtractedImage, new Scalar(noiseThreshold), mask);
                            CV.Sub(background, backgroundSubtractedImage, background, mask);
                        }
                        else
                        {
                            CV.Sub(background, backgroundSubtractedImage, background, backgroundSubtractedImage);
                        }
                    }
                    CV.AbsDiff(image, background, backgroundSubtractedImage);
                }
                return new BackgroundSubtractionData(image, background, backgroundSubtractedImage);
            });

        }

        public IObservable<BackgroundSubtractionData> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(value =>
            {
                IplImage backgroundSubtractedImage = new IplImage(value.Item1.Size, value.Item1.Depth, 1);
                IplImage mask = new IplImage(value.Item1.Size, value.Item1.Depth, 1);
                if (PixelSearch == PixelSearchMethod.Brightest)
                {
                    CV.Sub(value.Item1, value.Item2, backgroundSubtractedImage);
                    if (noiseThreshold != 0)
                    {
                        CV.SubS(backgroundSubtractedImage, new Scalar(noiseThreshold), mask);
                        CV.Add(backgroundSubtractedImage, value.Item2, value.Item2, mask);
                    }
                    else
                    {
                        CV.Add(backgroundSubtractedImage, value.Item2, value.Item2, backgroundSubtractedImage);
                    }
                }
                else
                {
                    CV.Sub(value.Item2, value.Item1, backgroundSubtractedImage);
                    if (noiseThreshold != 0)
                    {
                        CV.SubS(backgroundSubtractedImage, new Scalar(noiseThreshold), mask);
                        CV.Sub(value.Item2, backgroundSubtractedImage, value.Item2, mask);
                    }
                    else
                    {
                        CV.Sub(value.Item2, backgroundSubtractedImage, value.Item2, backgroundSubtractedImage);
                    }
                }
                CV.AbsDiff(value.Item1, value.Item2, backgroundSubtractedImage);
                return new BackgroundSubtractionData(value.Item1, value.Item2, backgroundSubtractedImage);
            });
        }
    }
}
