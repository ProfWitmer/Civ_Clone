using CivClone.Infrastructure;
using CivClone.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CivClone.Tests
{
    public class Phase2GameplayTests
    {
        [Test]
        public void Unit_ResetMovementRestoresMovementRemaining()
        {
            var unit = new Unit("scout", new GridPosition(0, 0), 2, 0)
            {
                MovementRemaining = 0
            };

            unit.ResetMovement();

            Assert.AreEqual(2, unit.MovementRemaining);
        }

        [Test]
        public void TurnSystem_CompletesBuildingWhenProductionMet()
        {
            var map = new WorldMap(2, 2);
            map.Tiles.Add(new Tile(new GridPosition(0, 0), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(1, 0), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(0, 1), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(1, 1), "plains"));

            var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
            var building = ScriptableObject.CreateInstance<BuildingType>();
            building.Id = "granary";
            building.ProductionCost = 10;
            catalog.BuildingTypes = new[] { building };

            var state = new GameState { Map = map };
            var player = new Player(0, "Player");
            var city = new City("City", new GridPosition(0, 0), player.Id, 1)
            {
                ProductionStored = 10,
                ProductionCost = 10,
                ProductionTargetId = "granary"
            };
            player.Cities.Add(city);
            state.Players.Add(player);

            var turnSystem = new TurnSystem(state, catalog);
            turnSystem.EndTurn();

            Assert.Contains("granary", city.Buildings);
        }
    }
}
