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

        private readonly HistoryWriter historyWriter;

        private Brain(IEnumerable<Matrix<double>> weights, string historyFile)
        {
            this.weights = weights.ToArray();

            historyWriter = !string.IsNullOrEmpty(historyFile)
                ? new HistoryWriter(historyFile)
                : null;
        }

        public static Brain LoadFromFile(string fileName, string historyFile = null)
        {
            IEnumerable<Matrix<double>> weights;
            using (var stream = File.OpenRead(fileName))
            {
                var formatter = new BinaryFormatter();
                weights = formatter.Deserialize(stream) as Matrix<double>[];
            }

            Console.WriteLine("loaded brain from '{0}' contains {1} layers", fileName, weights.Count());
            return new Brain(weights, historyFile);
        }

        public void SaveToFile(string fileName)
        {
            using (var stream = File.Create(fileName))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, weights);
            }
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

            historyWriter?.Write(input, hiddenLayerOutputs, probabilities, action);

            return action;
        }

        public Gradients ComputeGradient(History history, double reward)
        {
            var takenActions = history.TakenActions;

            var discountedRewards = DiscountReward(reward, takenActions.ColumnCount, 0.99);
            var dz3 = takenActions - history.Outputs;
            dz3 = dz3.RowwiseMultiply(discountedRewards);

            var dw3 = dz3.Multiply(history.GetHiddenLayersOutputs(1).Transpose()); // TODO optimize by doing transpose in History
            var da2 = weights[2].Transpose().Multiply(dz3);
            var dz2 = da2.ReLUGrad();
            var dw2 = dz2.Multiply(history.GetHiddenLayersOutputs(0).Transpose()); // TODO optimize by doing transpose in History
            var da1 = weights[1].Transpose().Multiply(dz2);
            var dz1 = da1.ReLUGrad();
            var dw1 = dz1.Multiply(history.Inputs.Transpose()); // TODO optimize by doing transpose in History
            var da0 = weights[0].Transpose().Multiply(dz1);

            return new Gradients(new[] { dw1, dw2, dw3 });
        }

        public void Descent(double LearningRate, Gradients gradients)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = weights[i] + LearningRate * gradients[i];
            }
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

        private static Vector<double> DiscountReward(double reward, int length, double gamma)
        {
            var rewards = Vector<double>.Build.Dense(length);
            var discountedReward = reward;
            for (int i = length - 1; i >= 0; i--)
            {
                rewards[i] = discountedReward;
                discountedReward = discountedReward * gamma;
            }
            return rewards;
        }
    }
}
