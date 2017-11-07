using System.Collections.Generic;

namespace MacroBot.Protocol
{
    public sealed class Fight
    {
        public int Id { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public int PlanetId { get; set; }
        public List<int> Player1UfoIds { get; set; }
        public List<int> Player2UfoIds { get; set; }
    }
}