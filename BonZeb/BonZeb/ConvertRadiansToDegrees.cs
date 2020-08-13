using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace BonZeb
{

    [Description("Converts radians to degrees.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class ConvertRadiansToDegrees : Transform<double, double>
    {
        public override IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(value => Utilities.ConvertRadiansToDegrees(value));
        }
        public IObservable<double[]> Process(IObservable<double[]> source)
        {
            return source.Select(value => Utilities.ConvertRadiansToDegrees(value));
        }
    }
}
