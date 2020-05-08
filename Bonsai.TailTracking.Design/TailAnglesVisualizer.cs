using Bonsai;
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

[assembly: TypeVisualizer(typeof(TailAnglesVisualizer), Target = typeof(TailAngles))]

namespace Bonsai.TailTracking.Design
{
    class TailAnglesVisualizer : IplImageVisualizer
    {
        TailAngles tailAngles;
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
        public static Vector2 NormalizePoint(Point2f point, OpenCV.Net.Size imageSize)
        {
            return new Vector2((point.X * 2f / imageSize.Width) - 1, -((point.Y * 2f / imageSize.Height) - 1));
        }
        public static Vector2 NormalizePointForTailAngle(Point2f point, double angle, OpenCV.Net.Size imageSize)
        {
            return new Vector2(((point.X + (float)Math.Cos(angle) * 10f) * 2f / imageSize.Width) - 1, -(((point.Y + (float)Math.Sin(angle) * 10f) * 2f / imageSize.Height) - 1));
        }

        public override void Show(object value)
        {
            tailAngles = (TailAngles)value;
            if (tailAngles != null && tailAngles.Image != null)
            {
                base.Show(tailAngles.Image);
            }
        }
        protected override void RenderFrame()
        {
            GL.Color4(Color4.White);
            base.RenderFrame();

            if (tailAngles != null && tailAngles.Image != null)
            {
                if (labelImage == null || labelImage.Size != tailAngles.Image.Size)
                {
                    labelImage = new IplImage(tailAngles.Image.Size, IplDepth.U8, 4);
                }

                labelImage.SetZero();
                GL.Disable(EnableCap.Texture2D);
                using (var labelBitmap = new Bitmap(labelImage.Width, labelImage.Height, labelImage.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, labelImage.ImageData))
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    GL.LineWidth(2 * VisualizerCanvas.Height / 640f);
                    for (int i = 0; i < tailAngles.Points.Length - 1; i++)
                    {
                        GL.Begin(PrimitiveType.Lines);
                        GL.Color4(1.0, 0.0, 0.0, 0.5);
                        GL.Vertex2(NormalizePointForTailAngle(tailAngles.Points[i], Math.Atan2(tailAngles.Points[i].Y - tailAngles.Points[i + 1].Y, tailAngles.Points[i].X - tailAngles.Points[i + 1].X), tailAngles.Image.Size));
                        GL.Vertex2(NormalizePointForTailAngle(tailAngles.Points[i + 1], Math.Atan2(tailAngles.Points[i + 1].Y - tailAngles.Points[i].Y, tailAngles.Points[i + 1].X - tailAngles.Points[i].X), tailAngles.Image.Size));
                        GL.End();
                    }

                    GL.Color4(Color4.White);
                    GL.Enable(EnableCap.Texture2D);
                    labelTexture.Update(labelImage);
                    labelTexture.Draw();
                }
            }
        }
    }
}
