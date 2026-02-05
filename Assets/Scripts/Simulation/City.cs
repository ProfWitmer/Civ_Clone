using System;

namespace CivClone.Simulation
{
    [Serializable]
    public class City
    {
        public string Name;
        public GridPosition Position;
        public int OwnerId;
        public int Population;

        public City(string name, GridPosition position, int ownerId, int population)
        {
            Name = name;
            Position = position;
            OwnerId = ownerId;
            Population = population;
        }
    }
}
