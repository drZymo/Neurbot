using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroBot.Protocol
{
    public sealed class Player
    {
        public string Name { get; set; }
        public List<Ufo> Ufos { get; set; }
    }
}
