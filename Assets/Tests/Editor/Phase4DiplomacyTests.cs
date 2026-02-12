using System.Reflection;
using CivClone.Presentation;
using CivClone.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CivClone.Tests
{
    public class Phase4DiplomacyTests
    {
        [Test]
        public void GameBootstrap_InitializesDiplomacyEntries()
        {
            var map = new WorldMap(1, 1);
            map.Tiles.Add(new Tile(new GridPosition(0, 0), "plains"));

            var state = new GameState { Map = map };
            state.Players.Add(new Player(0, "Player 1"));
            state.Players.Add(new Player(1, "Rival"));

            var obj = new GameObject("Bootstrap");
            var bootstrap = obj.AddComponent<GameBootstrap>();

            var method = typeof(GameBootstrap).GetMethod("InitializeDiplomacy", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "InitializeDiplomacy method not found");
            method.Invoke(bootstrap, new object[] { state });

            Assert.AreEqual(1, state.Players[0].Diplomacy.Count);
            Assert.AreEqual(1, state.Players[1].Diplomacy.Count);
        }
    }
}
