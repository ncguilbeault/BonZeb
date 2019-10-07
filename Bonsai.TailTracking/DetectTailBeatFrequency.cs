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
        private bool morePeaks;
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
            morePeaks = false;
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
            morePeaks = false;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            frequency = 0;
            return source.Select(value => DetectTailBeatFrequencyFunc(Utilities.CalculateMean(value)));
        }

        private double DetectTailBeatFrequencyFunc(double value)
        {
            maxVal = ((boutDetected || startCounter == 0) && (value > maxVal)) || (findMax && !boutDetected && (value > (minVal + delta))) || (!findMax && (value > (minVal + delta))) ? value : maxVal;
            minVal = ((boutDetected || startCounter == 0) && (value < minVal)) || (findMax && (value < (maxVal - delta))) ? value : minVal;
            findMax = (findMax && (value < (maxVal - delta))) ? false : ((!findMax && (value > minVal + delta)) || (startCounter > frameWindow)) ? true : findMax;
            boutDetected = (startCounter > frameWindow) ? false : (findMax && ((!boutDetected && (value > (minVal + delta))) || (value < (maxVal - delta)))) || (!findMax && (value > (minVal + delta))) ? true : boutDetected;
            maxVal = ((startCounter != 0) && !boutDetected && (prevFindMax == findMax)) ? 0 : maxVal;
            minVal = ((startCounter != 0) && !boutDetected && (prevFindMax == findMax)) ? 0 : minVal;
            startCounter = (!boutDetected || (startCounter > frameWindow) || (boutDetected && (prevFindMax != findMax))) ? 0 : startCounter + 1;
            morePeaks = (boutDetected && (prevFindMax == findMax) && !firstPeak) ? true : !boutDetected ? false : morePeaks;
            firstPeak = (boutDetected && (prevFindMax != findMax) ? false : !boutDetected) ? true : firstPeak;
            frequency = (morePeaks && boutDetected && (prevFindMax != findMax) && (startCounter == 0) && (startCounter != prevCounter)) ? frameRate / (2.0 * prevCounter) : !boutDetected ? 0 : frequency;
            prevFindMax = findMax;
            prevCounter = startCounter;
            return frequency;
        }
    }
}
