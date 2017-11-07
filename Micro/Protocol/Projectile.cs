using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Bot.Protocol;

namespace MicroBot.Protocol
{
    public class Projectile
    { 
        public Position Position { get; set; }
        public float Direction { get; set; }
    }
}
