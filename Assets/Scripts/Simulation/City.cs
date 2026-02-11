using System;

namespace CivClone.Simulation
{
    [Serializable]
    public class City
    {
        public const int BaseHealth = 20;
        public string Name;
        public GridPosition Position;
        public int OwnerId;
        public int Population;
        public int Health;
        public int MaxHealth;
        public bool UnderSiege;
        public int FoodStored;
        public int ProductionStored;
        public int BaseFoodPerTurn = 1;
        public int BaseProductionPerTurn = 1;
        public int FoodPerTurn = 1;
        public int ProductionPerTurn = 1;
        public int BuildingFoodBonus;
        public int BuildingProductionBonus;
        public int BuildingScienceBonus;
        public int BuildingDefenseBonus;
        public string ProductionTargetId = "scout";
        public System.Collections.Generic.List<string> ProductionQueue = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> Buildings = new System.Collections.Generic.List<string>();
        public int ProductionCost = 10;

        public City(string name, GridPosition position, int ownerId, int population)
        {
            Name = name;
            Position = position;
            OwnerId = ownerId;
            Population = population;
            MaxHealth = GetDefaultMaxHealth(population);
            Health = MaxHealth;
            FoodStored = 0;
            ProductionStored = 0;
            FoodPerTurn = BaseFoodPerTurn;
            ProductionPerTurn = BaseProductionPerTurn;
            BuildingFoodBonus = 0;
            BuildingProductionBonus = 0;
            BuildingScienceBonus = 0;
            BuildingDefenseBonus = 0;
        }

        public static int GetDefaultMaxHealth(int population)
        {
            return BaseHealth + Math.Max(0, population) * 2;
        }
    }
}
