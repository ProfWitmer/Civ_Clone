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
                        terrainId = hills;
                    }

                    bool edgeWater = x == 0 || y == 0 || x == _config.Width - 1 || y == _config.Height - 1;
                    if (edgeWater || roll < 0.08)
                    {
                        terrainId = water;
                    }

                    var tile = new Tile(new GridPosition(x, y), terrainId);
                    double resourceRoll = random.NextDouble();
                    if (terrainId == plains && resourceRoll > 0.93)
                    {
                        tile.ResourceId = wheat;
                    }
                    else if (terrainId == plains && resourceRoll > 0.9)
                    {
                        tile.ResourceId = horses;
                    }
                    else if (terrainId == hills && resourceRoll > 0.92)
                    {
                        tile.ResourceId = iron;
                    }
                    else if (terrainId == hills && resourceRoll > 0.88)
                    {
                        tile.ResourceId = copper;
                    }
                    else if (terrainId == hills && resourceRoll > 0.85)
                    {
                        tile.ResourceId = gems;
                    }
                    else if (terrainId == water && resourceRoll > 0.93)
                    {
                        tile.ResourceId = fish;
                    }

                    map.Tiles.Add(tile);
                }
            }

            return map;
        }
    }
}
