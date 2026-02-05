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

            _state.ActivePlayerIndex = (_state.ActivePlayerIndex + 1) % _state.Players.Count;
            if (_state.ActivePlayerIndex == 0)
            {
                _state.CurrentTurn += 1;
            }
        }
    }
}
