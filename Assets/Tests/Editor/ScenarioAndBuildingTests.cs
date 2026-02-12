using System.Collections.Generic;
using CivClone.Infrastructure;
using CivClone.Presentation;
using CivClone.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CivClone.Tests
{
    public class ScenarioAndBuildingTests
    {
        [Test]
        public void TurnSystem_AppliesBuildingBonuses()
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
            var granary = ScriptableObject.CreateInstance<BuildingType>();
            granary.Id = "granary";
            granary.FoodBonus = 2;
            catalog.BuildingTypes = new[] { granary };

            var state = new GameState { Map = map };
            var player = new Player(0, "Player");
            var city = new City("City", new GridPosition(1, 1), player.Id, 1);
            city.Buildings = new List<string> { "granary" };
            player.Cities.Add(city);
            state.Players.Add(player);

            var turnSystem = new TurnSystem(state, catalog);
            turnSystem.EndTurn();

            Assert.AreEqual(2, city.BuildingFoodBonus);
            Assert.AreEqual(3, city.FoodPerTurn);
        }

        [Test]
        public void ScenarioHooks_GrantTechEvent()
        {
            var map = new WorldMap(2, 2);
            map.Tiles.Add(new Tile(new GridPosition(0, 0), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(1, 0), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(0, 1), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(1, 1), "plains"));

            var state = new GameState { Map = map };
            var player = new Player(0, "Player");
            state.Players.Add(player);

            var scenario = new ScenarioDefinition
            {
                Events = new List<ScenarioEventDefinition>
                {
                    new ScenarioEventDefinition
                    {
                        Id = "grant",
                        Turn = 1,
                        Type = "grant_tech",
                        TargetPlayerId = 0,
                        TechId = "archery"
                    }
                }
            };

            var obj = new GameObject("ScenarioHooks");
            var hooks = obj.AddComponent<ScenarioHooksController>();
            var turnSystem = new TurnSystem(state, ScriptableObject.CreateInstance<GameDataCatalog>());
            hooks.Bind(scenario, state, turnSystem, null, null);

            state.CurrentTurn = 1;
            obj.SendMessage("Update");

            Assert.Contains("archery", player.KnownTechs);
        }

        [Test]
        public void ScenarioHooks_SpawnUnitEvent()
        {
            var map = new WorldMap(2, 2);
            map.Tiles.Add(new Tile(new GridPosition(0, 0), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(1, 0), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(0, 1), "plains"));
            map.Tiles.Add(new Tile(new GridPosition(1, 1), "plains"));

            var state = new GameState { Map = map };
            var player = new Player(0, "Player");
            state.Players.Add(player);

            var scenario = new ScenarioDefinition
            {
                Events = new List<ScenarioEventDefinition>
                {
                    new ScenarioEventDefinition
                    {
                        Id = "spawn",
                        Turn = 1,
                        Type = "spawn_unit",
                        TargetPlayerId = 0,
                        UnitTypeId = "warrior",
                        X = 0,
                        Y = 1
                    }
                }
            };

            var obj = new GameObject("ScenarioHooks");
            var hooks = obj.AddComponent<ScenarioHooksController>();
            var turnSystem = new TurnSystem(state, ScriptableObject.CreateInstance<GameDataCatalog>());
            hooks.Bind(scenario, state, turnSystem, null, null);

            state.CurrentTurn = 1;
            obj.SendMessage("Update");

            Assert.IsTrue(player.Units.Exists(u => u.UnitTypeId == "warrior"));
        }
    }
}
