using System.Linq;
using CivClone.Infrastructure;
using CivClone.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CivClone.Tests
{
    public class SimulationTests
    {
        [Test]
        public void FogOfWar_RevealsTilesAroundUnit()
        {
            var map = new WorldMap(7, 7);
            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    map.Tiles.Add(new Tile(new GridPosition(x, y), "plains"));
                }
            }

            var state = new GameState { Map = map };
            var player = new Player(0, "Player");
            player.Units.Add(new Unit("scout", new GridPosition(3, 3), 2, player.Id));
            state.Players.Add(player);

            var fog = new FogOfWarSystem(2);
            fog.Apply(state);

            Assert.IsTrue(map.GetTile(3, 3).Visible);
            Assert.IsTrue(map.GetTile(1, 1).Explored);
            Assert.IsFalse(map.GetTile(0, 0).Visible);
        }

        [Test]
        public void TurnSystem_GrowsCityWhenFoodThresholdReached()
        {
            var map = new WorldMap(3, 3);
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    map.Tiles.Add(new Tile(new GridPosition(x, y), "plains"));
                }
            }

            var state = new GameState { Map = map };
            var player = new Player(0, "Player");
            var city = new City("City", new GridPosition(1, 1), player.Id, 1)
            {
                FoodStored = 6,
                FoodPerTurn = 1
            };
            player.Cities.Add(city);
            state.Players.Add(player);

            var turnSystem = new TurnSystem(state);
            turnSystem.EndTurn();

            Assert.AreEqual(2, city.Population);
            Assert.AreEqual(0, city.FoodStored);
        }

        [Test]
        public void TurnSystem_SpawnsUnitWhenProductionMet()
        {
            var map = new WorldMap(3, 3);
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    map.Tiles.Add(new Tile(new GridPosition(x, y), "plains"));
                }
            }

            var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
            var unitType = ScriptableObject.CreateInstance<UnitType>();
            unitType.Id = "scout";
            unitType.MovementPoints = 2;
            catalog.UnitTypes = new[] { unitType };

            var state = new GameState { Map = map };
            var player = new Player(0, "Player");
            var city = new City("City", new GridPosition(1, 1), player.Id, 1)
            {
                ProductionStored = 10,
                ProductionCost = 10,
                ProductionTargetId = "scout"
            };
            player.Cities.Add(city);
            state.Players.Add(player);

            var turnSystem = new TurnSystem(state, catalog);
            turnSystem.EndTurn();

            Assert.IsTrue(player.Units.Any(u => u.UnitTypeId == "scout"));
        }

        [Test]
        public void ResearchSystem_CompletesTechWhenCostMet()
        {
            var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
            var tech = ScriptableObject.CreateInstance<TechType>();
            tech.Id = "agriculture";
            tech.DisplayName = "Agriculture";
            tech.Cost = 2;
            catalog.TechTypes = new[] { tech };

            var player = new Player(0, "Player");
            player.Cities.Add(new City("City", new GridPosition(0, 0), player.Id, 1));

            var research = new ResearchSystem(catalog);
            research.Advance(player);
            research.Advance(player);

            Assert.Contains("agriculture", player.KnownTechs);
        }
    }
}
