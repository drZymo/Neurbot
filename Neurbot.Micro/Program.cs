using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MicroBot
{
    class Program
    {
        static void Main(string[] args)
        {
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

            using (var writer = new StreamWriter(@"D:\Swoc2017\log.txt", true))
            {
                writer.WriteLine("brain = {0}, history = {1}", brainFileName, historyFileName);
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
