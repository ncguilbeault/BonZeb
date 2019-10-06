using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.TailTracking
{

    [Description("Detects bouts from tail curvature using a peak signal detection method.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DetectBout : Transform<double, int>
    {

        [Description("Delta is used to determine how much of a threshold is necessary to determine a peak in an ongoing signal.")]
        public double Delta { get; set; }

        [Description("Frame window is used to determine the window in which to continue detecting successive peaks. A shorter frame window causes the peak detection method to reset.")]
        public int FrameWindow { get; set; }

        private int i = 0;
        private double[] peaks = new double[2];
        private bool findMax = true;
        private double minVal = double.PositiveInfinity;
        private double maxVal = double.NegativeInfinity;
        private int maxPos = 0;
        private int minPos = 0;
        private int detection = 0;

        public override IObservable<int> Process(IObservable<double> source)
        {
            return source.Select(value =>
            {
                i++;
                if ((peaks.Length == 1 && (i - peaks[0]) > FrameWindow) || (peaks.Length == 2 && (i - peaks[1]) > FrameWindow))
                {
                    Array.Clear(peaks, 0, peaks.Length);
                    findMax = true;
                    minVal = double.PositiveInfinity;
                    maxVal = double.NegativeInfinity;
                    detection = 0;
                    maxPos = 0;
                    minPos = 0;
                    i = 0;
                }
                if (value > maxVal)
                {
                    maxVal = value;
                    maxPos = i;
                }
                if (value < minVal)
                {
                    minVal = value;
                    minPos = i;
                }
                if (findMax)
                {
                    if (value < (maxVal - Delta))
                    {
                        if (peaks.Length == 0)
                        {
                            peaks[0] = maxPos;
                        }
                        else if (peaks.Length == 1)
                        {
                            peaks[1] = maxPos;
                            detection = 1;
                        }
                        else
                        {
                            peaks[0] = peaks[1];
                            peaks[1] = maxPos;
                            detection = 1;
                        }
                        minVal = value;
                        minPos = i;
                        findMax = false;
                    }
                }
                else
                {
                    if (value > (minVal + Delta))
                    {
                        if (peaks.Length == 0)
                        {
                            peaks[0] = minPos;
                        }
                        else if (peaks.Length == 1)
                        {
                            peaks[1] = minPos;
                            detection = 1;
                        }
                        else
                        {
                            peaks[0] = peaks[1];
                            peaks[1] = minPos;
                            detection = 1;
                        }
                        maxVal = value;
                        maxPos = i;
                        findMax = true;
                    }
                }
                if (detection == double.PositiveInfinity || detection == double.NegativeInfinity)
                {
                    detection = 0;
                }
                return detection;
            });
        }
    }
}
