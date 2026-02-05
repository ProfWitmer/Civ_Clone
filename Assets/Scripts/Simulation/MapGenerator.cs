using System;

namespace CivClone.Simulation
{
    public class MapGenerator
    {
        private readonly MapConfig _config;

        public MapGenerator(MapConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public WorldMap Generate()
        {
            var map = new WorldMap(_config.Width, _config.Height);
            var random = new System.Random(_config.Seed);

            for (int y = 0; y < _config.Height; y++)
            {
                for (int x = 0; x < _config.Width; x++)
                {
                    string terrainId = random.NextDouble() > 0.8 ? "hills" : _config.DefaultTerrainId;
                    map.Tiles.Add(new Tile(new GridPosition(x, y), terrainId));
                }
            }

            return map;
        }
    }
}
