using System;
using UnityEngine;

namespace CivClone.Infrastructure.Data.Definitions
{
    [Serializable]
    public class ImprovementTypeDefinition
    {
        public string Id;
        public string DisplayName;
        public Color Color;
        public int FoodBonus;
        public int ProductionBonus;
    }
}
