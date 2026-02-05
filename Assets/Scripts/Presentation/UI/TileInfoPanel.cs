using TMPro;
using UnityEngine;

namespace CivClone.Presentation.UI
{
    public sealed class TileInfoPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        public void SetCoordinates(int x, int z)
        {
            if (label == null)
            {
                return;
            }

            label.text = $"Tile: ({x}, {z})";
        }

        public void Clear()
        {
            if (label == null)
            {
                return;
            }

            label.text = "Tile: (-, -)";
        }
    }
}
