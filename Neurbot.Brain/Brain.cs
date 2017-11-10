using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Neurbot.Brain
{
    public class Brain
    {
        private static readonly Random random = new Random();

        private readonly Matrix<double>[] weights;

        private readonly History history;

        private Brain(IEnumerable<Matrix<double>> weights)
        {
            this.weights = weights.ToArray();

            history = new History(this.weights.Length - 1);
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

        public int GetBestAction(Vector<double> input)
        {
            return GetAction(input, false, out var probabilities);
        }

        public int GetRandomAction(Vector<double> input)
        {
            return GetAction(input, true, out var probabilities);
        }

        public int GetAction(Vector<double> input, bool randomAction, out Vector<double> probabilities)
        {
            probabilities = Forward(input, out var hiddenLayerOutputs);

            var action = randomAction
                ? RandomIndexFromProbabilities(probabilities)
                : FindIndexOfMaxValue(probabilities);

            history.Add(input, hiddenLayerOutputs, probabilities, action);

            return action;
        }

        public void ForgetHistory()
        {
            history.ForgetEverything();
        }

        public void SaveHistory(string fileName)
        {
            history.Store(fileName);
        }

        private Vector<double> Forward(Vector<double> aIn, out Vector<double>[] hiddenLayerOutputs)
        {
            hiddenLayerOutputs = new Vector<double>[weights.Length - 1];
            var aPrev = aIn;
            for (int i = 0; i < weights.Length - 1; i++)
            {
                var z = weights[i].Multiply(aPrev);
                var a = z.Sigmoid();
                hiddenLayerOutputs[i] = a;
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
