using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Terrain Type", fileName = "TerrainType")]
    public class TerrainType : ScriptableObject
    {
        public string Id = "plains";
        public string DisplayName = "Plains";
        public int MovementCost = 1;
        public int DefenseBonus = 0;
        public Color Color = new Color(0.23f, 0.56f, 0.27f);
    }
}
