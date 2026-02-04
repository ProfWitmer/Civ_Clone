using System.Collections.Generic;

namespace CivClone.Simulation.Map
{
    public sealed class MapGrid
    {
        private readonly Dictionary<(int x, int y), Tile> _tiles = new();

        public void AddTile(Tile tile)
        {
            _tiles[(tile.X, tile.Y)] = tile;
        }

        public bool TryGetTile(int x, int y, out Tile tile)
        {
            return _tiles.TryGetValue((x, y), out tile);
        }
    }
}
