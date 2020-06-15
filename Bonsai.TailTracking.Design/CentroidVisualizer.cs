using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using Bonsai.TailTracking;
using Bonsai.TailTracking.Design;
using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(CentroidVisualizer), Target = typeof(CentroidData))]

namespace Bonsai.TailTracking.Design
{
    public class CentroidVisualizer : IplImageVisualizer
    {
        CentroidData centroidData;
        ThresholdImageViewer thresholdImageViewer;
        IplImage labelImage;
        IplImageTexture labelTexture;

        public override VisualizerCanvas VisualizerCanvas
        {
            get { return thresholdImageViewer?.Canvas; }
        }

        public override void Load(IServiceProvider provider)
        {
            thresholdImageViewer = new ThresholdImageViewer { Dock = DockStyle.Fill };
            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(thresholdImageViewer);
            }
            base.Load(provider);
            VisualizerCanvas.Load += (sender, e) =>
            {
                labelTexture = new IplImageTexture();
                GL.Enable(EnableCap.Blend);
                GL.Enable(EnableCap.PointSmooth);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            };
        }

        public override void Show(object value)
        {
            centroidData = (CentroidData)value;
            if (centroidData != null && centroidData.Image != null && centroidData.ThresholdImage != null)
            {
                if (thresholdImageViewer.ShowThresholdImage)
                {
                    base.Show(centroidData.ThresholdImage);
                }
                else
                {
                    base.Show(centroidData.Image);
                }
            }
        }

        public static Vector2 NormalizePoint(Point2f point, OpenCV.Net.Size imageSize)
        {
            return new Vector2((point.X * 2f / imageSize.Width) - 1, -((point.Y * 2f / imageSize.Height) - 1));
        }

        protected override void RenderFrame()
        {
            GL.Color4(Color4.White);
            base.RenderFrame();

            if (centroidData != null && centroidData.Image != null && centroidData.ThresholdImage != null)
            {
                GL.PointSize(3 * VisualizerCanvas.Height / 640f);
                if (labelImage == null || labelImage.Size != centroidData.Image.Size)
                {
                    labelImage = new IplImage(centroidData.Image.Size, IplDepth.U8, 4);
                }

                labelImage.SetZero();
                GL.Disable(EnableCap.Texture2D);
                using (var labelBitmap = new Bitmap(labelImage.Width, labelImage.Height, labelImage.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, labelImage.ImageData))
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    GL.Color4(1.0, 0.0, 0.0, 0.5);
                    GL.Begin(PrimitiveType.Points);
                    GL.Vertex2(NormalizePoint(centroidData.Centroid, centroidData.Image.Size));
                    GL.End();
                    GL.Color4(Color4.White);
                    GL.Enable(EnableCap.Texture2D);
                    labelTexture.Update(labelImage);
                    labelTexture.Draw();
                }
            }
        }
        public override void Unload()
        {
            base.Unload();
            thresholdImageViewer.Dispose();
            thresholdImageViewer = null;
        }
    }
}
