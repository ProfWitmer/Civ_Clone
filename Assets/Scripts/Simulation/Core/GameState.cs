namespace CivClone.Simulation.Core
{
    // Root of the simulation state. No UnityEngine references.
    public sealed class GameState
    {
        public int TurnNumber { get; private set; }

        public void AdvanceTurn()
        {
            TurnNumber++;
        }
    }
}
