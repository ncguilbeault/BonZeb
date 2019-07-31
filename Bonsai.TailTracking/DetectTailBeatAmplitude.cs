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
        [Description("Delta is used to determine how much of a threshold is necessary to determine a peak in an ongoing signal.")]
        public double Delta { get { return delta; } set { delta = value; } }

        public override IObservable<double> Process(IObservable<double> source)
        {
            bool findMax = true;
            double minVal = double.PositiveInfinity;
            double maxVal = double.NegativeInfinity;
            return source.Select(value => 
            {
                double amplitude = 0;
                if (value > maxVal)
                {
                    maxVal = value;
                }
                if (value < minVal)
                {
                    minVal = value;
                }
                if (findMax)
                {
                    if (value < (maxVal - delta))
                    {
                        minVal = value;
                        findMax = false;
                        amplitude = minVal;
                    }
                }
                else
                {
                    if (value > (minVal + delta))
                    {
                        maxVal = value;
                        findMax = true;
                        amplitude = maxVal;
                    }
                }
                return amplitude;
            });
        }
    }
}