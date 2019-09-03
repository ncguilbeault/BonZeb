using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the background using a method of comparing individual pixel values over time and maintaining the pixel-wise extrema.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateBackground : Transform<IplImage, IplImage>
    {

        [Description("Method to use for comparing pixels. Darkest maintains the darkest values for each pixel are maintained. Brightest maintains the brightest values for each pixel are maintained.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        private int noiseThreshold;
        [Description("Noise threshold that is used to check if the current pixel value is sufficiently different from the background value.")]
        public int NoiseThreshold { get => noiseThreshold; set => noiseThreshold = value > 0 ? value : 0; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            IplImage background = null;

            if (PixelSearch == Utilities.PixelSearch.Brightest)
            {
                return source.Select(value =>
                {
                    if (background == null)
                    {
                        background = value.Clone();
                    }
                    else
                    {
                        for (int i = 0; i < value.Height; i++)
                        {
                            for (int j = 0; j < value.Width; j++)
                            {
                                if (value[j, i].Val0 > background[j, i].Val0 + noiseThreshold)
                                {
                                    background[j, i] = value[j, i];
                                }
                            }
                        }
                    }
                    return background;
                });
            }
            else if (PixelSearch == Utilities.PixelSearch.Darkest)
            {
                return source.Select(value =>
                {
                    if (background == null)
                    {
                        background = value.Clone();
                    }
                    else
                    {
                        for (int i = 0; i < value.Height; i++)
                        {
                            for (int j = 0; j < value.Width; j++)
                            {
                                if (value[j, i].Val0 < background[j, i].Val0 - noiseThreshold)
                                {
                                    background[j, i] = value[j, i];
                                }
                            }
                        }
                    }
                    return background;
                });
            }
            return null;
        }
    }
}
