using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroBot.Protocol
{
    public sealed class MoveTo
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
    }
}
