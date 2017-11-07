using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MacroBot.Protocol
{
    public sealed class GameResponseBuy
    {
        public string Command { get { return "buy"; } }
        public int Amount { get; set; }
        public int PlanetId { get; set; }
    }
}
