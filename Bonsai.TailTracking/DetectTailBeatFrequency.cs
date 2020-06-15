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

        public DetectTailBeatFrequency()
        {
            Delta = 10;
            FrameRate = 30;
            FrameWindow = 5;
        }

        private double delta;
        [Description("Delta is used to determine how much of a threshold is necessary to determine a peak in an ongoing signal. Value must be greater than 0.")]
        public double Delta { get => delta; set => delta = value > 0 ? value : delta; }

        private double frameRate;
        [Description("Frame rate of the camera or video. Used to determine the frequency.")]
        public double FrameRate { get => frameRate; set => frameRate = value > 0 ? value : frameRate; }

        private int frameWindow;
        [Description("Frame window is used to determine the window in which to continue detecting successive peaks. A shorter frame window causes the peak detection method to reset more frequently.")]
        public int FrameWindow { get => frameWindow; set => frameWindow = value > 0 ? value : frameWindow; }

        private bool findMax;
        private bool prevFindMax;
        private bool boutDetected;
        private int startCounter;
        private int prevCounter;
        private bool firstPeak;
        private double minVal;
        private double maxVal;
        private double frequency;

        public override IObservable<double> Process(IObservable<double> source)
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
            return source.Select(value => DetectTailBeatFrequencyFunc(value));
        }

        public IObservable<double> Process(IObservable<double[]> source)
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
            return source.Select(value => DetectTailBeatFrequencyFunc(Utilities.CalculateMean(value)));
        }

        private double DetectTailBeatFrequencyFunc(double value)
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
                startCounter++;
            }
            else
            {
                if ((value > (minVal + delta)) || (value < (maxVal - delta)))
                {
                    boutDetected = true;
                    startCounter = 1;
                    if (value < (maxVal - delta))
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
            }

            prevFindMax = findMax;
            prevCounter = startCounter;
            return frequency;
        }
    }
}
