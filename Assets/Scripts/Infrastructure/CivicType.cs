using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Civic Type", fileName = "CivicType")]
    public class CivicType : ScriptableObject
    {
        public string Id = "civic";
        public string DisplayName = "Civic";
        public string Category = "Government";
        public string Description = "";
    }
}
