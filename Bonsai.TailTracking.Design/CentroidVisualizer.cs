using Bonsai;
using Bonsai.Design;
using Bonsai.Vision;
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
using System.Reactive.Linq;

[assembly: TypeVisualizer(typeof(CentroidVisualizer), Target = typeof(CentroidData))]

namespace Bonsai.TailTracking.Design
{
    public class CentroidVisualizer : IplImageVisualizer
    {
        CentroidData centroidData;
        MultipleImageViewer centroidViewer;
        IplImage labelImage;
        IplImageTexture labelTexture;

        public override VisualizerCanvas VisualizerCanvas
        {
            get { return centroidViewer?.Canvas; }
        }

        public override void Load(IServiceProvider provider)
        {
            centroidViewer = new MultipleImageViewer { Dock = DockStyle.Fill };
            centroidViewer.PopulateComboBoxItems<CentroidData>();
            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(centroidViewer);
            }
            VisualizerCanvas.Load += (sender, e) =>
            {
                labelTexture = new IplImageTexture();
                GL.Enable(EnableCap.Blend);
                GL.Enable(EnableCap.PointSmooth);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            };
            VisualizerCanvas.RenderFrame += (sender, e) => RenderFrame();
            base.Load(provider);
        }

        public override void Show(object value)
        {
            centroidData = (CentroidData)value;
            if (centroidData != null)
            {
                if (centroidViewer.SelectedImageViewer == nameof(centroidData.BackgroundSubtractedImage) && centroidData.BackgroundSubtractedImage != null)
                {
                    centroidViewer.Update(centroidData.BackgroundSubtractedImage);
                }
                else if (centroidViewer.SelectedImageViewer == nameof(centroidData.Background) && centroidData.Background != null)
                {
                    centroidViewer.Update(centroidData.Background);
                }
                else if (centroidViewer.SelectedImageViewer == nameof(centroidData.Contours) && centroidData.Contours != null)
                {
                    centroidViewer.Update(centroidData.Contours);
                }
                else if (centroidViewer.SelectedImageViewer == nameof(centroidData.ThresholdImage) && centroidData.ThresholdImage != null)
                {
                    centroidViewer.Update(centroidData.ThresholdImage);
                }
                else
                {
                    centroidViewer.Update(centroidData.Image);
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

            if (centroidData != null && centroidData.Image != null && centroidData.ThresholdImage != null)
            {
                GL.PointSize(5 * VisualizerCanvas.Height / 640f);
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
                    GL.Color4(1.0, 0.0, 0.0, 1.0);
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
            centroidViewer.Dispose();
            centroidViewer = null;
        }
    }
}
