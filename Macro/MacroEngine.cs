using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MacroBot.Protocol;
using static MacroBot.Helpers;

namespace MacroBot
{
    public sealed class MacroEngine : Swoc.Engine<Protocol.GameState>
    {
        private const string playerName = "playerName";

        private const int UfoCost = 100000;

        public override void Response(List<GameState> gameStates)
        {
            try
            {
                DoResponse(gameStates);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Response failure: " + ex.Message);
            }
        }

        private readonly Dictionary<int, SolarPlanet> mUfoMovingToPlanet = new Dictionary<int, SolarPlanet>();
        private readonly Dictionary<int, SolarPlanet> mUfoInOrbitAtPlanet = new Dictionary<int, SolarPlanet>();
        private readonly HashSet<int> recentUfos = new HashSet<int>();

        private void DoResponse(List<GameState> gameStates)
        {
            var gameState = gameStates.Last();
            var mePlayer = GetPlayerByName(gameState.Players, playerName);

            RemoveDestroyedUfos(mePlayer);
            BuyUfos(gameState, mePlayer);
            MoveToPlanets(gameState, mePlayer);
            StartConquer(gameState, mePlayer);
            FinalizeConquer(gameState, mePlayer);
        }

        private void FinalizeConquer(GameState gameState, Player mePlayer)
        {
            List<int> toRemove = new List<int>();
            foreach (var kvp in mUfoInOrbitAtPlanet)
            {
                var ufo = GetUfoById(mePlayer, kvp.Key);
                var solarSystem = GetSolarSystemByid(gameState.SolarSystems, kvp.Value.SolarSystemId);
                var planet = GetPlanetById(solarSystem, kvp.Value.PlanetId);
                if (!ufo.InFight && planet.OwnedBy == mePlayer.Id)
                    toRemove.Add(kvp.Key);
            }

            foreach (var ufoId in toRemove)
                mUfoInOrbitAtPlanet.Remove(ufoId);
        }

        private void StartConquer(GameState gameState, Player mePlayer)
        {
            List<int> toRemove = new List<int>();
            foreach (var kvp in mUfoMovingToPlanet)
            {
                var solarSystem = GetSolarSystemByid(gameState.SolarSystems, kvp.Value.SolarSystemId);
                var planet = GetPlanetById(solarSystem, kvp.Value.PlanetId);
                var ufo = GetUfoById(mePlayer, kvp.Key);
                if (InOrbit(solarSystem, planet, ufo))
                {
                    toRemove.Add(kvp.Key);
                    mUfoInOrbitAtPlanet[kvp.Key] = kvp.Value;
                    WriteMessage(new Protocol.GameResponseConquer { PlanetId = kvp.Value.PlanetId });
                }
            }

            foreach (var ufoId in toRemove)
                mUfoMovingToPlanet.Remove(ufoId);
        }

        private void MoveToPlanets(GameState gameState, Player mePlayer)
        {
            List<Planet> excludedPlanets = gameState.SolarSystems.SelectMany(ss => ss.Planets).Where(p => p.OwnedBy == mePlayer.Id).ToList();
            excludedPlanets.AddRange(mUfoMovingToPlanet.Select(kvp => GetPlanetById(GetSolarSystemByid(gameState.SolarSystems, kvp.Value.SolarSystemId), kvp.Value.PlanetId)));

            foreach (var ufo in mePlayer.Ufos.Where(u => !mUfoMovingToPlanet.ContainsKey(u.Id) && !mUfoInOrbitAtPlanet.ContainsKey(u.Id)))
            {
                var planet = RandomPlanet(gameState.SolarSystems, ufo, excludedPlanets);
                if (planet == default(Planet))
                    continue;

                excludedPlanets.Add(planet);
                mUfoMovingToPlanet[ufo.Id] = new SolarPlanet(GetSolarSystem(gameState.SolarSystems, planet), planet);
                WriteMessage(new Protocol.GameResponseMoveToPlanet
                {
                    PlanetId = planet.Id,
                    Ufos = new List<int> { ufo.Id, },
                });
            }
        }

        private void BuyUfos(GameState gameState, Player mePlayer)
        {
            const int MaxUfos = 20;
            if (mePlayer.Ufos.Count > MaxUfos)
                return;

            var amount = (int)(mePlayer.Credits / UfoCost);
            if (amount < 1)
                return;

            var planet = gameState.SolarSystems.SelectMany(ss => ss.Planets).FirstOrDefault(p => p.OwnedBy == mePlayer.Id);

            WriteMessage(new Protocol.GameResponseBuy
            {
                PlanetId = planet != default(Planet) ? planet.Id : -1,
                Amount = amount,
            });
        }

        private void RemoveDestroyedUfos(Player mePlayer)
        {
            var ufoIds = mePlayer.Ufos.Select(ufo => ufo.Id);
            recentUfos.ExceptWith(ufoIds);
            foreach (var ufoId in recentUfos)
            {
                mUfoMovingToPlanet.Remove(ufoId);
                mUfoInOrbitAtPlanet.Remove(ufoId);
            }
            recentUfos.Clear();
            recentUfos.UnionWith(ufoIds);
        }

        public class SolarPlanet
        {
            public SolarPlanet(SolarSystem solarSystem, Planet planet)
            {
                solarSystemId = solarSystem.Id;
                planetId = planet.Id;
            }

            private int solarSystemId;
            public int SolarSystemId { get { return solarSystemId; } }

            private int planetId;
            public int PlanetId { get { return planetId; } }

            public override bool Equals(object obj)
            {
                var other = obj as SolarPlanet;
                if (other == default(SolarPlanet))
                    return false;

                return other.planetId == planetId && other.solarSystemId == solarSystemId;
            }

            public override int GetHashCode()
            {
                return 3 * planetId.GetHashCode() ^ 7 * solarSystemId.GetHashCode();
            }
        }

    }
}
