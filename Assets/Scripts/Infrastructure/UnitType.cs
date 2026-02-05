using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Unit Type", fileName = "UnitType")]
    public class UnitType : ScriptableObject
    {
        public string Id = "scout";
        public string DisplayName = "Scout";
        public int MovementPoints = 2;
        public int Attack = 1;
        public int Defense = 1;
    }
}
