using Bonsai;
using Bonsai.Design;
using Bonsai.Vision;
using Bonsai.Vision.Design;
using BonZeb;
using BonZeb.Design;
using OpenCV.Net;
using System;

[assembly: TypeVisualizer(typeof(TailAnglesVisualizer), Target = typeof(TailAngleData<double>))]
[assembly: TypeVisualizer(typeof(TailAnglesVisualizer), Target = typeof(TailAngleData<double[]>))]
[assembly: TypeVisualizer(typeof(TailAnglesVisualizer), Target = typeof(TailAngleData<double>[]))]
[assembly: TypeVisualizer(typeof(TailAnglesVisualizer), Target = typeof(TailAngleData<double[]>[]))]
[assembly: TypeVisualizer(typeof(TailAnglesOverlay), Target = typeof(MashupSource<ImageMashupVisualizer, TailAnglesVisualizer>))]

namespace BonZeb.Design
{
    // Shows the image carried by the tail data with the tail segments drawn over it.
    public class TailAnglesVisualizer : IplImageVisualizer
    {
        IplImage canvas;

        public override void Show(object value)
        {
            if (DesignHelper.TryGetTails(value, out Point2f[][] tails, out IplImage image, out bool multipleAnimals) && image != null)
            {
                canvas = IplImageHelper.EnsureColorCopy(canvas, image);
                DesignHelper.DrawTailAngles(canvas, tails, multipleAnimals);
                base.Show(canvas);
            }
        }

        public override void Unload()
        {
            base.Unload();
            canvas = null;
        }
    }

    // Draws the tail segments over the image of another image visualizer when used as a mashup.
    public class TailAnglesOverlay : DialogTypeVisualizer
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
                DesignHelper.DrawTailAngles(image, tails, multipleAnimals);
            }
        }

        public override void Unload()
        {
            visualizer = null;
        }
    }
}
