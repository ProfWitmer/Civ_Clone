using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class TileView : MonoBehaviour
    {
        public GridPosition Position { get; private set; }

        private SpriteRenderer spriteRenderer;
        private SpriteRenderer improvementRenderer;

        public void Bind(GridPosition position, SpriteRenderer renderer, Sprite improvementSprite)
        {
            Position = position;
            spriteRenderer = renderer;

            var improvementObj = new GameObject("Improvement");
            improvementObj.transform.SetParent(transform, false);
            improvementObj.transform.localPosition = new Vector3(0f, 0.02f, -0.05f);
            improvementRenderer = improvementObj.AddComponent<SpriteRenderer>();
            improvementRenderer.sprite = improvementSprite;
            improvementRenderer.enabled = false;
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
                if (improvementRenderer != null)
                {
                    improvementRenderer.color = new Color(improvementRenderer.color.r, improvementRenderer.color.g, improvementRenderer.color.b, 1f);
                }
                return;
            }

            if (explored)
            {
                spriteRenderer.color = new Color(0.4f, 0.4f, 0.45f, 1f);
                if (improvementRenderer != null)
                {
                    improvementRenderer.color = new Color(improvementRenderer.color.r, improvementRenderer.color.g, improvementRenderer.color.b, 0.4f);
                }
                return;
            }

            spriteRenderer.color = new Color(0.05f, 0.05f, 0.05f, 1f);
            if (improvementRenderer != null)
            {
                improvementRenderer.color = new Color(improvementRenderer.color.r, improvementRenderer.color.g, improvementRenderer.color.b, 0f);
            }
        }

        public void SetImprovement(Color color, int sortingOrder, bool visible)
        {
            if (improvementRenderer == null)
            {
                return;
            }

            improvementRenderer.color = color;
            improvementRenderer.sortingOrder = sortingOrder;
            improvementRenderer.enabled = visible;
        }
    }
}
