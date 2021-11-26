using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using BonZeb;
using BonZeb.Design;
using OpenCV.Net;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

[assembly: TypeVisualizer(typeof(MultiAnimalTrackingVisualizer), Target = typeof(MultiAnimalTrackingData))]

namespace BonZeb.Design
{
    public class MultiAnimalTrackingVisualizer : IplImageVisualizer
    {
        MultiAnimalTrackingData multiAnimalTrackingData;
        MultipleImageViewer imageViewer;
        IplImage labelImage;
        IplImageTexture labelTexture;
        Random rnd = new Random(0);
        List<Color> randomColors = new List<Color>();

        public override VisualizerCanvas VisualizerCanvas
        {
            get { return imageViewer?.Canvas; }
        }

        public override void Load(IServiceProvider provider)
        {
            imageViewer = new MultipleImageViewer { Dock = DockStyle.Fill };
            imageViewer.PopulateMultipleImageViewer<MultiAnimalTrackingData>();
            imageViewer.MaximizeDropDownMenuWidth();
            imageViewer.SelectImageIndex(0);
            Random rnd = new Random();
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
            multiAnimalTrackingData = (MultiAnimalTrackingData)value;
            if (imageViewer.SelectedImageViewer == nameof(multiAnimalTrackingData.BackgroundSubtractedImage) && multiAnimalTrackingData.BackgroundSubtractedImage != null)
            {
                imageViewer.Update(multiAnimalTrackingData.BackgroundSubtractedImage);
            }
            else if (imageViewer.SelectedImageViewer == nameof(multiAnimalTrackingData.ThresholdImage) && multiAnimalTrackingData.ThresholdImage != null)
            {
                imageViewer.Update(multiAnimalTrackingData.ThresholdImage);
            }
            else
            {
                if (multiAnimalTrackingData.Image != null) imageViewer.Update(multiAnimalTrackingData.Image);
            }
        }

        protected override void RenderFrame()
        {
            GL.Color4(Color4.White);

            if (multiAnimalTrackingData != null && multiAnimalTrackingData.Image != null && multiAnimalTrackingData.OrderedCentroids != null)
            {
                if (labelImage == null || labelImage.Size != multiAnimalTrackingData.Image.Size)
                {
                    labelImage = new IplImage(multiAnimalTrackingData.Image.Size, IplDepth.U8, 4);
                }
                labelImage.SetZero();
                GL.PointSize(5 * VisualizerCanvas.Height / 640f);
                GL.Disable(EnableCap.Texture2D);
                while (randomColors.Count < multiAnimalTrackingData.OrderedCentroids.Length)
                {
                    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                    randomColors.Add(randomColor);
                }
                using (var labelBitmap = new Bitmap(labelImage.Width, labelImage.Height, labelImage.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, labelImage.ImageData))
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    try
                    {
                        for (int i = 0; i < multiAnimalTrackingData.OrderedCentroids.Length; i++)
                        {
                            GL.Color4(randomColors[i].R, randomColors[i].G, randomColors[i].B, randomColors[i].A);
                            //GL.Color4(1.0, 0.0, 0.0, 1.0);
                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex2(Utilities.NormalizePoint(multiAnimalTrackingData.OrderedCentroids[i], multiAnimalTrackingData.Image.Size));
                        }
                    }
                    finally
                    {
                        GL.End();
                        GL.Color4(Color4.White);
                        GL.Enable(EnableCap.Texture2D);
                        labelTexture.Update(labelImage);
                        labelTexture.Draw();
                    }
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