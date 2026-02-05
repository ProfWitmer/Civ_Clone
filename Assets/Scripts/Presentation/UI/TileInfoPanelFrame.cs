using UnityEngine;
using UnityEngine.UI;

namespace CivClone.Presentation.UI
{
    public sealed class TileInfoPanelFrame : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Outline outline;
        [SerializeField] private Color backgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.75f);
        [SerializeField] private Color outlineColor = new Color(0.9f, 0.8f, 0.45f, 0.9f);
        [SerializeField] private Vector2 outlineDistance = new Vector2(2f, -2f);

        private void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            if (background != null)
            {
                background.color = backgroundColor;
            }

            if (outline != null)
            {
                outline.effectColor = outlineColor;
                outline.effectDistance = outlineDistance;
            }
        }
    }
}
