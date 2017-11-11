using MathNet.Numerics.LinearAlgebra;
using System;

namespace Neurbot.Brain
{
    public static class ActivationFunctions
    {
        public static Matrix<double> Sigmoid(this Matrix<double> z)
        {
            return 1 / (1 + (-z).PointwiseExp());
        }

        public static Vector<double> Sigmoid(this Vector<double> z)
        {
            return 1 / (1 + (-z).PointwiseExp());
        }

        public static double Sigmoid(double z)
        {
            return 1.0 / (1.0 + Math.Exp(-z));
        }

        public static Matrix<double> SigmoidGrad(this Matrix<double> z)
        {
            var s = Sigmoid(z);
            return s.PointwiseMultiply(1 - s);
        }

        public static Vector<double> ReLU(this Vector<double> z)
        {
            return z.PointwiseMaximum(0);
        }

        public static Matrix<double> ReLU(this Matrix<double> z)
        {
            return z.PointwiseMaximum(0);
        }

        public static Matrix<double> ReLUGrad(this Matrix<double> z)
        {
            return z.Map(v => v < 0 ? 0.0 : 1.0);
        }

        public static Vector<double> ReLUGrad(this Vector<double> z)
        {
            return z.Map(v => v < 0 ? 0.0 : 1.0);
        }

        public static Matrix<double> Softmax(this Matrix<double> z)
        {
            var zExp = z.PointwiseExp();
            // Get the sum of each column (i.e. each sample)
            var sum = zExp.ColumnSums();
            // Divide all values of a sample by its sum
            return zExp.RowwiseDivide(sum);
        }

        public static Vector<double> Softmax(this Vector<double> z)
        {
            var zExp = z.PointwiseExp();
            return zExp / zExp.Sum();
        }

    }
}
