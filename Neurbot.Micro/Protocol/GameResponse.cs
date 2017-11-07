using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroBot.Protocol
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
