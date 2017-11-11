using System.IO;
using System.Reflection;

namespace Neurbot.Micro
{
    class Program
    {
        static void Main(string[] args)
        {
            MathNet.Numerics.Control.UseNativeMKL();

            string brainFileName = @"brain.dat";
            string historyFileName = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i].ToLower() == "-brain") && (i < args.Length - 1))
                {
                    brainFileName = args[i + 1];
                }
                else if ((args[i].ToLower() == "-history") && (i < args.Length - 1))
                {
                    historyFileName = args[i + 1];
                }
            }
            brainFileName = GetAbsolutePath(brainFileName);
            if (!string.IsNullOrEmpty(historyFileName))
            {
                historyFileName = GetAbsolutePath(historyFileName);
            }

            var engine = new MicroEngine(brainFileName, historyFileName);
            engine.Run();
        }

        private static string GetAbsolutePath(string fileName)
        {
            // If path is relative, then make it relative to this executable.
            if (!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
            }

            return Path.GetFullPath(fileName);
        }
    }
}
