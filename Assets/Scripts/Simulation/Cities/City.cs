namespace CivClone.Simulation.Cities
{
    public sealed class City
    {
        public string Name { get; }
        public int X { get; }
        public int Y { get; }

        public City(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }
    }
}
