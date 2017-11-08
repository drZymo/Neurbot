using MathNet.Numerics.LinearAlgebra;
using Neurbot.Brain;
using Neurbot.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Neurbot.Learner
{
    class Program
    {
        private const string WeightsFile = @"D:\Swoc2017\brain.dat";
        private static readonly Random random = new Random();

        static void Main(string[] args)
        {
            MathNet.Numerics.Control.UseNativeMKL();

            //CreateNewWeights();

            var history = History.Load(@"D:\Swoc2017\history1.dat");
            var inputs = history.Inputs;
            var outputs = history.Outputs;
            Console.WriteLine("inputs = {0}", inputs);
            Console.WriteLine("outputs = {0}", outputs);

        }

        private static void CreateNewWeights()
        {
            int[] nrOfUnitsInLayers = new[] { 240, 480, 120, 16 };
            var nrOfLayers = nrOfUnitsInLayers.Length;

            var weights = new Matrix<double>[nrOfLayers - 1];
            for (int i = 1; i < nrOfLayers; i++)
            {
                // Initialize parameters with "He initialization" (He et al., 2015)
                // Recommended for ReLU activation functions.
                var w = Matrix<double>.Build.Dense(nrOfUnitsInLayers[i], nrOfUnitsInLayers[i - 1], (r, c) => random.NextNormal() * Math.Sqrt(2.0 / nrOfUnitsInLayers[i - 1]));
                weights[i - 1] = w;
            }

            using (var stream = File.Create(WeightsFile))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, weights);
            }

            var brain = Brain.Brain.LoadFromFile(WeightsFile);
        }
    }
}
