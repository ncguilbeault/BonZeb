using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.TailTracking
{

    [Description("Detects tail beat frequency from tail curvature using a peak signal detection method to determine the time between successive positive peaks.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DetectTailBeatKinematics : Transform<double, TailKinematics>
    {

        public DetectTailBeatKinematics()
        {
            BoutThreshold = 10;
            FrameRate = 30;
            FrameWindow = 5;
        }

        private double boutThreshold;
        [Description("BoutThreshold is used to determine how much of a threshold is necessary to determine the start of a bout signal. Value must be greater than 0.")]
        public double BoutThreshold { get => boutThreshold; set => boutThreshold = value > 0 ? value : boutThreshold; }

        private double frameRate;
        [Description("Frame rate of the camera or video. Used to determine the tail beat frequency.")]
        public double FrameRate { get => frameRate; set => frameRate = value > 0 ? value : frameRate; }

        private int frameWindow;
        [Description("Frame window is used to determine the window in which to continue detecting successive peaks. A shorter frame window causes the peak detection method to reset more frequently.")]
        public int FrameWindow { get => frameWindow; set => frameWindow = value > 0 ? value : frameWindow; }

        private double? peakThreshold;
        private double delta;
        [Description("PeakThreshold is used to determine how much of a threshold is necessary to detect a new peak in the signal once a bout signal has been detected. If no value is given, peak threshold is set equal to the bout threshold.")]
        public double? PeakThreshold { get => peakThreshold; set { if (value.HasValue && value > 0) { peakThreshold = value; delta = value.Value; } else { peakThreshold = null; delta = boutThreshold; } } }

        private double? withinBoutThreshold;
        private double withinBoutDelta;
        [Description("WithinBoutThreshold is used to determine how much of a threshold is needed to maintain a bout. If no value is given, within bout threshold is set equal to the bout threshold.")]
        public double? WithinBoutThreshold { get => withinBoutThreshold; set { if (value.HasValue && value > 0) { withinBoutThreshold = value; withinBoutDelta = value.Value; } else { withinBoutThreshold = null; withinBoutDelta = boutThreshold; } } }

        private bool findMax;
        private bool boutDetected;
        private int boutCounter;
        private int startPeakCounter;
        private int prevPeakCounter;
        private bool firstPeak;
        private double minVal;
        private double maxVal;
        private double frequency;
        private double amplitude;
        private double[] valWindow;
        private double sumValDiff;

        public override IObservable<TailKinematics> Process(IObservable<double> source)
        {
            findMax = true;
            boutDetected = false;
            boutCounter = 0;
            startPeakCounter = 0;
            prevPeakCounter = 0;
            firstPeak = true;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            frequency = 0;
            amplitude = 0;
            valWindow = new double[frameWindow];
            return source.Select(value => DetectTailKinematicsFunc(value));
        }

        public IObservable<TailKinematics> Process(IObservable<double[]> source)
        {
            findMax = true;
            boutDetected = false;
            boutCounter = 0;
            startPeakCounter = 0;
            prevPeakCounter = 0;
            firstPeak = true;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            frequency = 0;
            amplitude = 0;
            valWindow = new double[frameWindow];
            return source.Select(value => DetectTailKinematicsFunc(Utilities.CalculateMean(value)));
        }

        private TailKinematics DetectTailKinematicsFunc(double value)
        {

            if (value > maxVal)
            {
                maxVal = value;
            }
            if (value < minVal)
            {
                minVal = value;
            }
            if (boutDetected)
            {
                if (!findMax && (value > (minVal + delta)))
                {
                    maxVal = value;
                    if (!findMax)
                    {
                        findMax = true;
                        startPeakCounter = 0;
                        if (firstPeak)
                        {
                            firstPeak = false;
                        }
                        else
                        {
                            frequency = frameRate / (2.0 * prevPeakCounter);
                        }
                    }
                }
                else if (findMax && (value < (maxVal - delta)))
                {
                    minVal = value;
                    if (findMax)
                    {
                        findMax = false;
                        startPeakCounter = 0;
                        if (firstPeak)
                        {
                            firstPeak = false;
                        }
                        else
                        {
                            frequency = frameRate / (2.0 * prevPeakCounter);
                        }
                    }
                }
                if (!firstPeak)
                {
                    if (findMax)
                    {
                        amplitude = minVal;
                    }
                    else
                    {
                        amplitude = maxVal;
                    }
                }
                startPeakCounter++;
                sumValDiff = 0;
                for (int i = 0; i < valWindow.Length - 1; i++)
                {
                    sumValDiff += Math.Abs(valWindow[i + 1] - valWindow[i]);
                    valWindow[i] = valWindow[i + 1];
                }
                sumValDiff += Math.Abs(value - valWindow[valWindow.Length - 1]);
                valWindow[valWindow.Length - 1] = value;

                if (sumValDiff > withinBoutDelta)
                {
                    boutCounter = 0;
                }
                else
                {
                    boutCounter++;
                }
            }
            else
            {
                if ((value > (minVal + boutThreshold)) || (value < (maxVal - boutThreshold)))
                {
                    boutDetected = true;
                    boutCounter = 1;
                    startPeakCounter = 1;
                    if (value < (maxVal - boutThreshold))
                    {
                        findMax = false;
                    }
                }
            }
            if (boutCounter > frameWindow)
            {
                //Reset values
                maxVal = double.NegativeInfinity;
                minVal = double.PositiveInfinity;
                boutDetected = false;
                findMax = true;
                firstPeak = true;
                boutCounter = 0;
                startPeakCounter = 0;
                frequency = 0;
                amplitude = 0;
                valWindow = new double[frameWindow];
            }

            prevPeakCounter = startPeakCounter;

            return new TailKinematics(frequency, amplitude, boutDetected);
        }
    }
}
