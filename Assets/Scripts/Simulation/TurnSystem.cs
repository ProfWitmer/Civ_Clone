using System;

namespace CivClone.Simulation
{
    public class TurnSystem
    {
        private readonly GameState _state;

        public TurnSystem(GameState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public void EndTurn()
        {
            if (_state.Players.Count == 0)
            {
                return;
            }

            var currentPlayer = _state.ActivePlayer;
            if (currentPlayer != null)
            {
                AdvanceCities(_state, currentPlayer);
            }

            _state.ActivePlayerIndex = (_state.ActivePlayerIndex + 1) % _state.Players.Count;
            if (_state.ActivePlayerIndex == 0)
            {
                _state.CurrentTurn += 1;
            }

            var nextPlayer = _state.ActivePlayer;
            if (nextPlayer == null)
            {
                return;
            }

            foreach (var unit in nextPlayer.Units)
            {
                unit.ResetMovement();
            }
        }

        private static void AdvanceCities(GameState state, Player player)
        {
            foreach (var city in player.Cities)
            {
                city.FoodStored += city.FoodPerTurn;
                city.ProductionStored += city.ProductionPerTurn;

                TrySpawnUnit(state, player, city);

                int foodNeeded = 5 + (city.Population * 2);
                if (city.FoodStored >= foodNeeded)
                {
                    city.FoodStored -= foodNeeded;
                    city.Population += 1;
                }
            }
        }

        private static void TrySpawnUnit(GameState state, Player player, City city)
        {
            if (state == null || player == null || city == null)
            {
                return;
            }

            if (city.ProductionStored < city.ProductionCost || string.IsNullOrWhiteSpace(city.ProductionTargetId))
            {
                return;
            }

            foreach (var unit in player.Units)
            {
                if (unit.Position.X == city.Position.X && unit.Position.Y == city.Position.Y)
                {
                    return;
                }
            }

            city.ProductionStored -= city.ProductionCost;
            var newUnit = new Unit(city.ProductionTargetId, city.Position, 2, player.Id);
            player.Units.Add(newUnit);
        }
    }
}
