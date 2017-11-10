using System.IO;

namespace Neurbot.Micro
{
    internal static class Logger
    {
        public static void Log(string format, params object[] arg)
        {
            using (var writer = new StreamWriter(@"D:\Swoc2017\log.txt", true))
            {
                writer.WriteLine(format, arg);
            }
        }
    }
}
