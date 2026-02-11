using System;

namespace CivClone.Infrastructure.Data.Definitions
{
    [Serializable]
    public class BuildingTypeDefinition
    {
        public string Id;
        public string DisplayName;
        public int ProductionCost;
        public int FoodBonus;
        public int ProductionBonus;
        public int ScienceBonus;
        public int DefenseBonus;
        public string RequiresTech;
    }
}
