using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Neurbot.Brain
{
    public class History
    {
        private readonly List<Vector<double>> inputs = new List<Vector<double>>();
        private readonly List<Vector<double>>[] hiddenLayerOutputs;
        private readonly List<Vector<double>> outputs = new List<Vector<double>>();
        private readonly List<int> takenActions = new List<int>();

        public History(int nrOfHiddenLayers) :
            this(new List<Vector<double>>(),
                CreateEmptyHiddenLayerOutputs(nrOfHiddenLayers),
                new List<Vector<double>>(),
                new List<int>())
        { }

        private History(
            List<Vector<double>> inputs,
            List<Vector<double>>[] hiddenLayerOutputs,
            List<Vector<double>> outputs,
            List<int> takenActions)
        {
            this.inputs = inputs;
            this.hiddenLayerOutputs = hiddenLayerOutputs;
            this.outputs = outputs;
            this.takenActions = takenActions;
        }

        public static History Load(string fileName)
        {
            List<Vector<double>> inputs;
            List<Vector<double>>[] hiddenLayerOutputs;
            List<Vector<double>> outputs;
            List<int> takenActions;

            using (var stream = File.OpenRead(fileName))
            {
                var formatter = new BinaryFormatter();
                inputs = formatter.Deserialize(stream) as List<Vector<double>>;
                hiddenLayerOutputs = formatter.Deserialize(stream) as List<Vector<double>>[];
                outputs = formatter.Deserialize(stream) as List<Vector<double>>;
                takenActions = formatter.Deserialize(stream) as List<int>;
            }
            return new History(inputs, hiddenLayerOutputs, outputs, takenActions);
        }

        public void ForgetEverything()
        {
            inputs.Clear();
            for (int i = 0; i < hiddenLayerOutputs.Length; i++)
            {
                hiddenLayerOutputs[i].Clear();
            }
            outputs.Clear();
            takenActions.Clear();
        }

        public void Add(
            Vector<double> input,
            Vector<double>[] hiddenLayerOutputs,
            Vector<double> output,
            int takenAction)
        {
            inputs.Add(input);
            for (int i = 0; i < this.hiddenLayerOutputs.Length; i++)
            {
                this.hiddenLayerOutputs[i].Add(hiddenLayerOutputs[i]);
            }
            outputs.Add(output);
            takenActions.Add(takenAction);
        }

        public void Store(string fileName)
        {
            using (var stream = File.Create(fileName))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, inputs);
                formatter.Serialize(stream, hiddenLayerOutputs);
                formatter.Serialize(stream, outputs);
                formatter.Serialize(stream, takenActions);
            }
        }

        public Matrix<double> GetInputs()
        {
            return Matrix<double>.Build.DenseOfColumnVectors(inputs);
        }

        public Matrix<double> GetHiddenLayerOutputs(int layer)
        {
            return Matrix<double>.Build.DenseOfColumnVectors(hiddenLayerOutputs[layer]);
        }

        public Matrix<double> GetOutputs()
        {
            return Matrix<double>.Build.DenseOfColumnVectors(outputs);
        }

        public Matrix<double> GetTakenActionsAsOneHot()
        {
            var result = Matrix<double>.Build.Dense(outputs.First().Count, outputs.Count, 0.0);
            for (int c = 0; c < outputs.Count; c++)
            {
                var takenAction = takenActions[c];
                result[takenAction, c] = 1.0;
            }
            return result;
        }

        private static List<Vector<double>>[] CreateEmptyHiddenLayerOutputs(int nrOfHiddenLayers)
        {
            var hiddenLayerOutputs = new List<Vector<double>>[nrOfHiddenLayers];
            for (int i = 0; i < nrOfHiddenLayers; i++)
            {
                hiddenLayerOutputs[i] = new List<Vector<double>>();
            }
            return hiddenLayerOutputs;
        }
    }
}
