using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Bot.Protocol;

namespace MicroBot.Protocol
{
    public sealed class Ufo
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public float Hitpoints { get; set; }
        public Position Position { get; set; }
    }
}
