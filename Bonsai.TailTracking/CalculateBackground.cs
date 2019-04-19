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

    public class CalculateBackground : Transform<IplImage, IplImage>
    {

        [Description("Method to use when calculating background. Darkest searches for the darkest pixels in the image whereas brightest searches for the brightest pixels.")]
        public Utilities.PixelSearch PixelSearch { get; set; }

        private byte[] background = new byte[0];

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                Utilities.PixelSearch pixelSearch = PixelSearch;
                int frameWidth = value.Size.Width;
                int frameHeight = value.Size.Height;
                byte[] frameData = new byte[frameWidth * frameHeight];
                Marshal.Copy(value.ImageData, frameData, 0, frameWidth * frameHeight);
                IplImage newBackground;
                if (background.Length == 0)
                {
                    background = new byte[frameWidth * frameHeight];
                    for (int i = 0; i < frameData.Length; i++)
                    {
                        background[i] = frameData[i];
                    }
                }
                else
                {
                    for (int i = 0; i < frameData.Length; i++)
                    {
                        if ((((int)frameData[i] > (int)background[i]) && pixelSearch == Utilities.PixelSearch.Brightest) || (((int)frameData[i] < (int)background[i]) && pixelSearch == Utilities.PixelSearch.Darkest))
                        {
                            background[i] = frameData[i];
                        }
                    }
                }

                unsafe
                {
                    fixed (byte* bytePointer = background)
                    {
                        IntPtr backgroundData = (IntPtr) bytePointer;
                        newBackground = new IplImage(new Size(frameWidth, frameHeight), IplDepth.U8, 1, backgroundData);
                    }
                }

                return (newBackground);
            });
        }
    }
}
