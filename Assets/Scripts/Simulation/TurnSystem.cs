using System;
using System.Collections.Generic;

namespace CivClone.Simulation
{
    public class TurnSystem
    {
        private readonly GameState state;
        private readonly ResearchSystem researchSystem;
        private readonly Infrastructure.GameDataCatalog catalog;

        public TurnSystem(GameState stateRef, Infrastructure.GameDataCatalog catalogRef = null)
        {
            state = stateRef ?? throw new ArgumentNullException(nameof(stateRef));
            catalog = catalogRef;
            researchSystem = new ResearchSystem(catalogRef);
        }

        public void RecalculateCityYields()
        {
            var player = state.ActivePlayer;
            if (player == null)
            {
                return;
            }

            RecalculateCityYields(player);
        }

        public void RefreshTradeRoutes()
        {
            var player = state.ActivePlayer;
            if (player == null)
            {
                return;
            }

            UpdateTradeRoutes(player);
        }

        public void EndTurn()
        {
            if (state.Players.Count == 0)
            {
                return;
            }

            var currentPlayer = state.ActivePlayer;
            if (currentPlayer != null)
            {
                AdvanceWorkerImprovements(currentPlayer);
                AdvanceCities(currentPlayer);
                UpdateTradeRoutes(currentPlayer);
                researchSystem?.Advance(currentPlayer);
            }

            state.ActivePlayerIndex = (state.ActivePlayerIndex + 1) % state.Players.Count;
            if (state.ActivePlayerIndex == 0)
            {
                state.CurrentTurn += 1;
            }

            var nextPlayer = state.ActivePlayer;
            if (nextPlayer == null)
            {
                return;
            }

            foreach (var unit in nextPlayer.Units)
            {
                unit.ResetMovement();
            }
        }

        private void RecalculateCityYields(Player player)
        {
            if (state?.Map == null || player == null)
            {
                return;
            }

            foreach (var city in player.Cities)
            {
                int food = city.BaseFoodPerTurn;
                int prod = city.BaseProductionPerTurn;
                int buildingFood = 0;
                int buildingProd = 0;
                int buildingScience = 0;
                int buildingDefense = 0;

                if (HasCivic(player, "despotism"))
                {
                    prod += 1;
                }
                if (HasCivic(player, "monarchy"))
                {
                    food += 1;
                }

                for (int y = city.Position.Y - 1; y <= city.Position.Y + 1; y++)
                {
                    for (int x = city.Position.X - 1; x <= city.Position.X + 1; x++)
                    {
                        var tile = state.Map.GetTile(x, y);
                        if (tile == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(tile.ImprovementId) && catalog != null && catalog.TryGetImprovementType(tile.ImprovementId, out var improvement))
                        {
                            food += improvement.FoodBonus;
                            prod += improvement.ProductionBonus;
                        }

                        if (!string.IsNullOrWhiteSpace(tile.ResourceId) && (tile.HasRoad || !string.IsNullOrWhiteSpace(tile.ImprovementId)))
                        {
                            if (catalog != null && catalog.TryGetResourceType(tile.ResourceId, out var resource))
                            {
                                food += resource.FoodBonus;
                                prod += resource.ProductionBonus;
                            }
                        }
                    }
                }

                if (city.Buildings != null && catalog != null)
                {
                    foreach (var buildingId in city.Buildings)
                    {
                        if (string.IsNullOrWhiteSpace(buildingId))
                        {
                            continue;
                        }

                        if (catalog.TryGetBuildingType(buildingId, out var building))
                        {
                            buildingFood += building.FoodBonus;
                            buildingProd += building.ProductionBonus;
                            buildingScience += building.ScienceBonus;
                            buildingDefense += building.DefenseBonus;
                        }
                    }
                }

                city.BuildingFoodBonus = buildingFood;
                city.BuildingProductionBonus = buildingProd;
                city.BuildingScienceBonus = buildingScience;
                city.BuildingDefenseBonus = buildingDefense;

                city.FoodPerTurn = Math.Max(1, food + buildingFood);
                city.ProductionPerTurn = Math.Max(1, prod + buildingProd);
            }
        }

