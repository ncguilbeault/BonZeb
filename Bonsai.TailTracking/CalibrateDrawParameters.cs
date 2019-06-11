using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.TailTracking
{

    [Description("Calibrates one set of draw parameters onto another set of draw parameters.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalibrateDrawParameters : Transform<Tuple<Utilities.DrawParameters, Utilities.DrawParameters>, Utilities.DrawParameters>
    {

        public override IObservable<Utilities.DrawParameters> Process(IObservable<Tuple<Utilities.DrawParameters, Utilities.DrawParameters>> source)
        {
            return source.Select(value => {
                Utilities.DrawParameters cameraDrawParameters = value.Item1;
                Utilities.DrawParameters projectorDrawParameters = value.Item2;
                double xRange = cameraDrawParameters.XRange * projectorDrawParameters.XRange;
                double yRange = cameraDrawParameters.YRange * projectorDrawParameters.YRange;
                double xOffset = xRange < 1 ? cameraDrawParameters.XOffset * projectorDrawParameters.XRange + projectorDrawParameters.XOffset : projectorDrawParameters.XOffset;
                double yOffset = yRange < 1 ? cameraDrawParameters.YOffset * projectorDrawParameters.YRange + projectorDrawParameters.YOffset : projectorDrawParameters.YOffset;
                return new Utilities.DrawParameters(xOffset, yOffset, xRange, yRange, cameraDrawParameters.Colour);
            });
        }
    }
}
