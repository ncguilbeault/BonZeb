using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;

namespace Bonsai.TailTracking
{
    public unsafe class CropImage : Transform<IplImage, IplImage>
    {
        [Description("The region of interest inside the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect RegionOfInterest { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                int frameWidth = value.Size.Width;
                int frameHeight = value.Size.Height;
                byte[] frameData = new byte[frameWidth * frameHeight];
                Marshal.Copy(value.ImageData, frameData, 0, frameWidth * frameHeight);
                IplImage newFrame;
                byte[] newFrameData;

                Rect regionOfInterest = RegionOfInterest;
                if (regionOfInterest.Width > 0 && regionOfInterest.Height > 0)
                {
                    int widthStep = regionOfInterest.Width % 4 == 0 ? regionOfInterest.Width : (int) Math.Ceiling((decimal) regionOfInterest.Width / 4) * 4;
                    newFrameData = new byte[widthStep * regionOfInterest.Height];
                    for (int i = 0; i < regionOfInterest.Height; i++)
                    {
                        for (int j = 0; j < regionOfInterest.Width; j++)
                        {
                            newFrameData[j + (i * widthStep)] = frameData[(regionOfInterest.Y * frameWidth) + (i * frameWidth) + regionOfInterest.X + j];
                        }
                    }
                }
                fixed (byte* bytePointer = newFrameData)
                {
                    IntPtr newFramePtrData = (IntPtr)bytePointer;
                    newFrame = new IplImage(new Size(regionOfInterest.Width, regionOfInterest.Height), IplDepth.U8, 1, newFramePtrData);
                }
                return newFrame;
            });
        }
    }
}