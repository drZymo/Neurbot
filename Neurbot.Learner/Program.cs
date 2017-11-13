using MathNet.Numerics.LinearAlgebra;
using Neurbot.Brain;
using Neurbot.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Neurbot.Learner
{
    class Program
    {
        private const string EpisodeRootFolder = @"D:\Swoc2017\Episodes\";
        private static readonly string BrainFileName = Path.Combine(EpisodeRootFolder, @"brain.dat");

        private static readonly Random random = new Random();

        private const int NrOfEpisodes = 50000;
        private const int BatchSize = 50;
        private const int NrOfConcurrentGames = 8;

        private const double RMSPropDecayRate = 0.99;
        private const double LearningRate = 1e-4;

        private static CancellationTokenSource cancellation = new CancellationTokenSource();

        private static readonly ConcurrentQueue<int> pendingEpisodes = new ConcurrentQueue<int>();

        static void Main(string[] args)
        {
            MathNet.Numerics.Control.UseNativeMKL();

            Console.CancelKeyPress += OnCancelKeyPress;

            // Create a fresh brain if it doesn't exist yet
            if (!File.Exists(BrainFileName))
            {
                CreateNewWeights(BrainFileName);
            }
            var brain = Brain.Brain.LoadFromFile(BrainFileName);

            //var history = History.Load(@"D:\Swoc2017\Episodes\episode0\Bot1\history.dat");
            //var gradientsWinner = brain.ComputeGradient(history, 1.0);
            //
            //Console.WriteLine("gradients = {0}", gradientsWinner[1]);
            //
            //var input = history.Inputs.Column(0);
            //Console.WriteLine("input = {0}", input);
            //int actionIdPre = brain.GetAction(input, false, out var probsPre);
            //Console.WriteLine("probs pre = {0}", probsPre);
            //Console.WriteLine("action prob pre = {0}", probsPre[actionIdPre]);
            //
            //brain.Descent(1e-2, gradientsWinner);
            //int actionIdPost = brain.GetAction(input, false, out var probsPost);
            //Console.WriteLine("probs post = {0}", probsPost);
            //Console.WriteLine("action prob post = {0}", probsPost[actionIdPost]);


            //var rmspropCache = Gradients.Empty;

            var allGradients = new ConcurrentQueue<Gradients>();
            for (int episode = 0; episode < NrOfEpisodes && !cancellation.IsCancellationRequested; episode += BatchSize)
            {
                // Run a number of episodes at once
                for (int i = 0; i < BatchSize; i++)
                {
                    pendingEpisodes.Enqueue(episode + i);
                }
                RunAllPendingEpisodes(brain, allGradients);

                if (!cancellation.IsCancellationRequested)
                {
                    // Apply the sum of all gradients so far
                    var gradientsSum = Gradients.Empty;
                    while (allGradients.TryDequeue(out var gradients))
                    {
                        gradientsSum = gradientsSum.Add(gradients);
                    }

                    // Descent
                    Console.WriteLine("Descending");
                    // TODO: RMS prop causes NaNs
                    //rmspropCache = rmspropCache.AddAndDecay(gradientsSum, RMSPropDecayRate);
                    //brain.Descent(LearningRate, gradientsSum.ApplyRMSProp(rmspropCache));
                    brain.Descent(LearningRate, gradientsSum);

                    // Store updated brain to file
                    brain.SaveToFile(BrainFileName);
                }
            }

            if (cancellation.IsCancellationRequested)
            {
                Console.WriteLine("Cancelled!");
            }
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cancellation.Cancel();
            Console.WriteLine("Cancelling...");
            e.Cancel = true;
        }

        private static void RunAllPendingEpisodes(Brain.Brain brain, ConcurrentQueue<Gradients> allGradients)
        {
            var runningTasks = new List<Task>();

            // pick the first episodes
            while (runningTasks.Count < NrOfConcurrentGames && pendingEpisodes.Any())
            {
                if (pendingEpisodes.TryDequeue(out var episode))
                {
                    runningTasks.Add(Task.Run(() => RunEpisode(brain, episode, allGradients)));
                }
            }

            do
            {
                // Add a new task if there is room
                if ((runningTasks.Count < NrOfConcurrentGames) &&
                    !cancellation.IsCancellationRequested &&
                    pendingEpisodes.TryDequeue(out var episode))
                {
                    runningTasks.Add(Task.Run(() => RunEpisode(brain, episode, allGradients)));
                }

                // Wait for one task to finish
                var completedTaskIndex = Task.WaitAny(runningTasks.ToArray());
                runningTasks.RemoveAt(completedTaskIndex);
            } while (runningTasks.Any() || (!cancellation.IsCancellationRequested && pendingEpisodes.Any()));
        }

        private static void RunEpisode(Brain.Brain brain, int episode, ConcurrentQueue<Gradients> allGradients)
        {
            var sw = Stopwatch.StartNew();

            int winnerId = -1;
            try
            {
                // Create an empty folder structure for this episode
                var episodeFolder = Path.Combine(EpisodeRootFolder, string.Format(@"episode{0}\", episode));
                var ticksFolder = Path.Combine(episodeFolder, @"Ticks\");

                var botFolders = new[] { Path.Combine(episodeFolder, @"Bot1\"), Path.Combine(episodeFolder, @"Bot2\") };
                var botHistoryFileNames = botFolders.Select(f => Path.Combine(f, @"history.dat")).ToArray();

                PrepareEpisode(episodeFolder, ticksFolder, botFolders, botHistoryFileNames);

                var output = RunGame(ticksFolder, botFolders[0], botFolders[1]);

                winnerId = output.winner;
                if (winnerId >= 0 && winnerId <= 1)
                {
                    var winner = output.players.SingleOrDefault(p => p.id == winnerId);
                    var loser = output.players.SingleOrDefault(p => p.id != winnerId);

                    // Update gradients
                    // winner gets +1 reward
                    var historyWinner = History.Load(botHistoryFileNames[winner.id]);
                    var gradientsWinner = brain.ComputeGradient(historyWinner, 1.0);
                    allGradients.Enqueue(gradientsWinner);

                    // loser gets -1 reward
                    var historyLoser = History.Load(botHistoryFileNames[loser.id]);
                    var gradientsLoser = brain.ComputeGradient(historyLoser, -1.0);
                    allGradients.Enqueue(gradientsLoser);
                }
                else
                {
                    Console.WriteLine("illegal winner id {0}", winnerId);
                }

                // Delete folder
                // Randomly keep 1 in 50
                if (random.NextDouble() >= 1.0 / 50)
                {
                    Directory.Delete(episodeFolder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in episode {0} '{1}': {2}", episode, ex.GetType(), ex.Message);
                Console.WriteLine("{0}", ex.StackTrace);
            }

            sw.Stop();
            Console.WriteLine("episode {0} won by {1} in {2:f1} s", episode, winnerId, sw.Elapsed.TotalSeconds);
        }

        private static void PrepareEpisode(string episodeFolder, string ticksFolder, string[] botFolders, string[] botHistoryFileNames)
        {
            if (Directory.Exists(episodeFolder))
            {
                Directory.Delete(episodeFolder, true);
            }
            Directory.CreateDirectory(episodeFolder);
            Directory.CreateDirectory(ticksFolder);

            var brainFileName = Path.Combine(episodeFolder, @"brain.dat");
            File.Copy(BrainFileName, brainFileName);

            // create bot folder
            for (int b = 0; b < botFolders.Length; b++)
            {
                var botFolder = botFolders[b];
                var botHistoryFileName = botHistoryFileNames[b];

                Directory.CreateDirectory(botFolder);

                using (var writer = new StreamWriter(Path.Combine(botFolder, @"runCommand.txt")))
                {
                    writer.WriteLine(@"D:\Swoc2017\Neurbot\Neurbot.Micro\bin\Release\Neurbot.Micro.exe -brain {0} -history {1}",
                        brainFileName, botHistoryFileName);
                }
            }
        }

        private static GameResult RunGame(string ticksFolder, string bot1Folder, string bot2Folder)
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
                            id = 0,
                            name = "bot1",
                            bot = bot1Folder,
                            ufos = new[] { 11 },
                            color = "#fffed800",
                            hue = 0.142
                        },
                        new {
                            id = 1,
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
            int[] nrOfUnitsInLayers = new[] { 240, 480, 480, 120, 16 };
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
