using MathNet.Numerics.LinearAlgebra;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Neurbot.Brain
{
    public class HistoryWriter : IDisposable
    {
        private readonly Stream outputStream;
        private readonly IFormatter formatter;

        public HistoryWriter(string fileName)
        {
            outputStream = File.Create(fileName);
            formatter = new BinaryFormatter();
        }

        public void Dispose()
        {
            outputStream.Dispose();
        }

        public void Write(
            Vector<double> input,
            Vector<double>[] hiddenLayerOutputs,
            Vector<double> output,
            int takenAction)
        {
            formatter.Serialize(outputStream, input);
            formatter.Serialize(outputStream, hiddenLayerOutputs);
            formatter.Serialize(outputStream, output);
            formatter.Serialize(outputStream, takenAction);
        }
    }
}
