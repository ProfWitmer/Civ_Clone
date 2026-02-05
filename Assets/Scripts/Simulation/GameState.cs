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

        public Player ActivePlayer =>
            ActivePlayerIndex >= 0 && ActivePlayerIndex < Players.Count ? Players[ActivePlayerIndex] : null;
    }
}
