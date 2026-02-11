using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Building Type", fileName = "BuildingType")]
    public class BuildingType : ScriptableObject
    {
        public string Id = "building";
        public string DisplayName = "Building";
        public int ProductionCost = 20;
        public int FoodBonus = 0;
        public int ProductionBonus = 0;
        public int ScienceBonus = 0;
        public int DefenseBonus = 0;
        public string RequiresTech = "";
    }
}
