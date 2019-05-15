using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class DrawRectangle : Transform<IplImage, Utilities.DrawParameters>
    {
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect RegionOfInterest { get; set; }

        public Scalar Fill { get; set; }

        public override IObservable<Utilities.DrawParameters> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                double xRange = RegionOfInterest.Width > 0 ? (double)RegionOfInterest.Width / (double)value.Width : 1;
                double xOffset = xRange < 1 ? ((double)RegionOfInterest.X - (((double)value.Width - (double)RegionOfInterest.Width) / 2)) * 2 / (double)value.Width : 0;
                double yRange = RegionOfInterest.Height > 0 ? (double)RegionOfInterest.Height / (double)value.Height : 1;
                double yOffset = yRange < 1 ? ((((double)value.Height - (double)RegionOfInterest.Height) / 2) - (double)RegionOfInterest.Y) * 2 / (double)value.Height : 0;
                return new Utilities.DrawParameters(xOffset, yOffset, xRange, yRange, Fill);
            });
        }
    }
}
