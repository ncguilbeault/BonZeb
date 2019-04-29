using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.TailTracking
{

    [Description("Detects tail beat amplitude from tail curvature.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DetectTailBeatAmplitude : Transform<double, double>
    {

        [Description("Delta is used to determine how much of a threshold is necessary to determine a peak in the tail angle.")]
        public double Delta { get; set; }

        private bool findMax = true;
        private double minVal = double.PositiveInfinity;
        private double maxVal = double.NegativeInfinity;
        private double amplitude = 0;

        public override IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(value => 
            {

                double delta = Delta;
                amplitude = 0;

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