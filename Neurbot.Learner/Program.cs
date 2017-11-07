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
        private const string WeightsFile = @"D:\Swoc2017\weights.dat";
        private static readonly Random random = new Random();

        static void Main(string[] args)
        {
            MathNet.Numerics.Control.UseNativeMKL();

            int[] nrOfUnitsInLayers = new[] { 4, 200, 2 };

            var w1 = Matrix<double>.Build.Dense(nrOfUnitsInLayers[1], nrOfUnitsInLayers[0], (r, c) => random.NextNormal() / Math.Sqrt(nrOfUnitsInLayers[0]));
            var w2 = Matrix<double>.Build.Dense(nrOfUnitsInLayers[2], nrOfUnitsInLayers[1], (r, c) => random.NextNormal() / Math.Sqrt(nrOfUnitsInLayers[1]));


            var weights = new[] { w1, w2 };

            using (var stream = File.Create(WeightsFile))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, weights);
            }

            var brain = Brain.Brain.LoadFromFile(WeightsFile);
        }
    }
}
