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

            const int FriendlyUfosOffset = 0;
            const int FriendlyUfosSize = MaxNrOfFriendlyUfos * 3;
            const int EnemyUfosOffset = FriendlyUfosOffset + FriendlyUfosSize;
            const int EnemyUfosSize = MaxNrOfEnemyUfos * 3;
            const int ProjectilesOffset = EnemyUfosOffset + EnemyUfosSize;
            const int ProjectilesSize = MaxNrOfProjectiles * 3;

            var input = new double[FriendlyUfosSize + EnemyUfosSize + ProjectilesSize];

            var friendlyUfos = Players.Where(player => player.Name == PlayerName).SelectMany(player => player.Ufos).ToArray();
            var enemyUfos = Players.Where(player => player.Name != PlayerName).SelectMany(player => player.Ufos).ToArray();

            var arenaWidth = Arena.Width;
            var arenaHeight = Arena.Height;

            // Data is normalize to be all (more or less) within the range [0...1].
            // This will prevent exploding values in the softmax layer.
            // This also makes the positions always between 0 and 1, even when the arena is shrinking.

            // first block is ufos of this player
            for (int i = 0; i < MaxNrOfFriendlyUfos; i++)
            {
                double hitPoints = 0.0;
                double x = 0.0;
                double y = 0.0;
                if (i < friendlyUfos.Length)
                {
                    var ufo = friendlyUfos[i];
                    hitPoints = ufo.Hitpoints / 100.0;
                    x = ufo.Position.X / arenaWidth;
                    y = ufo.Position.Y / arenaHeight;
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
                    hitPoints = ufo.Hitpoints / 100.0;
                    x = ufo.Position.X / arenaWidth;
                    y = ufo.Position.Y / arenaHeight;
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
                    x = projectile.Position.X / arenaWidth;
                    y = projectile.Position.Y / arenaHeight;
                    direction = projectile.Direction / 360.0;
                }
                input[ProjectilesOffset + i * 3 + 1] = x;
                input[ProjectilesOffset + i * 3 + 2] = y;
                input[ProjectilesOffset + i * 3 + 0] = direction;
            }

            return Vector<double>.Build.Dense(input);
        }
    }
}
