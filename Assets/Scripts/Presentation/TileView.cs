using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class TileView : MonoBehaviour
    {
        public GridPosition Position { get; private set; }

        private SpriteRenderer spriteRenderer;

        public void Bind(GridPosition position, SpriteRenderer renderer)
        {
            Position = position;
            spriteRenderer = renderer;
        }

        public void SetVisibility(bool visible, bool explored)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (visible)
            {
                spriteRenderer.color = Color.white;
                return;
            }

            if (explored)
            {
                spriteRenderer.color = new Color(0.4f, 0.4f, 0.45f, 1f);
                return;
            }

            spriteRenderer.color = new Color(0.05f, 0.05f, 0.05f, 1f);
        }
    }
}
