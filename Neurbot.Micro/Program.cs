using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new MicroEngine();
            engine.Run();
        }
    }
}
