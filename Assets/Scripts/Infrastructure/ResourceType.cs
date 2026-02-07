using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Resource Type", fileName = "ResourceType")]
    public class ResourceType : ScriptableObject
    {
        public string Id = "resource";
        public string DisplayName = "Resource";
        public string Category = "Bonus";
        public int FoodBonus;
        public int ProductionBonus;
        public int ScienceBonus;
        public Color Color = Color.white;
    }
}
