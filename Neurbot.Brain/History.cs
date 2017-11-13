using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Neurbot.Brain
{
    public class History
    {
        private readonly Matrix<double> inputs;
        private readonly Matrix<double>[] hiddenLayersOutputs;
        private readonly Matrix<double> outputs;
        private readonly Matrix<double> takenActions;

        private History(
            Matrix<double> inputs,
            Matrix<double>[] hiddenLayersOutputs,
            Matrix<double> outputs,
            Matrix<double> takenActions)
        {
            this.inputs = inputs;
            this.hiddenLayersOutputs = hiddenLayersOutputs;
            this.outputs = outputs;
            this.takenActions = takenActions;
        }

        public static History Load(string fileName)
        {
            var inputs = new List<Vector<double>>();
            var hiddenLayersOutputs = new List<Vector<double>[]>();
            var outputs = new List<Vector<double>>();
            var takenActions = new List<int>();

            using (var stream = File.OpenRead(fileName))
            {
                var formatter = new BinaryFormatter();
                while (stream.Position < stream.Length)
                {
                    inputs.Add(formatter.Deserialize(stream) as Vector<double>);
                    hiddenLayersOutputs.Add(formatter.Deserialize(stream) as Vector<double>[]);
                    outputs.Add(formatter.Deserialize(stream) as Vector<double>);
                    takenActions.Add((int)formatter.Deserialize(stream));
                }
            }

            return new History(
                Matrix<double>.Build.DenseOfColumnVectors(inputs),
                ConvertHiddenLayersOutputs(hiddenLayersOutputs),
                Matrix<double>.Build.DenseOfColumnVectors(outputs),
                ConvertTakenActionsToOneHotMatrix(takenActions, outputs.First().Count));
        }

        public Matrix<double> Outputs { get => outputs; }

        public Matrix<double> Inputs { get => inputs; }

        public Matrix<double> TakenActions { get => takenActions; }

        public Matrix<double> GetHiddenLayersOutputs(int layer)
        {
            return hiddenLayersOutputs[layer];
        }

        private static Matrix<double>[] ConvertHiddenLayersOutputs(List<Vector<double>[]> entries)
        {
            if (!entries.Any())
            {
                throw new ArgumentException("list is empty", "storedOutputs");
            }

            var nrOfEntries = entries.Count;

            var firstEntry = entries.First();

            // Create matrices based on first entry
            var nrOfHiddenLayers = firstEntry.Length;
            var hiddenLayerOutputs = new Matrix<double>[nrOfHiddenLayers];
            for (int l = 0; l < nrOfHiddenLayers; l++)
            {
                var nrOfUnitsInLayer = firstEntry[l].Count;
                hiddenLayerOutputs[l] = Matrix<double>.Build.Dense(nrOfUnitsInLayer, nrOfEntries);
            }

            // Store all entries in the right matrix
            for (int c = 0; c < entries.Count; c++)
            {
                var entry = entries[c];
                Debug.Assert(entry.Length == nrOfHiddenLayers);
                for (int l = 0; l < nrOfHiddenLayers; l++)
                {
                    hiddenLayerOutputs[l].SetColumn(c, entry[l]);
                }
            }

            return hiddenLayerOutputs;
        }

        private static Matrix<double> ConvertTakenActionsToOneHotMatrix(IList<int> takenActions, int length)
        {
            var result = Matrix<double>.Build.Dense(length, takenActions.Count, 0.0);
            for (int c = 0; c < takenActions.Count; c++)
            {
                var takenAction = takenActions[c];
                result[takenAction, c] = 1.0;
            }
            return result;
        }
    }
}
