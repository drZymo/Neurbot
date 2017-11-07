using System.Collections.Generic;

namespace MacroBot.Protocol
{
    public sealed class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Credits { get; set; }
        public List<Ufo> Ufos { get; set; }
    }
}