using System;

namespace CivClone.Infrastructure.Data.Definitions
{
    [Serializable]
    public class UnitTypeDefinition
    {
        public string Id;
        public string DisplayName;
        public int MovementPoints;
        public int Attack;
        public int Defense;
        public int ProductionCost;
        public int WorkCost;
    }
}
