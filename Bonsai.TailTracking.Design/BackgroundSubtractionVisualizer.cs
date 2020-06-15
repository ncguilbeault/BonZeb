using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using Bonsai.TailTracking;
using Bonsai.TailTracking.Design;
using OpenCV.Net;
using System.Reflection;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(BackgroundSubtractionVisualizer), Target = typeof(BackgroundSubtractionData))]

namespace Bonsai.TailTracking.Design
{
    public class BackgroundSubtractionVisualizer : IplImageVisualizer
    {
        BackgroundSubtractionData backgroundSubtractionData;
        MultipleImageViewer imageViewer;
        IplImage labelImage;
        IplImageTexture labelTexture;

        public override VisualizerCanvas VisualizerCanvas
        {
            get { return imageViewer?.Canvas; }
        }

        public override void Load(IServiceProvider provider)
        {
            imageViewer = new MultipleImageViewer { Dock = DockStyle.Fill };
            imageViewer.PopulateComboBoxItems<BackgroundSubtractionData>();
            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(imageViewer);
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
            backgroundSubtractionData = (BackgroundSubtractionData)value;
            if (backgroundSubtractionData != null)
            {
                if (imageViewer.SelectedImageViewer == nameof(backgroundSubtractionData.BackgroundSubtractedImage) && backgroundSubtractionData.BackgroundSubtractedImage != null)
                {
                    imageViewer.Update(backgroundSubtractionData.BackgroundSubtractedImage);
                }
                else if (imageViewer.SelectedImageViewer == nameof(backgroundSubtractionData.Background) && backgroundSubtractionData.Background != null)
                {
                    imageViewer.Update(backgroundSubtractionData.Background);
                }
                else
                {
                    if (backgroundSubtractionData.Image != null) imageViewer.Update(backgroundSubtractionData.Image);
                }
            }
        }

        protected override void RenderFrame()
        {
            GL.Color4(Color4.White);

            if (backgroundSubtractionData != null)
            {
                if (backgroundSubtractionData.Image != null)
                {
                    if (labelImage == null || labelImage.Size != backgroundSubtractionData.Image.Size)
                    {
                        labelImage = new IplImage(backgroundSubtractionData.Image.Size, IplDepth.U8, 4);
                    }
                }

                labelImage.SetZero();
                GL.Disable(EnableCap.Texture2D);
                using (var labelBitmap = new Bitmap(labelImage.Width, labelImage.Height, labelImage.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, labelImage.ImageData))
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    GL.Enable(EnableCap.Texture2D);
                    labelTexture.Update(labelImage);
                    labelTexture.Draw();
                }
            }
        }
        public override void Unload()
        {
            base.Unload();
            imageViewer.Dispose();
            imageViewer = null;
        }
    }
}
