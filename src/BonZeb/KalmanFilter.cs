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

            double SamplingRate2 = Math.Pow(SamplingRate, 2);
            double SamplingRate3 = Math.Pow(SamplingRate, 3);
            double SamplingRate4 = Math.Pow(SamplingRate, 4);

            A = DenseMatrix.OfArray(new double[,] 
            {
                { 1, 0, SamplingRate, 0 },
                { 0, 1, 0, SamplingRate },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            });

            B = DenseMatrix.OfArray(new double[,] 
            {
                { SamplingRate2 / 2 },
                { SamplingRate2 / 2 },
                { SamplingRate },
                { SamplingRate }
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
                { SamplingRate4 / 4, 0, SamplingRate3 / 2, 0 },
                { 0, SamplingRate4 / 4, 0, SamplingRate3 / 2 },
                { SamplingRate3 / 2, 0, SamplingRate2, 0 },
                { 0, SamplingRate3 / 2, 0, SamplingRate2 }
            });

            Ex.Multiply(aVar, result: Ex);

            Q_estimate = DenseMatrix.OfArray(new double[4, 1]);
            P = Ex.Clone();
        }

        public void Push(double x, double y)
        {
            Matrix<double> Qloc = DenseMatrix.OfArray(new double[,] 
            {
                { x },
                { y }
            });

            Q_estimate = Dot(A, Q_estimate).Add(B.Multiply(Acceleration));
            P = Dot(A, Dot(P, A.Transpose())).Add(Ex);
            Aux = Dot(C, Dot(P, C.Transpose())).Add(Ez).PseudoInverse();
            K = Dot(P, Dot(C.Transpose(), Aux));
            Q_estimate = Q_estimate.Add(Dot(K, Qloc.Subtract(Dot(C, Q_estimate))));
            P = Dot(diagonal.Subtract(Dot(K, C)), P);
        }

        public Matrix<double> Dot(Matrix<double> X, Matrix<double> Y)
        {
            Matrix<double> dotProduct = DenseMatrix.OfArray(new double[X.RowCount, Y.ColumnCount]);
            for (int i = 0; i <= X.RowCount; i++)
            {
                for (int j = 0; j <= Y.ColumnCount; j++)
                {
                    dotProduct[i, j] += X.Row(i).DotProduct(Y.Column(j));
                }
            }
            return dotProduct;
        }
    }
}
