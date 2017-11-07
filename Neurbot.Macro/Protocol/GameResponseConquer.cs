using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MacroBot.Protocol
{
    public sealed class GameResponseConquer
    {
        public string Command { get { return "conquer"; } }
        public int PlanetId { get; set; }
    }
}
