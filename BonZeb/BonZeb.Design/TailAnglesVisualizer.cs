using Bonsai;
using Bonsai.Vision.Design;
using BonZeb;
using BonZeb.Design;
using OpenCV.Net;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

[assembly: TypeVisualizer(typeof(TailAnglesVisualizer), Target = typeof(TailAngleData<double>))]
[assembly: TypeVisualizer(typeof(TailAnglesVisualizer), Target = typeof(TailAngleData<double[]>))]

namespace BonZeb.Design
{
    class TailAnglesVisualizer : IplImageVisualizer
    {
        Point2f[] tailPoints = new Point2f[0];
        OpenCV.Net.Size imageSize;
        IplImage labelImage;
        IplImageTexture labelTexture;

        public override void Load(IServiceProvider provider)
        {
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
            if (value != null)
            {
                if (value is TailAngleData<double>)
                {
                    TailAngleData<double> newValue = (TailAngleData<double>)value;
                    if (newValue.Image != null)
                    {
                        tailPoints = newValue.Points;
                        imageSize = newValue.Image.Size;
                        base.Show(newValue.Image);
                    }
                }
                else if (value is TailAngleData<double[]>)
                {
                    TailAngleData<double[]> newValue = (TailAngleData<double[]>)value;
                    if (newValue.Image != null)
                    {
                        tailPoints = newValue.Points;
                        imageSize = newValue.Image.Size;
                        base.Show(newValue.Image);
                    }
                }
            }
            else
            {
                if (tailPoints.Length != 0)
                {
                    tailPoints = new Point2f[0];
                }
            }
        }

        protected override void RenderFrame()
        {
            GL.Color4(Color4.White);
            base.RenderFrame();

            if (tailPoints.Length != 0)
            {
                if (labelImage == null || labelImage.Size != imageSize)
                {
                    labelImage = new IplImage(imageSize, IplDepth.U8, 4);
                }

                labelImage.SetZero();
                GL.Disable(EnableCap.Texture2D);
                using (var labelBitmap = new Bitmap(labelImage.Width, labelImage.Height, labelImage.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, labelImage.ImageData))
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    GL.LineWidth(3 * VisualizerCanvas.Height / 640f);
                    try
                    {
                        for (int i = 0; i < tailPoints.Length - 1; i++)
                        {
                            GL.Begin(PrimitiveType.Lines);
                            GL.Color4(1.0, 0.0, 0.0, 1.0);
                            GL.Vertex2(Utilities.NormalizePointForTailAngle(tailPoints[i], Math.Atan2(tailPoints[i].Y - tailPoints[i + 1].Y, tailPoints[i].X - tailPoints[i + 1].X), imageSize));
                            GL.Vertex2(Utilities.NormalizePointForTailAngle(tailPoints[i + 1], Math.Atan2(tailPoints[i + 1].Y - tailPoints[i].Y, tailPoints[i + 1].X - tailPoints[i].X), imageSize));
                            GL.End();
                        }
                    }
                    finally
                    {
                        GL.Color4(Color4.White);
                        GL.Enable(EnableCap.Texture2D);
                        labelTexture.Update(labelImage);
                        labelTexture.Draw();
                    }
                }
            }
        }
    }
}