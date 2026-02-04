namespace CivClone.Simulation.Units
{
    public sealed class Unit
    {
        public string Id { get; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public Unit(string id, int x, int y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
