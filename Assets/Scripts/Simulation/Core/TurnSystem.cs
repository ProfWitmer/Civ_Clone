namespace CivClone.Simulation.Core
{
    // Orchestrates turn progression and player sequencing.
    public sealed class TurnSystem
    {
        private readonly GameState _gameState;

        public TurnSystem(GameState gameState)
        {
            _gameState = gameState;
        }

        public void EndTurn()
        {
            _gameState.AdvanceTurn();
        }
    }
}
