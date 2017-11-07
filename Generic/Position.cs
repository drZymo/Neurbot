using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Protocol
{
    public struct Position
    {
        public float X { get; set; }
        public float Y { get; set; }

        public override string ToString()
        {
            return String.Format("{0},{1} (X,Y)", X, Y);
        }
    }
}
