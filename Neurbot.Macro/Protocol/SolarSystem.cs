using Neurbot.Generic;
using System.Collections.Generic;

namespace MacroBot.Protocol
{
    public sealed class SolarSystem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Position Coords { get; set; }
        public List<Planet> Planets { get; set; }
    }
}