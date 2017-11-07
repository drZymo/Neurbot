using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Neurbot.Brain
{
    public class Brain
    {
        private static readonly Random random = new Random();

        private readonly Matrix<double>[] weights;

        private Brain(IEnumerable<Matrix<double>> weights)
        {
            this.weights = weights.ToArray();
        }

        public static Brain LoadFromFile(string fileName)
        {
            IEnumerable<Matrix<double>> weights;
            using (var stream = File.OpenRead(fileName))
            {
                var formatter = new BinaryFormatter();
                weights = formatter.Deserialize(stream) as Matrix<double>[];
            }

            Console.WriteLine("loaded brain from '{0}' contains {1} layers", fileName, weights.Count());
            return new Brain(weights);
        }

        public int GetBestAction(double[] gameState)
        {
            var aIn = Vector<double>.Build.Dense(gameState);

            var probabilities = Forward(aIn);

            return FindIndexOfMaxValue(probabilities);
        }


        public int GetRandomAction(double[] gameState)
        {
            var aIn = Vector<double>.Build.Dense(gameState);

            var probabilities = Forward(aIn);

            return RandomIndexFromProbabilities(probabilities);
        }

        private Vector<double> Forward(Vector<double> aIn)
        {
            var aPrev = aIn;
            for (int i = 0; i < weights.Length - 1; i++)
            {
                var z = weights[i].Multiply(aPrev);
                var a = z.ReLU();
                aPrev = a;
            }

            var zOut = weights[weights.Length - 1].Multiply(aPrev);
            var aOut = zOut.Softmax();
            return aOut;
        }

        private static int FindIndexOfMaxValue<T>(IEnumerable<T> aOut) where T : IComparable<T>
        {
            return !aOut.Any() ? -1 :
                aOut
                .Select((value, index) => new { Value = value, Index = index })
                .Aggregate((a, b) => (a.Value.CompareTo(b.Value) > 0) ? a : b)
                .Index;
        }

        private static int RandomIndexFromProbabilities(IList<double> probabilities)
        {
            var value = random.NextDouble();

            var probabilitySum = 0.0;
            for (int i = 0; i < probabilities.Count - 1; i++)
            {
                var probability = probabilities[i];
                probabilitySum += probability;

                if (value < probabilitySum)
                {
                    return i;
                }
            }

            return probabilities.Count - 1;
        }
    }
}
