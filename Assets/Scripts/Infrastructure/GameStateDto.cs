using System;
using System.Collections.Generic;
using CivClone.Simulation;

namespace CivClone.Infrastructure
{
    [Serializable]
    public class GameStateDto
    {
        public int CurrentTurn;
        public int ActivePlayerIndex;
        public int MapWidth;
        public int MapHeight;
        public List<TileDto> Tiles = new List<TileDto>();
        public List<PlayerDto> Players = new List<PlayerDto>();

        public static GameStateDto FromState(GameState state)
        {
            var dto = new GameStateDto
            {
                CurrentTurn = state.CurrentTurn,
                ActivePlayerIndex = state.ActivePlayerIndex,
                MapWidth = state.Map.Width,
                MapHeight = state.Map.Height
            };

            foreach (var tile in state.Map.Tiles)
            {
                dto.Tiles.Add(new TileDto
                {
                    X = tile.Position.X,
                    Y = tile.Position.Y,
                    TerrainId = tile.TerrainId,
                    Explored = tile.Explored,
                    Visible = tile.Visible
                });
            }

            foreach (var player in state.Players)
            {
                var playerDto = new PlayerDto
                {
                    Id = player.Id,
                    Name = player.Name
                };

                foreach (var unit in player.Units)
                {
                    playerDto.Units.Add(new UnitDto
                    {
                        UnitTypeId = unit.UnitTypeId,
                        X = unit.Position.X,
                        Y = unit.Position.Y,
                        MovementPoints = unit.MovementPoints,
                        MovementRemaining = unit.MovementRemaining
                    });
                }

                foreach (var city in player.Cities)
                {
                    playerDto.Cities.Add(new CityDto
                    {
                        Name = city.Name,
                        X = city.Position.X,
                        Y = city.Position.Y,
                        Population = city.Population,
                        FoodStored = city.FoodStored,
                        ProductionStored = city.ProductionStored,
                        FoodPerTurn = city.FoodPerTurn,
                        ProductionPerTurn = city.ProductionPerTurn,
                        ProductionTargetId = city.ProductionTargetId,
                        ProductionCost = city.ProductionCost
                    });
                }

                dto.Players.Add(playerDto);
            }

            return dto;
        }

        public GameState ToState()
        {
            var map = new WorldMap(MapWidth, MapHeight);
            foreach (var tile in Tiles)
            {
                var newTile = new Tile(new GridPosition(tile.X, tile.Y), tile.TerrainId)
            {
                Explored = tile.Explored,
                Visible = tile.Visible
            };
            map.Tiles.Add(newTile);
            }

            var state = new GameState
            {
                Map = map,
                CurrentTurn = CurrentTurn,
                ActivePlayerIndex = ActivePlayerIndex
            };

            foreach (var playerDto in Players)
            {
                var player = new Player(playerDto.Id, playerDto.Name);

                foreach (var unit in playerDto.Units)
                {
                    var newUnit = new Unit(unit.UnitTypeId, new GridPosition(unit.X, unit.Y), unit.MovementPoints, player.Id);
                    newUnit.MovementRemaining = unit.MovementRemaining;
                    player.Units.Add(newUnit);
                }

                foreach (var city in playerDto.Cities)
                {
                    var newCity = new City(city.Name, new GridPosition(city.X, city.Y), player.Id, city.Population)
                    {
                        FoodStored = city.FoodStored,
                        ProductionStored = city.ProductionStored,
                        FoodPerTurn = city.FoodPerTurn,
                        ProductionPerTurn = city.ProductionPerTurn,
                        ProductionTargetId = city.ProductionTargetId,
                        ProductionCost = city.ProductionCost
                    };
                    player.Cities.Add(newCity);
                }

                state.Players.Add(player);
            }

            return state;
        }

        [Serializable]
        public class TileDto
        {
            public int X;
            public int Y;
            public string TerrainId;
            public bool Explored;
            public bool Visible;
        }

        [Serializable]
        public class PlayerDto
        {
            public int Id;
            public string Name;
            public List<UnitDto> Units = new List<UnitDto>();
            public List<CityDto> Cities = new List<CityDto>();
        }

        [Serializable]
        public class UnitDto
        {
            public string UnitTypeId;
            public int X;
            public int Y;
            public int MovementPoints;
            public int MovementRemaining;
        }

        [Serializable]
        public class CityDto
        {
            public string Name;
            public int X;
            public int Y;
            public int Population;
            public int FoodStored;
            public int ProductionStored;
            public int FoodPerTurn;
            public int ProductionPerTurn;
            public string ProductionTargetId;
            public int ProductionCost;
        }
    }
}
