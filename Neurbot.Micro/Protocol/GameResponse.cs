using System.Collections.Generic;

namespace Neurbot.Micro.Protocol
{
    public sealed class GameResponse
    {
        public GameResponse()
        {
            Commands = new List<UfoAction>();
        }

        public List<UfoAction> Commands { get; set; }
    }
}
