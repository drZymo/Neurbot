using Neurbot.Generic;
using System.Collections.Generic;

namespace MacroBot.Protocol
{
    public sealed class GameResponseMoveToCoord
    {
        public string Command { get { return "moveToCoord"; } }
        public List<int> Ufos { get; set; }
        public Position Coord { get; set; }
    }
}