        private void AdvanceCities(Player player)
        {
            RecalculateCityYields(player);

            foreach (var city in player.Cities)
            {
                if (city.MaxHealth <= 0)
                {
                    city.MaxHealth = City.GetDefaultMaxHealth(city.Population);
                }
                if (city.Health <= 0)
                {
                    city.Health = city.MaxHealth;
                }
                else if (!city.UnderSiege)
                {
                    city.Health = Math.Min(city.MaxHealth, city.Health + 1);
                }

                city.FoodStored += city.FoodPerTurn;
                city.ProductionStored += city.ProductionPerTurn;

                TrySpawnUnit(player, city);

                int foodNeeded = 5 + (city.Population * 2);
                if (city.FoodStored >= foodNeeded)
                {
                    city.FoodStored -= foodNeeded;
                    city.Population += 1;
                }
            }
        }

        private void AdvanceWorkerImprovements(Player player)
        {
            if (player == null || state?.Map == null)
            {
                return;
            }

            bool improvementsCompleted = false;
            foreach (var unit in player.Units)
            {
                if (unit.WorkRemaining <= 0 || (!unit.WorkTargetIsRoad && string.IsNullOrWhiteSpace(unit.WorkTargetImprovementId)))
                {
                    continue;
                }

                if (unit.Position.X != unit.WorkTargetPosition.X || unit.Position.Y != unit.WorkTargetPosition.Y)
                {
                    continue;
                }

                unit.WorkRemaining = Math.Max(0, unit.WorkRemaining - 1);
                if (unit.WorkRemaining > 0)
                {
                    continue;
                }

                var tile = state.Map.GetTile(unit.WorkTargetPosition.X, unit.WorkTargetPosition.Y);
                if (tile == null)
                {
                    continue;
                }

                if (unit.WorkTargetIsRoad)
                {
                    tile.HasRoad = true;
                    unit.WorkTargetIsRoad = false;
                }
                else
                {
                    tile.ImprovementId = unit.WorkTargetImprovementId;
                    unit.WorkTargetImprovementId = string.Empty;
                }
                improvementsCompleted = true;
            }

            if (improvementsCompleted)
            {
                RecalculateCityYields(player);
            }
        }

        private void TrySpawnUnit(Player player, City city)
        {
            if (player == null || city == null)
            {
                return;
            }

            string targetId = city.ProductionTargetId;
            if (city.ProductionQueue != null && city.ProductionQueue.Count > 0)
            {
                targetId = city.ProductionQueue[0];
            }

            if (string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            if (catalog != null && catalog.TryGetUnitType(targetId, out var unitType))
            {
                city.ProductionCost = unitType.ProductionCost;
                if (!HasRequiredResource(player, unitType.RequiresResource))
                {
                    return;
                }
                if (!HasRequiredTech(player, unitType.RequiresTech))
                {
                    return;
                }
            }
            else if (catalog != null && catalog.TryGetBuildingType(targetId, out var buildingType))
            {
                city.ProductionCost = buildingType.ProductionCost;
                if (!HasRequiredTech(player, buildingType.RequiresTech))
                {
                    return;
                }

                if (city.Buildings != null && city.Buildings.Contains(targetId))
                {
                    AdvanceProductionQueue(city);
                    if (string.IsNullOrWhiteSpace(city.ProductionTargetId))
                    {
                        city.ProductionCost = 0;
                    }
                    return;
                }
            }
            else
            {
                return;
            }

            if (city.ProductionStored < city.ProductionCost)
            {
                return;
            }

            if (catalog != null && catalog.TryGetBuildingType(targetId, out var building))
            {
                city.ProductionStored -= city.ProductionCost;
                if (city.Buildings == null)
                {
                    city.Buildings = new List<string>();
                }

                if (!city.Buildings.Contains(targetId))
                {
                    city.Buildings.Add(targetId);
                }

                AdvanceProductionQueue(city);
                if (string.IsNullOrWhiteSpace(city.ProductionTargetId))
                {
                    city.ProductionCost = 0;
                }
                RecalculateCityYields(player);
                return;
            }

            foreach (var unit in player.Units)
            {
                if (unit.Position.X == city.Position.X && unit.Position.Y == city.Position.Y)
                {
                    return;
                }
            }

            int movement = 2;
            if (catalog != null && catalog.TryGetUnitType(targetId, out var unitType2))
            {
                movement = unitType2.MovementPoints;
            }

            city.ProductionStored -= city.ProductionCost;
            var newUnit = new Unit(targetId, city.Position, movement, player.Id);
            newUnit.Health = newUnit.MaxHealth;
            player.Units.Add(newUnit);

            AdvanceProductionQueue(city);
        }

        private void AdvanceProductionQueue(City city)
        {
            if (city == null || city.ProductionQueue == null || city.ProductionQueue.Count == 0)
            {
                return;
            }

            city.ProductionQueue.RemoveAt(0);
            while (city.ProductionQueue.Count > 0)
            {
                var nextId = city.ProductionQueue[0];
                if (catalog != null && catalog.TryGetBuildingType(nextId, out _)
                    && city.Buildings != null && city.Buildings.Contains(nextId))
                {
                    city.ProductionQueue.RemoveAt(0);
                    continue;
                }

                city.ProductionTargetId = nextId;
                if (catalog != null && catalog.TryGetUnitType(nextId, out var unitType))
                {
                    city.ProductionCost = unitType.ProductionCost;
                }
                else if (catalog != null && catalog.TryGetBuildingType(nextId, out var buildingType))
                {
                    city.ProductionCost = buildingType.ProductionCost;
                }
                break;
            }
        }

        private void UpdateTradeRoutes(Player player)
        {
            if (player == null || state?.Map == null)
            {
                return;
            }

            player.AvailableResources.Clear();
            player.TradeRoutes.Clear();

            if (player.Cities.Count == 0)
            {
                return;
            }

            for (int i = 0; i < player.Cities.Count; i++)
            {
                for (int j = i + 1; j < player.Cities.Count; j++)
                {
                    var cityA = player.Cities[i];
                    var cityB = player.Cities[j];
                    if (IsRoadConnected(player, cityA.Position, cityB.Position))
                    {
                        player.TradeRoutes.Add(new TradeRoute
                        {
                            CityA = cityA.Name,
                            CityB = cityB.Name
                        });
                    }
                }
            }

            foreach (var city in player.Cities)
            {
                for (int y = city.Position.Y - 1; y <= city.Position.Y + 1; y++)
                {
                    for (int x = city.Position.X - 1; x <= city.Position.X + 1; x++)
                    {
                        var tile = state.Map.GetTile(x, y);
                        if (tile == null || string.IsNullOrWhiteSpace(tile.ResourceId))
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(tile.ImprovementId) && !tile.HasRoad)
                        {
                            continue;
                        }

                        if (!player.AvailableResources.Contains(tile.ResourceId))
                        {
                            player.AvailableResources.Add(tile.ResourceId);
                        }
                    }
                }
            }
        }

