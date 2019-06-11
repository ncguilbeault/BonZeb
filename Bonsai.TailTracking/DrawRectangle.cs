using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.TailTracking
{

    [Description("Draws a rectangle and creates draw parameters.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DrawRectangle : Transform<IplImage, Utilities.DrawParameters>
    {
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        [Description("Region in the input image used for drawing rectangular region of interest.")]
        public Rect RegionOfInterest { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Colour used for drawing rectangular region of interest.")]
        public Scalar Colour { get; set; }

        public override IObservable<Utilities.DrawParameters> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                double xRange = RegionOfInterest.Width > 0 ? (double)RegionOfInterest.Width / (double)value.Width : 1;
                double xOffset = xRange < 1 ? ((double)RegionOfInterest.X - (((double)value.Width - (double)RegionOfInterest.Width) / 2)) * 2 / (double)value.Width : 0;
                double yRange = RegionOfInterest.Height > 0 ? (double)RegionOfInterest.Height / (double)value.Height : 1;
                double yOffset = yRange < 1 ? ((((double)value.Height - (double)RegionOfInterest.Height) / 2) - (double)RegionOfInterest.Y) * 2 / (double)value.Height : 0;
                return new Utilities.DrawParameters(xOffset, yOffset, xRange, yRange, Colour);
            });
        }
    }
}
