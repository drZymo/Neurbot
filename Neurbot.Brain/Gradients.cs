using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neurbot.Brain
{
    public class Gradients
    {
        private readonly Matrix<double>[] gradients;

        private Gradients() : this(new Matrix<double>[0])
        { }

        public Gradients(IEnumerable<Matrix<double>> gradients)
        {
            this.gradients = gradients.ToArray();
        }

        private static readonly Gradients empty = new Gradients();
        public static Gradients Empty { get => empty; }

        public bool IsEmpty { get => gradients.Length == 0; }

        public Matrix<double> this[int i]
        {
            get { return gradients[i]; }
        }

        public Gradients Add(Gradients other)
        {
            if (IsEmpty)
            {
                return other;
            }
            else
            {
                if (other.gradients.Length != gradients.Length)
                {
                    throw new ArgumentException("Invalid number of gradients");
                }
                return new Gradients(gradients.Select(
                    (grad, index) => grad + other.gradients[index]));
            }
        }

        public Gradients AddAndDecay(Gradients gradients, double decayRate)
        {
            // TODO: Causes NAN!!!
            if (IsEmpty)
            {
                return gradients;
            }
            else
            {
                if (gradients.gradients.Length != this.gradients.Length)
                {
                    throw new ArgumentException("Invalid number of gradients");
                }
                return new Gradients(this.gradients.Select(
                    (grad, index) => decayRate * grad + (1 - decayRate) * gradients.gradients[index].PointwisePower(2)));
            }
        }

        public Gradients ApplyRMSProp(Gradients rmspropCache)
        {
            // TODO: Causes NAN!!!
            if (IsEmpty)
            {
                return this;
            }
            else
            {
                if (rmspropCache.gradients.Length != gradients.Length)
                {
                    throw new ArgumentException("Invalid number of gradients");
                }

                return new Gradients(gradients.Select(
                    (grad, index) => grad.PointwiseDivide(rmspropCache.gradients[index].PointwiseSqrt() + 1e-5))); // TODO: 1e-5 -> 1e-9?
            }

        }
    }
}
