
using Neurbot.Generic;

namespace MacroBot.Protocol
{
    public sealed class Ufo
    {
        public int Id { get; set; }
        public bool InFight { get; set; }
        public Position Coord { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Ufo;
            if (other == default(Ufo))
                return false;

            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}