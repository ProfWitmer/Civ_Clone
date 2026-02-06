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
                    string terrainId = _config.DefaultTerrainId;
                    double roll = random.NextDouble();
                    if (roll > 0.9)
                    {
                        terrainId = "hills";
                    }

                    bool edgeWater = x == 0 || y == 0 || x == _config.Width - 1 || y == _config.Height - 1;
                    if (edgeWater || roll < 0.08)
                    {
                        terrainId = "water";
                    }

                    map.Tiles.Add(new Tile(new GridPosition(x, y), terrainId));
                }
            }

            return map;
        }
    }
}
