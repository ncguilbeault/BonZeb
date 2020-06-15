using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using OpenCV.Net;

namespace Bonsai.TailTracking
{
    public class TailAngles : TailPoints
    {
        public double[] Angles;
        public TailAngles(double[] angles, Point2f[] points = null, IplImage image = null) : base(points, image)
        {
            Image = image;
            Points = points;
            Angles = angles;
        }
    }
}
