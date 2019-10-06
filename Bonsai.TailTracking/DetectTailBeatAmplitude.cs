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
        private int prevCounter;
        private double minVal;
        private double maxVal;

        public override IObservable<double> Process(IObservable<double> source)
        {
            findMax = true;
            prevFindMax = findMax;
            boutDetected = false;
            startCounter = 0;
            prevCounter = startCounter;
            minVal = double.PositiveInfinity;
            maxVal = double.NegativeInfinity;
            return source.Select(value => 
            {
                double amplitude = 0;
                maxVal = ((boutDetected || startCounter == 0) && (value > maxVal)) || (findMax && !boutDetected && (value > (minVal + delta))) || (!findMax && (value > (minVal + delta))) ? value : maxVal;
                minVal = ((boutDetected || startCounter == 0) && (value < minVal)) || (findMax && (value < (maxVal - delta))) ? value : minVal;
                findMax = (findMax && (value < (maxVal - delta))) ? false : ((!findMax && (value > minVal + delta)) || (startCounter > FrameWindow)) ? true : findMax;
                boutDetected = (findMax && ((!boutDetected && (value > (minVal + delta))) || (value < (maxVal - delta)))) || (!findMax && (value > (minVal + delta))) ? true : (startCounter > frameWindow) ? false : boutDetected;
                maxVal = (startCounter != 0 && !boutDetected && prevFindMax == findMax) ? 0 : maxVal;
                minVal = (startCounter != 0 && !boutDetected && prevFindMax == findMax) ? 0 : minVal;
                startCounter = !boutDetected || (startCounter > frameWindow) || (boutDetected && prevFindMax != findMax) ? 0 : startCounter + 1;
                amplitude = boutDetected && prevFindMax != findMax && !findMax ? maxVal : boutDetected && prevFindMax != findMax && findMax ? minVal : 0;
                prevFindMax = findMax;
                return amplitude;
            });
        }
    }
}