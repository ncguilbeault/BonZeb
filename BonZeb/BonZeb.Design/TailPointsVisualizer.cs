﻿using Bonsai;
using Bonsai.Vision.Design;
using BonZeb;
using BonZeb.Design;
using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

[assembly: TypeVisualizer(typeof(TailPointsVisualizer), Target = typeof(TailPoints))]

namespace BonZeb.Design
{
    public class TailPointsVisualizer : IplImageVisualizer
    {
        TailPoints tailPoints;
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
            tailPoints = (TailPoints)value;
            if (tailPoints != null)
            {
                base.Show(tailPoints.Image);
            }
        }

        public static Vector2 NormalizePoint(Point2f point, OpenCV.Net.Size imageSize)
        {
            return new Vector2(
                (point.X * 2f / imageSize.Width) - 1,
                -((point.Y * 2f / imageSize.Height) - 1));
        }

        protected override void RenderFrame()
        {
            GL.Color4(Color4.White);
            base.RenderFrame();
    
            if (tailPoints != null)
            {
                GL.PointSize(5 * VisualizerCanvas.Height / 640f);
                if (labelImage == null || labelImage.Size != tailPoints.Image.Size)
                {
                    labelImage = new IplImage(tailPoints.Image.Size, IplDepth.U8, 4);
                }

                labelImage.SetZero();
                GL.Disable(EnableCap.Texture2D);
                GL.Begin(PrimitiveType.Points);
                using (var labelBitmap = new Bitmap(labelImage.Width, labelImage.Height, labelImage.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, labelImage.ImageData))
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    try
                    {
                        for (int i = 0; i < tailPoints.Points.Length; i++)
                        {
                            GL.Color3(1.0, 0.0, 0.0);
                            GL.Vertex2(NormalizePoint(tailPoints.Points[i], tailPoints.Image.Size));
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
    }
}
