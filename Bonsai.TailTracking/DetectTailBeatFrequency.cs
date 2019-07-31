using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.TailTracking
{

    [Description("Detects tail beat frequency from tail curvature using a peak signal detection method to determine the time between successive positive peaks.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DetectTailBeatFrequency : Transform<double, double>
    {

        private double delta;
        [Description("Delta is used to determine how much of a threshold is necessary to determine a peak in an ongoing signal.")]
        public double Delta { get { return delta; } set { delta = value; } }

        private double frameRate;
        [Description("Frame rate of the camera or video. Used to determine the frequency.")]
        public double FrameRate { get { return frameRate; } set { frameRate = value; } }

        private int frameWindow;
        [Description("Frame window is used to determine the window in which to continue detecting successive peaks. A shorter frame window causes the peak detection method to reset.")]
        public int FrameWindow { get { return frameWindow; } set { frameWindow = value; } }

        public override IObservable<double> Process(IObservable<double> source)
        {
            int i = 0;
            double[] peaks = new double[2];
            bool findMax = true;
            double minVal = double.PositiveInfinity;
            double maxVal = double.NegativeInfinity;
            int pos = 0;
            double frequency = 0;
            return source.Select(value => 
            {
                i++;
                if ((peaks.Length == 1 && (i - peaks[0]) > frameWindow) || (peaks.Length == 2 && (i - peaks[1]) > frameWindow))
                {
                    Array.Clear(peaks, 0, peaks.Length);
                    findMax = true;
                    minVal = double.PositiveInfinity;
                    maxVal = double.NegativeInfinity;
                    frequency = 0;
                    pos = 0;
                    i = 0;
                }

                if (value > maxVal)
                {
                    maxVal = value;
                    pos = i;
                }

                if (value < minVal)
                {
                    minVal = value;
                }

                if (findMax)
                {
                    if (value < (maxVal - delta))
                    {
                        if (peaks.Length == 0)
                        {
                            peaks[0] = pos;
                        }
                        else if (peaks.Length == 1)
                        {
                            peaks[1] = pos;
                            frequency = frameRate / (peaks[1] - peaks[0]);
                        }
                        else
                        {
                            peaks[0] = peaks[1];
                            peaks[1] = pos;
                            frequency = frameRate / (peaks[1] - peaks[0]);
                        }
                        minVal = value;
                        findMax = false;
                    }
                }
                else
                {
                    if (value > (minVal + delta))
                    {
                        maxVal = value;
                        findMax = true;
                    }
                }

                if (frequency == double.PositiveInfinity || frequency == double.NegativeInfinity)
                {
                    frequency = 0;
                }

                return frequency;

            });

        }

    }

}
