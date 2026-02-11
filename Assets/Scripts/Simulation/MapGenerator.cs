using System;

namespace CivClone.Simulation
{
    public class MapGenerator
    {
        private readonly MapConfig _config;
        private const string TerrainPlains = "plains";
        private const string TerrainHills = "hills";
        private const string TerrainWater = "water";
        private const string ResourceWheat = "wheat";
        private const string ResourceHorses = "horses";
        private const string ResourceIron = "iron";
        private const string ResourceCopper = "copper";
        private const string ResourceGems = "gems";
        private const string ResourceFish = "fish";

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
                        terrainId = TerrainHills;
                    }

                    bool edgeWater = x == 0 || y == 0 || x == _config.Width - 1 || y == _config.Height - 1;
                    if (edgeWater || roll < 0.08)
                    {
                        terrainId = TerrainWater;
                    }

                    var tile = new Tile(new GridPosition(x, y), terrainId);
                    double resourceRoll = random.NextDouble();
                    if (terrainId == TerrainPlains && resourceRoll > 0.93)
                    {
                        tile.ResourceId = ResourceWheat;
                    }
                    else if (terrainId == TerrainPlains && resourceRoll > 0.9)
                    {
                        tile.ResourceId = ResourceHorses;
                    }
                    else if (terrainId == TerrainHills && resourceRoll > 0.92)
                    {
                        tile.ResourceId = ResourceIron;
                    }
                    else if (terrainId == TerrainHills && resourceRoll > 0.88)
                    {
                        tile.ResourceId = ResourceCopper;
                    }
                    else if (terrainId == TerrainHills && resourceRoll > 0.85)
                    {
                        tile.ResourceId = ResourceGems;
                    }
                    else if (terrainId == TerrainWater && resourceRoll > 0.93)
                    {
                        tile.ResourceId = ResourceFish;
                    }

                    map.Tiles.Add(tile);
                }
            }

            return map;
        }
    }
}
