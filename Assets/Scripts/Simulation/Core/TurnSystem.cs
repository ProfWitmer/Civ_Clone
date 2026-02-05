namespace CivClone.Simulation.Core
{
    public sealed class TurnSystem
    {
        private int currentTurn = 1;
        private int currentYear = -4000;
        private int yearStep = 20;

        public int CurrentTurn => currentTurn;
        public string CurrentYearLabel => currentYear < 0 ? $"{-currentYear} BC" : $"{currentYear} AD";

        public void AdvanceTurn()
        {
            currentTurn++;
            currentYear += yearStep;
        }
    }
}
