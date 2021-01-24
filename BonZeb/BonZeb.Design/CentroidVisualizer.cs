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

[assembly: TypeVisualizer(typeof(CentroidVisualizer), Target = typeof(CentroidData))]

namespace BonZeb.Design
{
    public class CentroidVisualizer : IplImageVisualizer
    {
        CentroidData centroidData;
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
            imageViewer.PopulateMultipleImageViewer<CentroidData>();
            imageViewer.AddItemToMultipleImageViewer(nameof(centroidData.LargestContour));
            imageViewer.MaximizeDropDownMenuWidth();
            imageViewer.SelectImageIndex(0);
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
            centroidData = (CentroidData)value;
            if (centroidData != null)
            {
                if (imageViewer.SelectedImageViewer == nameof(centroidData.BackgroundSubtractedImage) && centroidData.BackgroundSubtractedImage != null)
                {
                    imageViewer.Update(centroidData.BackgroundSubtractedImage);
                }
                else if (imageViewer.SelectedImageViewer == nameof(centroidData.ThresholdImage) && centroidData.ThresholdImage != null)
                {
                    imageViewer.Update(centroidData.ThresholdImage);
                }
                else if (imageViewer.SelectedImageViewer == nameof(centroidData.LargestContour) && centroidData.ThresholdImage != null)
                {
                    IplImage contourImage = new IplImage(centroidData.ThresholdImage.Size, centroidData.ThresholdImage.Depth, 3);
                    contourImage.SetZero();
                    if (centroidData.LargestContour != null)
                    {
                        CV.DrawContours(contourImage, centroidData.LargestContour.Contour, Scalar.All(255), Scalar.All(0), 0, -1);
                        CV.DrawContours(contourImage, centroidData.LargestContour.Contour, new Scalar(0, 0, 255), Scalar.All(0), 0, 1 * VisualizerCanvas.Height / 640);
                    }
                    imageViewer.Update(contourImage);
                }
                else
                {
                    if (imageViewer.SelectedImageIndex != 0)
                    {
                        imageViewer.SelectImageIndex(0);
                    }
                    imageViewer.Update(centroidData.Image);
                }
            }
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
                    GL.Vertex2(Utilities.NormalizePoint(centroidData.Centroid, centroidData.Image.Size));
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
            imageViewer.Dispose();
            imageViewer = null;
        }
    }
}