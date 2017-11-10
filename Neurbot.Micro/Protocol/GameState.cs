using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;

namespace Neurbot.Micro.Protocol
{
    public sealed class GameState
    {
        public Arena Arena { get; set; }

        public List<Player> Players { get; set; }

        public List<Projectile> Projectiles { get; set; }

        public int PlayerId { get; set; }

        public string PlayerName { get; set; }

        public Vector<double> ToNeuralNetInput()
        {
            const int MaxNrOfFriendlyUfos = 10;
            const int MaxNrOfEnemyUfos = 20;
            const int MaxNrOfProjectiles = 50;

            const int ArenaOffset = 0;
            const int ArenaSize = 2;
            const int FriendlyUfosOffset = ArenaOffset + ArenaSize;
            const int FriendlyUfosSize = MaxNrOfFriendlyUfos * 3;
            const int EnemyUfosOffset = FriendlyUfosOffset + FriendlyUfosSize;
            const int EnemyUfosSize = MaxNrOfEnemyUfos * 3;
            const int ProjectilesOffset = EnemyUfosOffset + EnemyUfosSize;
            const int ProjectilesSize = MaxNrOfProjectiles * 3;

            var input = new double[ArenaSize + FriendlyUfosSize + EnemyUfosSize + ProjectilesSize];

            var friendlyUfos = Players.Where(player => player.Name == PlayerName).SelectMany(player => player.Ufos).ToArray();
            var enemyUfos = Players.Where(player => player.Name != PlayerName).SelectMany(player => player.Ufos).ToArray();

            // first block is ufos of this player
            for (int i = 0; i < MaxNrOfFriendlyUfos; i++)
            {
                double hitPoints = 0.0;
                double x = 0.0;
                double y = 0.0;
                if (i < friendlyUfos.Length)
                {
                    var ufo = friendlyUfos[i];
                    hitPoints = ufo.Hitpoints;
                    x = ufo.Position.X;
                    y = ufo.Position.Y;
                }
                input[FriendlyUfosOffset + i * 3 + 0] = hitPoints;
                input[FriendlyUfosOffset + i * 3 + 1] = x;
                input[FriendlyUfosOffset + i * 3 + 2] = y;
            }

            // Enemy ufos
            for (int i = 0; i < MaxNrOfEnemyUfos; i++)
            {
                double hitPoints = 0.0;
                double x = 0.0;
                double y = 0.0;
                if (i < enemyUfos.Length)
                {
                    var ufo = enemyUfos[i];
                    hitPoints = ufo.Hitpoints;
                    x = ufo.Position.X;
                    y = ufo.Position.Y;
                }
                input[EnemyUfosOffset + i * 3 + 0] = hitPoints;
                input[EnemyUfosOffset + i * 3 + 1] = x;
                input[EnemyUfosOffset + i * 3 + 2] = y;
            }

            // projectiles
            for (int i = 0; i < MaxNrOfProjectiles; i++)
            {
                double x = 0.0;
                double y = 0.0;
                double direction = 0.0;
                if (i < Projectiles.Count)
                {
                    var projectile = Projectiles[i];
                    x = projectile.Position.X;
                    y = projectile.Position.Y;
                    direction = projectile.Direction;
                }
                input[ProjectilesOffset + i * 3 + 1] = x;
                input[ProjectilesOffset + i * 3 + 2] = y;
                input[ProjectilesOffset + i * 3 + 0] = direction;
            }

            return Vector<double>.Build.Dense(input);
        }
    }
}
