using Neurbot.Micro.Protocol;
using System.Collections.Generic;
using System.Linq;

namespace Neurbot.Micro
{
    public static class Helpers
    {
        public static Player GetPlayerByName(IEnumerable<Player> players, string name)
        {
            return players.Single(player => player.Name == name);
        }

        public static IEnumerable<Protocol.Ufo> GetUfosWithHitPoints(IEnumerable<Protocol.Ufo> ufos)
        {
            return ufos.Where(b => b.Hitpoints > 0);
        }

        public static IEnumerable<Protocol.Ufo> GetOtherUfos(GameState gameState, string playerName)
        {
            return gameState.Players.Where(p => p.Name != playerName).SelectMany(p => p.Ufos);
        }
    }
}
