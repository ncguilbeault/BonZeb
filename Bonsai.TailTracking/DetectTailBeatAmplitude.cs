using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.TailTracking
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
        private bool prevFindMax;
        private bool boutDetected;
        private int startCounter;
        private bool firstPeak;
        private bool morePeaks;
        private double minVal;
        private double maxVal;

        public override IObservable<double> Process(IObservable<double> source)
        {
            findMax = true;
            prevFindMax = true;
            boutDetected = false;
            startCounter = 0;
            firstPeak = true;
            morePeaks = false;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            return source.Select(value => DetectTailBeatAmplitudeFunc(value));
        }

        public IObservable<double> Process(IObservable<double[]> source)
        {
            findMax = true;
            prevFindMax = true;
            boutDetected = false;
            startCounter = 0;
            firstPeak = true;
            morePeaks = false;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            return source.Select(value => Utilities.CalculateMean(value));
        }

        private double DetectTailBeatAmplitudeFunc(double value)
        {
            double amplitude = 0;
            maxVal = ((boutDetected || startCounter == 0) && (value > maxVal)) || (findMax && !boutDetected && (value > (minVal + delta))) || (!findMax && (value > (minVal + delta))) ? value : maxVal;
            minVal = ((boutDetected || startCounter == 0) && (value < minVal)) || (findMax && (value < (maxVal - delta))) ? value : minVal;
            findMax = (findMax && (value < (maxVal - delta))) ? false : ((!findMax && (value > minVal + delta)) || (startCounter > frameWindow)) ? true : findMax;
            boutDetected = (startCounter > frameWindow) ? false : (findMax && ((!boutDetected && (value > (minVal + delta))) || (value < (maxVal - delta)))) || (!findMax && (value > (minVal + delta))) ? true : boutDetected;
            maxVal = ((startCounter != 0) && !boutDetected && (prevFindMax == findMax)) ? 0 : maxVal;
            minVal = ((startCounter != 0) && !boutDetected && (prevFindMax == findMax)) ? 0 : minVal;
            startCounter = (!boutDetected || (startCounter > frameWindow) || (boutDetected && (prevFindMax != findMax))) ? 0 : startCounter + 1;
            morePeaks = (boutDetected && (prevFindMax == findMax) && !firstPeak) ? true : !boutDetected ? false : morePeaks;
            firstPeak = (boutDetected && (prevFindMax != findMax) && !findMax) ? false : !boutDetected ? true : firstPeak;
            amplitude = (morePeaks && boutDetected && (prevFindMax != findMax) && !findMax) ? maxVal : (morePeaks && boutDetected && (prevFindMax != findMax) && findMax) ? minVal : 0;
            prevFindMax = findMax;
            return amplitude;
        }
    }
}