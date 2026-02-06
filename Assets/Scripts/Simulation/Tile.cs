using System;

namespace CivClone.Simulation
{
    [Serializable]
    public class Tile
    {
        public GridPosition Position;
        public string TerrainId;
        public bool Explored;
        public bool Visible;

        public Tile(GridPosition position, string terrainId)
        {
            Position = position;
            TerrainId = terrainId;
            Explored = false;
            Visible = false;
        }
    }
}
