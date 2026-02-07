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
        public int FoodStored;
        public int ProductionStored;
        public int BaseFoodPerTurn = 1;
        public int BaseProductionPerTurn = 1;
        public int FoodPerTurn = 1;
        public int ProductionPerTurn = 1;
        public string ProductionTargetId = "scout";
        public System.Collections.Generic.List<string> ProductionQueue = new System.Collections.Generic.List<string>();
        public int ProductionCost = 10;

        public City(string name, GridPosition position, int ownerId, int population)
        {
            Name = name;
            Position = position;
            OwnerId = ownerId;
            Population = population;
            FoodStored = 0;
            ProductionStored = 0;
            FoodPerTurn = BaseFoodPerTurn;
            ProductionPerTurn = BaseProductionPerTurn;
        }
    }
}
