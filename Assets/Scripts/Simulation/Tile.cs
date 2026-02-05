using System;

namespace CivClone.Simulation
{
    [Serializable]
    public class Tile
    {
        public GridPosition Position;
        public string TerrainId;

        public Tile(GridPosition position, string terrainId)
        {
            Position = position;
            TerrainId = terrainId;
        }
    }
}
