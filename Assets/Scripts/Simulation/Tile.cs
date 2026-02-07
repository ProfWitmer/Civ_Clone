using System;

namespace CivClone.Simulation
{
    [Serializable]
    public class Tile
    {
        public GridPosition Position;
        public string TerrainId;
        public string ImprovementId;
        public bool Explored;
        public bool Visible;
        public bool HasRoad;

        public Tile(GridPosition position, string terrainId)
        {
            Position = position;
            TerrainId = terrainId;
            ImprovementId = string.Empty;
            Explored = false;
            Visible = false;
            HasRoad = false;
        }
    }
}