        private bool HasRequiredResource(Player player, string resourceId)
        {
            if (player == null || string.IsNullOrWhiteSpace(resourceId))
            {
                return true;
            }

            return player.AvailableResources != null && player.AvailableResources.Contains(resourceId);
        }

        private bool HasRequiredTech(Player player, string techId)
        {
            if (player == null || string.IsNullOrWhiteSpace(techId))
            {
                return true;
            }

            return player.KnownTechs != null && player.KnownTechs.Contains(techId);
        }

        private bool HasCivic(Player player, string civicId)
        {
            if (player?.Civics == null || string.IsNullOrWhiteSpace(civicId))
            {
                return false;
            }

            foreach (var civic in player.Civics)
            {
                if (civic != null && civic.CivicId == civicId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsRoadConnected(Player player, GridPosition start, GridPosition end)
        {
            if (state?.Map == null)
            {
                return false;
            }

            if (start.X == end.X && start.Y == end.Y)
            {
                return true;
            }

            int width = state.Map.Width;
            int height = state.Map.Height;
            var visited = new bool[width, height];
            var queue = new Queue<GridPosition>();
            queue.Enqueue(start);
            visited[start.X, start.Y] = true;

            var directions = new[]
            {
                new GridPosition(1, 0),
                new GridPosition(-1, 0),
                new GridPosition(0, 1),
                new GridPosition(0, -1)
            };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var dir in directions)
                {
                    int nx = current.X + dir.X;
                    int ny = current.Y + dir.Y;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        continue;
                    }

                    if (visited[nx, ny])
                    {
                        continue;
                    }

                    var tile = state.Map.GetTile(nx, ny);
                    if (tile == null)
                    {
                        continue;
                    }

                    bool isCityTile = IsCityTile(player, nx, ny);
                    if (!tile.HasRoad && !isCityTile)
                    {
                        continue;
                    }

                    if (nx == end.X && ny == end.Y)
                    {
                        return true;
                    }

                    visited[nx, ny] = true;
                    queue.Enqueue(new GridPosition(nx, ny));
                }
            }

            return false;
        }

        private bool IsCityTile(Player player, int x, int y)
        {
            if (player == null)
            {
                return false;
            }

            foreach (var city in player.Cities)
            {
                if (city.Position.X == x && city.Position.Y == y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
