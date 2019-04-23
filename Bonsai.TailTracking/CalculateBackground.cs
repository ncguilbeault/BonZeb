using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Calculates the ongoing background of a video using the pixel search method.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public unsafe class CalculateBackground : Transform<IplImage, IplImage>
    {

        [Description("Method to use when calculating background. Darkest searches for the darkest pixels in the image whereas brightest searches for the brightest pixels.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        [Description("Resets the background when the workflow has started.")]
        public bool ResetBackground { get; set; }

        private byte[] background = new byte[0];

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            if (ResetBackground)
            {
                background = new byte[0];
            }        
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
                            if ((((int) frameData[j + (i * frameWidthStep)] > (int) background[j + (i * frameWidthStep)]) && pixelSearch == Utilities.PixelSearch.Brightest) || (((int) frameData[j + (i * frameWidthStep)] < (int) background[j + (i * frameWidthStep)]) && pixelSearch == Utilities.PixelSearch.Darkest))
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
