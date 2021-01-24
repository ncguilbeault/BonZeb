using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace BonZeb
{

    [Description("Calculates the background using a method of comparing individual pixel values over time and maintaining the pixel-wise extrema. Input image must contain only a single channel.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateBackground : Transform<IplImage, IplImage>
    {

        [Description("Method to use for comparing pixels. Darkest maintains the darkest values for each pixel. Brightest maintains the brightest values for each pixel.")]
        public PixelSearchMethod PixelSearch { get; set; }

        private int noiseThreshold;
        [Description("Noise threshold used to check if current pixel value deviates far enough from the background value.")]
        public int NoiseThreshold { get => noiseThreshold; set => noiseThreshold = value > 0 ? value : 0; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            IplImage background = null;

            return source.Select(value =>
            {
                IplImage image = new IplImage(value.Size, value.Depth, 1);
                if (value.Channels != 1)
                {
                    CV.CvtColor(value, image, ColorConversion.Bgr2Gray);
                }
                else
                {
                    image = value.Clone();
                }
                if (background == null)
                {
                    if (value.Channels != 1)
                    {
                        background = new IplImage(value.Size, value.Depth, 1);
                        CV.CvtColor(value, background, ColorConversion.Bgr2Gray);
                    }
                    else
                    {
                        background = value.Clone();
                    }
                }
                else
                {
                    IplImage temp = new IplImage(image.Size, image.Depth, image.Channels);
                    if (PixelSearch == PixelSearchMethod.Brightest)
                    {
                        CV.Sub(image, background, temp);
                        if (noiseThreshold != 0)
                        {
                            CV.SubS(temp, Scalar.All(noiseThreshold), image);
                            CV.Add(temp, background, background, image);
                        }
                        else
                        {
                            CV.Add(temp, background, background, temp);
                        }
                    }
                    else
                    {
                        CV.Sub(background, image, temp);
                        if (noiseThreshold != 0)
                        {
                            CV.SubS(temp, Scalar.All(noiseThreshold), image);
                            CV.Sub(background, temp, background, image);
                        }
                        else
                        {
                            CV.Sub(background, temp, background, temp);
                        }
                    }
                }
                return background;
            });

        }
    }
}
