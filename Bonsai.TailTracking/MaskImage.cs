using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bonsai.TailTracking
{
    public unsafe class MaskImage : Transform<IplImage, IplImage>
    {
        [Description("The region of interest inside the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect RegionOfInterest { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Colour used to fill area outside of cropped region.")]
        public Scalar Colour { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                int frameWidth = value.Size.Width;
                int frameHeight = value.Size.Height;
                int frameWidthStep = value.WidthStep;
                byte[] frameData = new byte[frameWidthStep * frameHeight];
                Marshal.Copy(value.ImageData, frameData, 0, frameWidthStep * frameHeight);
                IplImage newFrame;
                byte[] newFrameData;

                Rect regionOfInterest = RegionOfInterest;
                if (regionOfInterest.Width > 0 && regionOfInterest.Height > 0)
                {
                    newFrameData = new byte[frameWidthStep * frameHeight];
                    for (int i = 0; i < frameHeight; i++)
                    {
                        for (int j = 0; j < frameWidth; j++)
                        {
                            if (i >= regionOfInterest.Y && i <= (regionOfInterest.Y + regionOfInterest.Height) && j >= regionOfInterest.X && j <= (regionOfInterest.X + regionOfInterest.Width))
                            {
                                newFrameData[(i * frameWidthStep) + j] = frameData[(i * frameWidthStep) + j];
                            }
                            else
                            {
                                newFrameData[(i * frameWidthStep) + j] = (byte) Colour.Val0;
                            }
                        }
                    }
                }
                fixed (byte* bytePointer = newFrameData)
                {
                    IntPtr newFramePtrData = (IntPtr)bytePointer;
                    newFrame = new IplImage(new OpenCV.Net.Size(frameWidth, frameHeight), IplDepth.U8, 1, newFramePtrData);
                }
                return newFrame;
            });
        }
    }
}
