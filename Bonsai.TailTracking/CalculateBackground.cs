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

        [Description("Noise threshold that is used to check if the current pixel value is sufficiently different from the background value.")]
        public int NoiseThreshold { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            Utilities.PixelSearch pixelSearch = PixelSearch;
            int noiseThreshold = NoiseThreshold;
            IplImage background = null;

            if (pixelSearch == Utilities.PixelSearch.Brightest)
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
            else if (pixelSearch == Utilities.PixelSearch.Darkest)
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
