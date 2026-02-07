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
                    ImprovementId = tile.ImprovementId,
                    ResourceId = tile.ResourceId,
                    Explored = tile.Explored,
                    Visible = tile.Visible,
                    HasRoad = tile.HasRoad
                });
            }

            foreach (var player in state.Players)
            {
                var playerDto = new PlayerDto
                {
                    Id = player.Id,
                    Name = player.Name,
                    CurrentTechId = player.CurrentTechId,
                    ResearchProgress = player.ResearchProgress
                };

                foreach (var unit in player.Units)
                {
                    playerDto.Units.Add(new UnitDto
                    {
                        UnitTypeId = unit.UnitTypeId,
                        X = unit.Position.X,
                        Y = unit.Position.Y,
                        MovementPoints = unit.MovementPoints,
                        MovementRemaining = unit.MovementRemaining,
                        WorkRemaining = unit.WorkRemaining,
                        Health = unit.Health,
                        MaxHealth = unit.MaxHealth,
                        WorkTargetImprovementId = unit.WorkTargetImprovementId,
                        WorkTargetIsRoad = unit.WorkTargetIsRoad,
                        WorkTargetX = unit.WorkTargetPosition.X,
                        WorkTargetY = unit.WorkTargetPosition.Y,
                        Promotions = new List<string>(unit.Promotions)
                    });
                }

                foreach (var tech in player.KnownTechs)
                {
                    playerDto.KnownTechs.Add(tech);
                }

                if (player.Civics != null)
                {
                    foreach (var civic in player.Civics)
                    {
                        if (civic == null)
                        {
                            continue;
                        }

                        playerDto.Civics.Add(new CivicSelectionDto
                        {
                            Category = civic.Category,
                            CivicId = civic.CivicId
                        });
                    }
                }

                if (player.AvailableResources != null)
                {
                    playerDto.AvailableResources.AddRange(player.AvailableResources);
                }

                if (player.TradeRoutes != null)
                {
                    foreach (var route in player.TradeRoutes)
                    {
                        if (route == null)
                        {
                            continue;
                        }

                        playerDto.TradeRoutes.Add(new TradeRouteDto
                        {
                            CityA = route.CityA,
                            CityB = route.CityB
                        });
                    }
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
                        BaseFoodPerTurn = city.BaseFoodPerTurn,
                        BaseProductionPerTurn = city.BaseProductionPerTurn,
                        FoodPerTurn = city.FoodPerTurn,
                        ProductionPerTurn = city.ProductionPerTurn,
                        ProductionTargetId = city.ProductionTargetId,
                        ProductionCost = city.ProductionCost,
                        ProductionQueue = new List<string>(city.ProductionQueue)
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
                    ImprovementId = tile.ImprovementId,
                    ResourceId = tile.ResourceId,
                    Explored = tile.Explored,
                    Visible = tile.Visible,
                    HasRoad = tile.HasRoad
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
                player.CurrentTechId = playerDto.CurrentTechId;
                player.ResearchProgress = playerDto.ResearchProgress;
                if (playerDto.KnownTechs != null)
                {
                    player.KnownTechs.AddRange(playerDto.KnownTechs);
                }

                if (playerDto.Civics != null)
                {
                    foreach (var civic in playerDto.Civics)
                    {
                        player.Civics.Add(new CivicSelection
                        {
                            Category = civic.Category,
                            CivicId = civic.CivicId
                        });
                    }
                }

                if (playerDto.AvailableResources != null)
                {
                    player.AvailableResources.AddRange(playerDto.AvailableResources);
                }

                if (playerDto.TradeRoutes != null)
                {
                    foreach (var route in playerDto.TradeRoutes)
                    {
                        player.TradeRoutes.Add(new TradeRoute
                        {
                            CityA = route.CityA,
                            CityB = route.CityB
                        });
                    }
                }

                foreach (var unit in playerDto.Units)
                {
                    var newUnit = new Unit(unit.UnitTypeId, new GridPosition(unit.X, unit.Y), unit.MovementPoints, player.Id);
                    newUnit.MovementRemaining = unit.MovementRemaining;
                    newUnit.WorkRemaining = unit.WorkRemaining;
                    newUnit.Health = unit.Health;
                    newUnit.MaxHealth = unit.MaxHealth;
                    newUnit.WorkTargetImprovementId = unit.WorkTargetImprovementId;
                    newUnit.WorkTargetIsRoad = unit.WorkTargetIsRoad;
                    newUnit.WorkTargetPosition = new GridPosition(unit.WorkTargetX, unit.WorkTargetY);
                    if (unit.Promotions != null)
                    {
                        newUnit.Promotions.AddRange(unit.Promotions);
                    }
                    player.Units.Add(newUnit);
                }

                foreach (var city in playerDto.Cities)
                {
                    var newCity = new City(city.Name, new GridPosition(city.X, city.Y), player.Id, city.Population)
                    {
                        FoodStored = city.FoodStored,
                        ProductionStored = city.ProductionStored,
                        BaseFoodPerTurn = city.BaseFoodPerTurn,
                        BaseProductionPerTurn = city.BaseProductionPerTurn,
                        FoodPerTurn = city.FoodPerTurn,
                        ProductionPerTurn = city.ProductionPerTurn,
                        ProductionTargetId = city.ProductionTargetId,
                        ProductionCost = city.ProductionCost,
                        ProductionQueue = new List<string>(city.ProductionQueue)
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
            public string ImprovementId;
            public string ResourceId;
            public bool Explored;
            public bool Visible;
            public bool HasRoad;
        }

        [Serializable]
        public class PlayerDto
        {
            public int Id;
            public string Name;
            public List<UnitDto> Units = new List<UnitDto>();
            public List<CityDto> Cities = new List<CityDto>();
            public List<string> KnownTechs = new List<string>();
            public List<CivicSelectionDto> Civics = new List<CivicSelectionDto>();
            public List<string> AvailableResources = new List<string>();
            public List<TradeRouteDto> TradeRoutes = new List<TradeRouteDto>();
            public string CurrentTechId;
            public int ResearchProgress;
        }

        [Serializable]
        public class UnitDto
        {
            public string UnitTypeId;
            public int X;
            public int Y;
            public int MovementPoints;
            public int MovementRemaining;
            public int WorkRemaining;
            public int Health;
            public int MaxHealth;
            public string WorkTargetImprovementId;
            public bool WorkTargetIsRoad;
            public int WorkTargetX;
            public int WorkTargetY;
            public List<string> Promotions = new List<string>();
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
            public int BaseFoodPerTurn;
            public int BaseProductionPerTurn;
            public int FoodPerTurn;
            public int ProductionPerTurn;
            public string ProductionTargetId;
            public int ProductionCost;
            public List<string> ProductionQueue = new List<string>();
        }

        [Serializable]
        public class CivicSelectionDto
        {
            public string Category;
            public string CivicId;
        }

        [Serializable]
        public class TradeRouteDto
        {
            public string CityA;
            public string CityB;
        }
    }
}
