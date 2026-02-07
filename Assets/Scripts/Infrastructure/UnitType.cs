using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Unit Type", fileName = "UnitType")]
    public class UnitType : ScriptableObject
    {
        public string Id = "unit";
        public string DisplayName = "Unit";
        public int MovementPoints = 2;
        public int Attack = 1;
        public int Defense = 1;
        public int ProductionCost = 10;
        public int WorkCost = 2;
        public int Range = 1;
        public string RequiresResource = "";
        public string RequiresTech = "";
    }
}
