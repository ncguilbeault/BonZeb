using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace BonZeb
{
    public class KalmanFilter
    {

        Matrix<double> Q_estimate;

        Matrix<double> A;
        Matrix<double> B;
        Matrix<double> C;

        Matrix<double> Ez;
        Matrix<double> Ex;
        Matrix<double> P;
        Matrix<double> K;
        Matrix<double> Aux;

        static readonly Matrix<double> diagonal = Matrix<double>.Build.DenseIdentity(4, 4);

        public double X
        {
            get { return Q_estimate[0, 0]; }
            set { Q_estimate[0, 0] = value; }
        }

        public double Y
        {
            get { return Q_estimate[1, 0]; }
            set { Q_estimate[1, 0] = value; }
        }

        public double XAxisVelocity
        {
            get { return Q_estimate[2, 0]; }
            set { Q_estimate[2, 0] = value; }
        }

        public double YAxisVelocity
        {
            get { return Q_estimate[3, 0]; }
            set { Q_estimate[3, 0] = value; }
        }

        public double NoiseX
        {
            get { return Ez[0, 0]; }
            set { Ez[0, 0] = value; }
        }

        public double NoiseY
        {
            get { return Ez[1, 1]; }
            set { Ez[1, 1] = value; }
        }

        public double SamplingRate { get; set; }
        public double Acceleration { get; set; }
        public double AccelerationStdDev { get; set; }

        public KalmanFilter(double samplingRate, double acceleration, double accelerationStdDev)
        {
            SamplingRate = samplingRate;
            Acceleration = acceleration;
            AccelerationStdDev = accelerationStdDev;

            double dt = 1.0 / samplingRate;
            double dt2 = Math.Pow(dt, 2);
            double dt3 = Math.Pow(dt, 3);
            double dt4 = Math.Pow(dt, 4);

            A = DenseMatrix.OfArray(new double[,]
            {
                { 1, 0, dt, 0 },
                { 0, 1, 0, dt },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            });

            B = DenseMatrix.OfArray(new double[,]
            {
                { dt2 / 2 },
                { dt2 / 2 },
                { dt },
                { dt }
            });

            C = DenseMatrix.OfArray(new double[,]
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 }
            });

            Ez = DenseMatrix.OfArray(new double[,]
            {
                { 1.0, 0.0 },
                { 0.0, 1.0 }
            });

            double aVar = Math.Pow(accelerationStdDev, 2);

            Ex = DenseMatrix.OfArray(new double[4, 4]
            {
                { dt4 / 4, 0, dt3 / 2, 0 },
                { 0, dt4 / 4, 0, dt3 / 2 },
                { dt3 / 2, 0, dt2, 0 },
                { 0, dt3 / 2, 0, dt2 }
            });

            Ex.Multiply(aVar, result: Ex);

            Q_estimate = DenseMatrix.OfArray(new double[4, 1]);
            P = Ex.Clone();
        }

        public void Predict()
        {
            // Advances the state estimate by one time step.
            Q_estimate = A.Multiply(Q_estimate).Add(B.Multiply(Acceleration));
            P = A.Multiply(P).Multiply(A.Transpose()).Add(Ex);
        }

        public void Update(double x, double y)
        {
            // Updates the state estimate with a measured position.
            Matrix<double> Qloc = DenseMatrix.OfArray(new double[,]
            {
                { x },
                { y }
            });

            Aux = C.Multiply(P).Multiply(C.Transpose()).Add(Ez).PseudoInverse();
            K = P.Multiply(C.Transpose()).Multiply(Aux);
            Q_estimate = Q_estimate.Add(K.Multiply(Qloc.Subtract(C.Multiply(Q_estimate))));
            P = diagonal.Subtract(K.Multiply(C)).Multiply(P);
        }

        public void Push(double x, double y)
        {
            Predict();
            Update(x, y);
        }
    }
}
