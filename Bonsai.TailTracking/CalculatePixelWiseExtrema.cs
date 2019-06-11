using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the pixel-wise extrema over time using the pixel search method.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public unsafe class CalculatePixelWiseExtrema : Transform<IplImage, IplImage>
    {

        [Description("Method to use when calculating pixel-wise extrema. Darkest maintains the darkest values for each pixel over time. Brightest maintains the brightest values for each pixel over time.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            byte[] background = new byte[0];
            return source.Select(value =>
            {
                Utilities.PixelSearch pixelSearch = PixelSearch;
                int frameWidth = value.Size.Width;
                int frameHeight = value.Size.Height;
                int frameWidthStep = value.WidthStep;
                byte[] frameData = new byte[frameWidthStep * frameHeight];
                Marshal.Copy(value.ImageData, frameData, 0, frameWidthStep * frameHeight);
                IplImage newBackground;
                if (background.Length == 0)
                {
                    background = new byte[frameWidthStep * frameHeight];
                    for (int i = 0; i < frameHeight; i++)
                    {
                        for (int j = 0; j < frameWidth; j++)
                        {
                            background[j + (i * frameWidthStep)] = frameData[j + (i * frameWidthStep)];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < frameHeight; i++)
                    {
                        for (int j = 0; j < frameWidth; j++)
                        {
                            if ((pixelSearch == Utilities.PixelSearch.Brightest && (frameData[j + (i * frameWidthStep)] > background[j + (i * frameWidthStep)])) || (pixelSearch == Utilities.PixelSearch.Darkest && (frameData[j + (i * frameWidthStep)] < background[j + (i * frameWidthStep)])))
                            {
                                background[j + (i * frameWidthStep)] = frameData[j + (i * frameWidthStep)];
                            }
                        }
                    }
                }
                fixed (byte* bytePointer = background)
                {
                    IntPtr backgroundData = (IntPtr)bytePointer;
                    newBackground = new IplImage(new Size(frameWidth, frameHeight), IplDepth.U8, 1, backgroundData);
                }
                return (newBackground);
            });
        }
    }
}
