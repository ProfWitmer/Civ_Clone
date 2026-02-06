using System;
using UnityEngine;

namespace CivClone.Infrastructure.Data.Definitions
{
    [Serializable]
    public class TerrainTypeDefinition
    {
        public string Id;
        public string DisplayName;
        public int MovementCost;
        public int DefenseBonus;
        public Color Color;
    }
}
