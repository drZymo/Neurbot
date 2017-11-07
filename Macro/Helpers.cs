using MacroBot.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Swoc;

namespace MacroBot
{
    public static class Helpers
    {
        public static Player GetPlayerByName(List<Player> players, string name)
        {
            return players.Single(player => player.Name == name);
        }

        public static SolarSystem GetSolarSystemByid(List<SolarSystem> solarSystems, int solarSystemId)
        {
            return solarSystems.Single(ss => ss.Id == solarSystemId);
        }

        public static Planet GetPlanetById(List<SolarSystem> solarSystems, int planetId)
        {
            return solarSystems.SelectMany(ss => ss.Planets).Single(p => p.Id == planetId);
        }

        public static Planet GetPlanetById(SolarSystem solarSystem, int planetId)
        {
            return solarSystem.Planets.Single(p => p.Id == planetId);
        }

        public static SolarSystem GetSolarSystem(List<SolarSystem> solarSystems, Planet planet)
        {
            return solarSystems.Single(ss => ss.Planets.Any(p => p.Id == planet.Id));
        }

        public static Ufo GetUfoById(Player player, int ufoId)
        {
            return player.Ufos.Single(ufo => ufo.Id == ufoId);
        }

        public static Planet NearestPlanet(List<SolarSystem> solarSystems, Ufo ufo, List<Planet> excluded)
        {
            var ufoCc = new CartesianCoord(ufo.Coord.X, ufo.Coord.Y);
            var planets = GetPlanetCoords(solarSystems);
            var orderedPlanetCoord = planets.Where(kvp => !excluded.Any(ex => ex.Id == kvp.Key.Id))
                          .OrderBy(kvp => (kvp.Value - ufoCc).LengthSquared());

            return orderedPlanetCoord.Any() ? orderedPlanetCoord.First().Key : null;
        }

        public static Planet RandomPlanet(List<SolarSystem> solarSystems, Ufo ufo, List<Planet> excluded)
        {
            var planets = solarSystems.SelectMany(ss => ss.Planets).Where(p => !excluded.Any(ex => ex.Id == p.Id));
            var count = planets.Count();
            return count > 0 ? planets.ElementAt(new Random(DateTime.Now.Millisecond).Next(count -1)) : null;
        }

        public static Dictionary<Planet, CartesianCoord> GetPlanetCoords(List<SolarSystem> solarSystems)
        {
            Dictionary<Planet, CartesianCoord> res = new Dictionary<Planet, CartesianCoord>();
            foreach (var solarSystem in solarSystems)
                foreach (var planet in solarSystem.Planets)
                    res[planet] = GetPlanetCoord(solarSystem, planet);
            return res;
        }

        public static Dictionary<Planet, CartesianCoord> GetPlanetCoords(SolarSystem solarSystem)
        {
            return solarSystem.Planets.ToDictionary(kvp => kvp, kvp => GetPlanetCoord(solarSystem, kvp));
        }

        public static bool InOrbit(SolarSystem solarSystem, Planet planet, Ufo ufo)
        {
            var ufoCc = new CartesianCoord(ufo.Coord.X, ufo.Coord.Y);
            return InOrbit(ufoCc, GetPlanetCoord(solarSystem, planet));
        }

        public static bool InOrbit(CartesianCoord ufo, CartesianCoord planet)
        {
            return ((planet - ufo).LengthSquared() < 256 * 256);
        }

        public static CartesianCoord GetPlanetCoord(SolarSystem solarSystem, Planet planet)
        {
            var planetCoord = PolarCoord.AsCartesianCoord(planet.OrbitDistance, planet.OrbitRotation, PolarCoord.AngleType.Degree);
            return new CartesianCoord(solarSystem.Coords.X + planetCoord.X, solarSystem.Coords.Y + planetCoord.Y);
        }
    }
}
