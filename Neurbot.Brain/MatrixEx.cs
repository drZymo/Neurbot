using MathNet.Numerics.LinearAlgebra;

namespace Neurbot.Brain
{
    public static class MatrixEx
    {
        /// <summary>
        /// Add a vector to each column of the matrix.
        /// </summary>
        public static Matrix<double> ColumnwiseAdd(this Matrix<double> m, Vector<double> v)
        {
            return m.MapIndexed((r, c, val) => val + v[r]);
        }

        /// <summary>
        /// Subtract a vector from each column of the matrix.
        /// </summary>
        public static Matrix<double> ColumnwiseSubtract(this Matrix<double> m, Vector<double> v)
        {
            return m.MapIndexed((r, c, val) => val - v[r]);
        }

        /// <summary>
        /// Multiply each column of the matrix with a vector.
        /// </summary>
        public static Matrix<double> ColumnwiseMultiply(this Matrix<double> m, Vector<double> v)
        {
            return m.MapIndexed((r, c, val) => val * v[r]);
        }

        /// <summary>
        /// Divide each column of the matrix with a vector.
        /// </summary>
        public static Matrix<double> ColumnwiseDivide(this Matrix<double> m, Vector<double> v)
        {
            return m.MapIndexed((r, c, val) => val / v[r]);
        }

        /// <summary>
        /// Add a vector to each row of the matrix.
        /// </summary>
        public static Matrix<double> RowwiseAdd(this Matrix<double> m, Vector<double> v)
        {
            return m.MapIndexed((r, c, val) => val + v[c]);
        }

        /// <summary>
        /// Subtract a vector from each row of the matrix.
        /// </summary>
        public static Matrix<double> RowwiseSubtract(this Matrix<double> m, Vector<double> v)
        {
            return m.MapIndexed((r, c, val) => val - v[c]);
        }

        /// <summary>
        /// Multiply each row of the matrix with a vector.
        /// </summary>
        public static Matrix<double> RowwiseMultiply(this Matrix<double> m, Vector<double> v)
        {
            return m.MapIndexed((r, c, val) => val * v[c]);
        }

        /// <summary>
        /// Divide each row of the matrix with a vector.
        /// </summary>
        public static Matrix<double> RowwiseDivide(this Matrix<double> m, Vector<double> v)
        {
            return m.MapIndexed((r, c, val) => val / v[c]);
        }
    }
}
