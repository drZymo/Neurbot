using MathNet.Numerics.LinearAlgebra;
using Neurbot.Brain;
using Neurbot.Generic;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Neurbot.Learner
{
    class Program
    {
        private const string EpisodeRootFolder = @"D:\Swoc2017\Episodes\";

        private static readonly Random random = new Random();


        static void Main(string[] args)
        {
            MathNet.Numerics.Control.UseNativeMKL();

            int episode = 0;
            {
                // Create an empty folder structure for this episode
                var episodeFolder = Path.Combine(EpisodeRootFolder, string.Format(@"episode{0}\", episode));
                var ticksFolder = Path.Combine(episodeFolder, @"Ticks\");
                var bot1Folder = Path.Combine(episodeFolder, @"Bot1\");
                var bot2Folder = Path.Combine(episodeFolder, @"Bot2\");

                if (Directory.Exists(episodeFolder))
                {
                    Directory.Delete(episodeFolder, true);
                }
                Directory.CreateDirectory(episodeFolder);
                Directory.CreateDirectory(ticksFolder);
                Directory.CreateDirectory(bot1Folder);
                Directory.CreateDirectory(bot2Folder);

                var bot1HistoryFileName = Path.Combine(bot1Folder, @"history.dat");
                var bot2HistoryFileName = Path.Combine(bot2Folder, @"history.dat");
                var brainFileName = Path.Combine(episodeFolder, @"brain.dat");

                var prevEpisodeFolder = Path.Combine(EpisodeRootFolder, string.Format(@"episode{0}\", episode - 1));
                var prevBrainFileName = Path.Combine(prevEpisodeFolder, @"brain.dat");


                // create run commands
                using (var writer = new StreamWriter(Path.Combine(bot1Folder, @"runCommand.txt")))
                {
                    writer.WriteLine(@"D:\Swoc2017\Neurbot\Neurbot.Micro\bin\Debug\Neurbot.Micro.exe -brain {0} -history {1}",
                        brainFileName, bot1HistoryFileName);
                }
                using (var writer = new StreamWriter(Path.Combine(bot2Folder, @"runCommand.txt")))
                {
                    writer.WriteLine(@"D:\Swoc2017\Neurbot\Neurbot.Micro\bin\Debug\Neurbot.Micro.exe -brain {0} -history {1}",
                        brainFileName, bot2HistoryFileName);
                }

                if (episode > 0)
                {
                    File.Copy(prevBrainFileName, brainFileName);
                }
                else
                {
                    CreateNewWeights(brainFileName);
                }

                // Run the process
                Console.WriteLine("Running episode {0}...", episode);
                var sw = Stopwatch.StartNew();
                var output = RunEpisode(ticksFolder, bot1Folder, bot2Folder);
                sw.Stop();
                Console.WriteLine("Running episode {0}...done in {1:f1} s", episode, sw.Elapsed.TotalSeconds);
                var winner = output.players.SingleOrDefault(p => p.id == output.winner);
                var loser = output.players.SingleOrDefault(p => p.id != output.winner);
                Console.WriteLine("  winner: {0}", winner.name);
                Console.WriteLine("  loser: {0}", loser.name);
            }

            //var history = History.Load(@"D:\Swoc2017\history1.dat");
            //var inputs = history.Inputs;
            //var outputs = history.Outputs;
            //Console.WriteLine("inputs = {0}", inputs);
            //Console.WriteLine("outputs = {0}", outputs);
            //
            //var brain = Brain.Brain.LoadFromFile(BrainFile);
            //
            //var input0 = inputs.Column(0);
            //var output0 = outputs.Column(0);
            //Console.WriteLine("in: {0}", input0);
            //Console.WriteLine("out: {0}", output0);
        }

        private static GameResult RunEpisode(string ticksFolder, string bot1Folder, string bot2Folder)
        {
            GameResult output;
            using (var process = StartProcess())
            {
                var input = new
                {
                    gameId = 1,
                    ticks = ticksFolder,
                    players = new[]
                    {
                        new {
                            id = 1,
                            name = "bot1",
                            bot = bot1Folder,
                            ufos = new[] { 11 },
                            color = "#fffed800",
                            hue = 0.142
                        },
                        new {
                            id = 2,
                            name = "bot2",
                            bot = bot2Folder,
                            ufos = new[] { 21 },
                            color = "#ff4cfe00",
                            hue = 0.284
                        },
                    }
                };
                var inputStr = JsonConvert.SerializeObject(input);
                process.StandardInput.WriteLine(inputStr);
                var outputStr = process.StandardOutput.ReadLine();
                output = JsonConvert.DeserializeObject<GameResult>(outputStr);
            }
            return output;
        }

        private static void CreateNewWeights(string brainFileName)
        {
            int[] nrOfUnitsInLayers = new[] { 242, 480, 120, 16 };
            var nrOfLayers = nrOfUnitsInLayers.Length;

            var weights = new Matrix<double>[nrOfLayers - 1];
            for (int i = 1; i < nrOfLayers; i++)
            {
                // Initialize parameters with "Xavier initialization"
                var w = Matrix<double>.Build.Dense(nrOfUnitsInLayers[i], nrOfUnitsInLayers[i - 1], (r, c) => random.NextNormal() * Math.Sqrt(1.0 / nrOfUnitsInLayers[i - 1]));
                weights[i - 1] = w;
            }

            using (var stream = File.Create(brainFileName))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, weights);
            }
        }

        private static Process StartProcess()
        {
            var startInfo = new ProcessStartInfo(@"C:\ProgramData\Oracle\Java\javapath\java.exe", @"-jar D:\Swoc2017\MicroEngine\micro.jar");
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;

            return Process.Start(startInfo);
        }
    }
}
