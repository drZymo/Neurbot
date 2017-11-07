using Neurbot.Generic;

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
