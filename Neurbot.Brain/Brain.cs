using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Neurbot.Brain
{
    public class Brain
    {
        private const bool useSigmoid = true;

        // Use a seed unique for this process, so two of the same instances started at the same time have a different seed.
        private static readonly Random random = new Random(DateTime.Now.GetHashCode() % Process.GetCurrentProcess().Id);

        private readonly Matrix<double>[] weights;

        private readonly HistoryWriter historyWriter;

        private Brain(IEnumerable<Matrix<double>> weights, string historyFile)
        {
            this.weights = weights.ToArray();

            CheckForNaNsInWeights();

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
            var dzOutput = takenActions - history.Outputs;
            dzOutput = dzOutput.RowwiseMultiply(discountedRewards);

            Backward(history, dzOutput, out var gradients);

            return gradients;
        }

        public void Descent(double LearningRate, Gradients gradients)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = weights[i] + LearningRate * gradients[i];
            }

            CheckForNaNsInWeights();
        }

        private void CheckForNaNsInWeights()
        {
            if (weights.Any(w => !w.ForAll(v => !double.IsNaN(v))))
            {
                Console.WriteLine("NaNs in weights!");
            }
        }

        private Vector<double> Forward(Vector<double> aIn, out Vector<double>[] hiddenLayerOutputs)
        {
            hiddenLayerOutputs = new Vector<double>[weights.Length - 1];
            var aPrev = aIn;
            for (int i = 0; i < weights.Length - 1; i++)
            {
                var z = weights[i].Multiply(aPrev);
                var a = useSigmoid
                    ? z.Sigmoid()
                    : z.ReLU();
                hiddenLayerOutputs[i] = a;
                aPrev = a;
            }

            var zOut = weights[weights.Length - 1].Multiply(aPrev);
            var aOut = zOut.Softmax();
            return aOut;
        }

        private void Backward(History history, Matrix<double> dz3, out Gradients gradients)
        {
            var gradientsBuffer = new Matrix<double>[weights.Length];

            var dz = dz3;
            for (int l = weights.Length - 1; l > 0; l--)
            {
                gradientsBuffer[l] = dz.Multiply(history.GetHiddenLayersOutputs(l - 1).Transpose());
                var da = weights[l].Transpose().Multiply(dz);
                dz = useSigmoid
                    ? da.SigmoidGrad()
                    : da.ReLUGrad();
            }

            gradientsBuffer[0] = dz.Multiply(history.Inputs.Transpose());
            var da0 = weights[0].Transpose().Multiply(dz);

            gradients = new Gradients(gradientsBuffer);
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
