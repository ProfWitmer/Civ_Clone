using System;
using System.Collections.Generic;

namespace CivClone.Simulation
{
    [Serializable]
    public class GameState
    {
        public WorldMap Map;
        public List<Player> Players = new List<Player>();
        public int CurrentTurn = 1;
        public int ActivePlayerIndex = 0;
        public string ScenarioId = string.Empty;
        public int ScenarioLastTurn = -1;
        public List<string> ScenarioFiredEvents = new List<string>();
        public bool ScenarioComplete;

        public Player ActivePlayer =>
            ActivePlayerIndex >= 0 && ActivePlayerIndex < Players.Count ? Players[ActivePlayerIndex] : null;
    }
}
