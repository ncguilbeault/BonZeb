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
    public class MaskImage : Transform<IplImage, IplImage>
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
                byte[] frameData = new byte[frameWidth * frameHeight];
                Marshal.Copy(value.ImageData, frameData, 0, frameWidth * frameHeight);
                IplImage newFrame = value.Clone();

                Rect regionOfInterest = RegionOfInterest;
                if (regionOfInterest.Width > 0 && regionOfInterest.Height > 0)
                {
                    byte[] newFrameData = new byte[frameWidth * frameHeight];
                    for (int i = 0; i < frameHeight; i++)
                    {
                        for (int j = 0; j < frameWidth; j++)
                        {
                            if (i >= regionOfInterest.Y && i <= (regionOfInterest.Y + regionOfInterest.Height) && j >= regionOfInterest.X && j <= (regionOfInterest.X + regionOfInterest.Width))
                            {
                                newFrameData[(i * frameWidth) + j] = frameData[(i * frameWidth) + j];
                            }
                            else
                            {
                                newFrameData[(i * frameWidth) + j] = (byte) Colour.Val0;
                            }
                        }
                    }
                    unsafe
                    {
                        fixed (byte* bytePointer = newFrameData)
                        {
                            IntPtr newFramePtrData = (IntPtr)bytePointer;
                            newFrame = new IplImage(new OpenCV.Net.Size(frameWidth, frameHeight), IplDepth.U8, 1, newFramePtrData);
                        }
                    }
                }
                return newFrame;
            });
        }
    }
}
