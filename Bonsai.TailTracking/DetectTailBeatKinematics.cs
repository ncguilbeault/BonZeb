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

        private bool findMax;
        private bool prevFindMax;
        private bool boutDetected;
        private int startCounter;
        private int prevCounter;
        private bool firstPeak;
        private double minVal;
        private double maxVal;
        private double frequency;
        private double amplitude;

        public override IObservable<TailKinematics> Process(IObservable<double> source)
        {
            findMax = true;
            prevFindMax = true;
            boutDetected = false;
            startCounter = 0;
            prevCounter = 0;
            firstPeak = true;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            frequency = 0;
            amplitude = 0;
            return source.Select(value => DetectTailKinematicsFunc(value));
        }

        public IObservable<TailKinematics> Process(IObservable<double[]> source)
        {
            findMax = true;
            prevFindMax = true;
            boutDetected = false;
            startCounter = 0;
            prevCounter = 0;
            firstPeak = true;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            frequency = 0;
            amplitude = 0;
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
                        startCounter = 0;
                        if (firstPeak)
                        {
                            firstPeak = false;
                        }
                        else
                        {
                            if (prevFindMax != findMax && startCounter == 0 && prevCounter != startCounter)
                            {
                                frequency = frameRate / (2.0 * prevCounter);
                            }
                        }
                    }
                }
                else if (findMax && (value < (maxVal - delta)))
                {
                    minVal = value;
                    if (findMax)
                    {
                        findMax = false;
                        startCounter = 0;
                        if (firstPeak)
                        {
                            firstPeak = false;
                        }
                        else
                        {
                            if (prevFindMax != findMax && startCounter == 0 && prevCounter != startCounter)
                            {
                                frequency = frameRate / (2.0 * prevCounter);
                            }
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
                startCounter++;
            }
            else
            {
                if ((value > (minVal + boutThreshold)) || (value < (maxVal - boutThreshold)))
                {
                    boutDetected = true;
                    startCounter = 1;
                    if (value < (maxVal - boutThreshold))
                    {
                        findMax = false;
                    }
                }
            }
            if (startCounter > frameWindow)
            {
                //Reset values
                maxVal = double.NegativeInfinity;
                minVal = double.PositiveInfinity;
                boutDetected = false;
                findMax = true;
                firstPeak = true;
                startCounter = 0;
                frequency = 0;
                amplitude = 0;
            }

            prevFindMax = findMax;
            prevCounter = startCounter;

            return new TailKinematics(frequency, amplitude, boutDetected);
        }
    }
}
