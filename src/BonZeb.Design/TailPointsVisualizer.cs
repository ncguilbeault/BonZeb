using Bonsai;
using Bonsai.Design;
using Bonsai.Vision;
using Bonsai.Vision.Design;
using BonZeb;
using BonZeb.Design;
using OpenCV.Net;
using System;

[assembly: TypeVisualizer(typeof(TailPointsVisualizer), Target = typeof(TailPointData))]
[assembly: TypeVisualizer(typeof(TailPointsVisualizer), Target = typeof(TailPointData[]))]
[assembly: TypeVisualizer(typeof(TailPointsOverlay), Target = typeof(MashupSource<ImageMashupVisualizer, TailPointsVisualizer>))]

namespace BonZeb.Design
{
    // Shows the image carried by the tail data with the tail points drawn over it.
    public class TailPointsVisualizer : IplImageVisualizer
    {
        IplImage canvas;

        public override void Show(object value)
        {
            if (DesignHelper.TryGetTails(value, out Point2f[][] tails, out IplImage image, out bool multipleAnimals) && image != null)
            {
                canvas = IplImageHelper.EnsureColorCopy(canvas, image);
                DesignHelper.DrawTailPoints(canvas, tails, multipleAnimals);
                base.Show(canvas);
            }
        }

        public override void Unload()
        {
            base.Unload();
            canvas = null;
        }
    }

    // Draws the tail points over the image of another image visualizer when used as a mashup.
    public class TailPointsOverlay : DialogTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Load(IServiceProvider provider)
        {
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(MashupVisualizer));
        }

        public override void Show(object value)
        {
            IplImage image = visualizer?.VisualizerImage;
            if (image != null && DesignHelper.TryGetTails(value, out Point2f[][] tails, out _, out bool multipleAnimals))
            {
                DesignHelper.DrawTailPoints(image, tails, multipleAnimals);
            }
        }

        public override void Unload()
        {
            visualizer = null;
        }
    }
}
