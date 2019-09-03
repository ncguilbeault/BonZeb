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
        private Rect regionOfInterest;
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        [Description("Region in the input image used for drawing rectangular region of interest.")]
        public Rect RegionOfInterest { get { return regionOfInterest; } set { regionOfInterest = value; } }

        private Scalar colour;
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Colour used for drawing rectangular region of interest.")]
        public Scalar Colour { get { return colour; } set { colour = value; } }

        public override IObservable<Utilities.DrawParameters> Process(IObservable<IplImage> source)
        {
            return source.Select(value =>
            {
                double xRange = regionOfInterest.Width > 0 ? regionOfInterest.Width / value.Width : 1;
                double xOffset = xRange < 1 ? (regionOfInterest.X - ((value.Width - regionOfInterest.Width) / 2)) * 2 / value.Width : 0;
                double yRange = regionOfInterest.Height > 0 ? regionOfInterest.Height / value.Height : 1;
                double yOffset = yRange < 1 ? (((value.Height - regionOfInterest.Height) / 2) - regionOfInterest.Y) * 2 / value.Height : 0;
                return new Utilities.DrawParameters(xOffset, yOffset, xRange, yRange, colour);
            });
        }
    }
}
