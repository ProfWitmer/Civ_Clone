using CivClone.Infrastructure;
using CivClone.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CivClone.Tests
{
    public class Phase3EconomyTests
    {
        [Test]
        public void ResearchSystem_UsesResourceScienceBonus()
        {
            var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
            var tech = ScriptableObject.CreateInstance<TechType>();
            tech.Id = "agriculture";
            tech.Cost = 10;
            catalog.TechTypes = new[] { tech };

            var resource = ScriptableObject.CreateInstance<ResourceType>();
            resource.Id = "gems";
            resource.ScienceBonus = 2;
            catalog.ResourceTypes = new[] { resource };

            var state = new GameState
            {
                Map = new WorldMap(1, 1)
            };
            state.Map.Tiles.Add(new Tile(new GridPosition(0, 0), "plains"));

            var player = new Player(0, "Player");
            player.Cities.Add(new City("City", new GridPosition(0, 0), player.Id, 1));
            player.AvailableResources.Add("gems");
            player.CurrentTechId = "agriculture";
            state.Players.Add(player);

            var research = new ResearchSystem(catalog);
            research.Advance(player);

            Assert.AreEqual(1 + player.Cities.Count + 2, player.ResearchProgress);
        }
    }
}
