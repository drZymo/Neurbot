using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MacroBot.Protocol
{
    public sealed class GameState
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Tick { get; set; }
        public List<SolarSystem> SolarSystems { get; set; }
        public List<Player> Players { get; set; }
        public List<Fight> Fights { get; set; }
    }
}
