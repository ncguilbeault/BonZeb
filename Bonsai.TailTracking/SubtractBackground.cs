using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
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
                IplImage temp = new IplImage(value.Size, value.Depth, 1);
                IplImage mask = new IplImage(value.Size, value.Depth, 1);
                if (value.Channels != 1)
                {
                    CV.CvtColor(value, temp, ColorConversion.Bgr2Gray);
                }
                else
                {
                    CV.Copy(value, temp);
                }
                if (background == null)
                {
                    background = value.Clone();
                }
                else
                {
                    if (PixelSearch == PixelSearchMethod.Brightest)
                    {
                        CV.Sub(value, background, temp);
                        if (noiseThreshold != 0)
                        {
                            CV.SubS(temp, new Scalar(noiseThreshold), mask);
                            CV.Add(temp, background, background, mask);
                        }
                        else
                        {
                            CV.Add(temp, background, background, temp);
                        }
                    }
                    else
                    {
                        CV.Sub(background, value, temp);
                        if (noiseThreshold != 0)
                        {
                            CV.SubS(temp, new Scalar(noiseThreshold), mask);
                            CV.Sub(background, temp, background, mask);
                        }
                        else
                        {
                            CV.Sub(background, temp, background, temp);
                        }
                    }
                    CV.AbsDiff(value, background, temp);
                }
                return new BackgroundSubtractionData(value, background, temp);
            });

        }
    }
}
