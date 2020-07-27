using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace BonZeb
{

    [Description("Calibrates one set of draw parameters onto another set of draw parameters.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class CalculateCalibrationParameters : Transform<Tuple<CalibrationParameters, CalibrationParameters>, CalibrationParameters>
    {

        public override IObservable<CalibrationParameters> Process(IObservable<Tuple<CalibrationParameters, CalibrationParameters>> source)
        {
            return source.Select(value => 
            {
                CalibrationParameters cameraDrawParameters = value.Item1;
                CalibrationParameters projectorDrawParameters = value.Item2;
                double xRange = cameraDrawParameters.XRange * projectorDrawParameters.XRange;
                double yRange = cameraDrawParameters.YRange * projectorDrawParameters.YRange;
                double xOffset = xRange < 1 ? cameraDrawParameters.XOffset * projectorDrawParameters.XRange + projectorDrawParameters.XOffset : projectorDrawParameters.XOffset;
                double yOffset = yRange < 1 ? cameraDrawParameters.YOffset * projectorDrawParameters.YRange + projectorDrawParameters.YOffset : projectorDrawParameters.YOffset;
                return new CalibrationParameters(xOffset, yOffset, xRange, yRange, cameraDrawParameters.Colour);
            });
        }
    }
}
