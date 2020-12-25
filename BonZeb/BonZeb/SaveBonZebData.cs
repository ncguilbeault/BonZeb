using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Bonsai;
using Bonsai.IO;
using Bonsai.Expressions;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Drawing.Design;
using System.Reactive.Disposables;
using System.Reactive;
using OpenCV.Net;
using Microsoft.SqlServer.Server;

namespace BonZeb
{

    [Combinator]
    [DefaultProperty("FileName")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Saves tail tracking data to csv file.")]

    public class SaveBonZebData : FileSink<object, BonZebDataDisposable>
    {

        static readonly object SyncRoot = new object();

        //protected new bool Buffered { get; private set; }

        public SaveBonZebData()
        {
            Overwrite = false;
        }

        protected override BonZebDataDisposable CreateWriter(string fileName, object input)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("A valid file path must be specified.");
            }

            PathHelper.EnsureDirectory(fileName);
            if (File.Exists(fileName) && !Overwrite)
            {
                throw new IOException(string.Format("The file '{0}' already exists.", fileName));
            }

            lock (SyncRoot)
            {
                var writer = new StreamWriter(fileName, false, Encoding.ASCII);
                if (!input.GetType().Equals(typeof(TailPoints)) && !input.GetType().Equals(typeof(TailKinematics)))
                {
                    writer.Close();
                    throw new InvalidOperationException("Incorrect data type passed to SaveTailTrackingData.");
                }
                string saveData = "";
                try
                {
                    Point2f[] tailPoints = ((TailPoints)input).Points;
                    saveData += "Centroid,";
                    for (int i = 0; i < tailPoints.Length - 1; i++)
                    {
                        saveData += string.Format("TailPoint{0}", i + 1);
                        if (i < tailPoints.Length - 2)
                        {
                            saveData += ",";
                        }
                    }
                    writer.WriteLine(saveData);
                }
                catch
                {
                    TailKinematics tailKinematics = (TailKinematics)input;
                    saveData += "Amplitude,Frequency,Instance";
                    writer.WriteLine(saveData);
                }
                return new BonZebDataDisposable(writer, Disposable.Create(() =>
                {
                    lock (SyncRoot)
                    {
                        writer.Close();
                    }
                }));
            }
        }

        protected override void Write(BonZebDataDisposable writer, object input)
        {
            try
            {
                string saveData = "";
                try
                {
                    Point2f[] tailPoints = ((TailPoints)input).Points;
                    for (int i = 0; i < tailPoints.Length; i++)
                    {
                        saveData += string.Format("{0}_{1}", tailPoints[i].X, tailPoints[i].Y);
                        if (i < tailPoints.Length - 1)
                        {
                            saveData += ",";
                        }
                    }
                    writer.Writer.WriteLine(saveData);
                }
                catch
                {
                    TailKinematics tailKinematics = (TailKinematics)input;
                    saveData += string.Format("{0},{1},{2}", tailKinematics.Amplitude, tailKinematics.Frequency, tailKinematics.Instance);
                    writer.Writer.WriteLine(saveData);
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Incorrect data type passed to SaveTailTrackingData. Error: {0}", exception);
            }
        }
    }
}
