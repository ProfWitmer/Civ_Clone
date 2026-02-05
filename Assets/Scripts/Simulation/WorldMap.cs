using System;
using System.Collections.Generic;

namespace CivClone.Simulation
{
    [Serializable]
    public class WorldMap
    {
        public int Width;
        public int Height;
        public List<Tile> Tiles = new List<Tile>();

        public WorldMap(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Tile GetTile(int x, int y)
        {
            int index = y * Width + x;
            return index >= 0 && index < Tiles.Count ? Tiles[index] : null;
        }
    }
}
