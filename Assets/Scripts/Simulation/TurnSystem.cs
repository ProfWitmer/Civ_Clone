using System;

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

            if (improvementsCompleted)
            {
                RecalculateCityYields(player);
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

                for (int y = city.Position.Y - 1; y <= city.Position.Y + 1; y++)
                {
                    for (int x = city.Position.X - 1; x <= city.Position.X + 1; x++)
                    {
                        var tile = state.Map.GetTile(x, y);
                        if (tile == null || string.IsNullOrWhiteSpace(tile.ImprovementId))
                        {
                            continue;
                        }

                        if (catalog != null && catalog.TryGetImprovementType(tile.ImprovementId, out var improvement))
                        {
                            food += improvement.FoodBonus;
                            prod += improvement.ProductionBonus;
                        }
                    }
                }

                city.FoodPerTurn = Math.Max(1, food);
                city.ProductionPerTurn = Math.Max(1, prod);
            }

            if (improvementsCompleted)
            {
                RecalculateCityYields(player);
            }
        }

private void AdvanceCities(Player player)
        {
            RecalculateCityYields(player);

            foreach (var city in player.Cities)
            {
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

            if (improvementsCompleted)
            {
                RecalculateCityYields(player);
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
                if (unit.WorkRemaining <= 0 || string.IsNullOrWhiteSpace(unit.WorkTargetImprovementId))
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

                tile.ImprovementId = unit.WorkTargetImprovementId;
                unit.WorkTargetImprovementId = string.Empty;
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

            if (city.ProductionStored < city.ProductionCost || string.IsNullOrWhiteSpace(city.ProductionTargetId))
            {
                return;
            }

            bool improvementsCompleted = false;
            foreach (var unit in player.Units)
            {
                if (unit.Position.X == city.Position.X && unit.Position.Y == city.Position.Y)
                {
                    return;
                }
            }

            int movement = 2;
            if (catalog != null && catalog.TryGetUnitType(city.ProductionTargetId, out var unitType))
            {
                movement = unitType.MovementPoints;
            }

            city.ProductionStored -= city.ProductionCost;
            var newUnit = new Unit(city.ProductionTargetId, city.Position, movement, player.Id);
            newUnit.Health = newUnit.MaxHealth;
            player.Units.Add(newUnit);
        }
    }
}
