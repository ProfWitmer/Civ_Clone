using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Tech Type", fileName = "TechType")]
    public class TechType : ScriptableObject
    {
        public string Id = "agriculture";
        public string DisplayName = "Agriculture";
        public int Cost = 20;
    }
}
