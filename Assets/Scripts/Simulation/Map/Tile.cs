namespace CivClone.Simulation.Map
{
    public sealed class Tile
    {
        public int X { get; }
        public int Y { get; }
        public int Elevation { get; }

        public Tile(int x, int y, int elevation)
        {
            X = x;
            Y = y;
            Elevation = elevation;
        }
    }
}
