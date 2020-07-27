using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace BonZeb
{

    [Description("Detects extrema in tail beat amplitude from tail curvature using a peak signal detection method.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DetectTailBeatAmplitude : Transform<double, double>
    {

        public DetectTailBeatAmplitude()
        {
            Delta = 10;
            FrameWindow = 5;
        }

        private double delta;
        [Description("Delta is used to determine how much of a threshold is necessary to determine a peak in an ongoing signal. Value must be greater than 0.")]
        public double Delta { get => delta; set => delta = value > 0 ? value : delta; }

        private int frameWindow;
        [Description("Frame window is used to determine the window in which to continue detecting successive peaks. A shorter frame window causes the peak detection method to reset more frequently.")]
        public int FrameWindow { get => frameWindow; set => frameWindow = value > 0 ? value : frameWindow; }

        private bool findMax;
        private bool boutDetected;
        private int startCounter;
        private bool firstPeak;
        private double minVal;
        private double maxVal;
        private double amplitude;

        public override IObservable<double> Process(IObservable<double> source)
        {
            findMax = true;
            boutDetected = false;
            startCounter = 0;
            firstPeak = true;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            amplitude = 0;
            return source.Select(value => DetectTailBeatAmplitudeFunc(value));
        }

        public IObservable<double> Process(IObservable<double[]> source)
        {
            findMax = true;
            boutDetected = false;
            startCounter = 0;
            firstPeak = true;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            amplitude = 0;
            return source.Select(value => Utilities.CalculateMean(value));
        }

        private double DetectTailBeatAmplitudeFunc(double value)
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
                amplitude = 0;
            }
            return amplitude;
        }
    }
}