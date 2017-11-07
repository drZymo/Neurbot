using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Swoc
{
    public static class Win32IO
    {
        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;

        static Win32IO()
        {
            Encoding encoding = Encoding.ASCII;

            var stdIn = GetStdHandle(STD_INPUT_HANDLE);
            StreamReader standardInput = new StreamReader(new FileStream(new SafeFileHandle(stdIn, true), FileAccess.Read), encoding);
            Console.SetIn(standardInput);

            var stdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            StreamWriter standardOutput = new StreamWriter(new FileStream(new SafeFileHandle(stdOut, true), FileAccess.Write), encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);

            var stdError = GetStdHandle(STD_ERROR_HANDLE);
            StreamWriter standardError = new StreamWriter(new FileStream(new SafeFileHandle(stdError, true), FileAccess.Write), encoding);
            standardError.AutoFlush = true;
            Console.SetError(standardError);
        }

        public static void Init()
        {
        }
    }
}
