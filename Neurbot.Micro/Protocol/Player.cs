using System.Collections.Generic;

namespace Neurbot.Micro.Protocol
{
    public sealed class Player
    {
        public string Name { get; set; }
        public List<Ufo> Ufos { get; set; }
    }
}
