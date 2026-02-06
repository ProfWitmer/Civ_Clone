using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Improvement Type", fileName = "ImprovementType")]
    public class ImprovementType : ScriptableObject
    {
        public string Id = "farm";
        public string DisplayName = "Farm";
        public Color Color = new Color(0.4f, 0.8f, 0.35f);
        public int FoodBonus = 1;
        public int ProductionBonus = 0;
    }
}
