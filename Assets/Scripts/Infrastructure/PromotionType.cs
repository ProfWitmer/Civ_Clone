using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Promotion Type", fileName = "PromotionType")]
    public class PromotionType : ScriptableObject
    {
        public string Id = "promotion";
        public string DisplayName = "Promotion";
        public string Description = "";
        public string Requires = "";
    }
}
