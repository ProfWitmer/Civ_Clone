using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class TileView : MonoBehaviour
    {
        private const string Improvement = "Improvement";
        private const string Resource = "Resource";

        public GridPosition Position { get; private set; }

        private SpriteRenderer spriteRenderer;
        private SpriteRenderer improvementRenderer;
        private SpriteRenderer resourceRenderer;

        public void Bind(GridPosition position, SpriteRenderer renderer, Sprite improvementSprite, Sprite resourceSprite)
        {
            Position = position;
            spriteRenderer = renderer;

            var improvementObj = new GameObject(Improvement);
            improvementObj.transform.SetParent(transform, false);
            improvementObj.transform.localPosition = new Vector3(0f, 0.02f, -0.05f);
            improvementRenderer = improvementObj.AddComponent<SpriteRenderer>();
            improvementRenderer.sprite = improvementSprite;
            improvementRenderer.enabled = false;

            var resourceObj = new GameObject(Resource);
            resourceObj.transform.SetParent(transform, false);
            resourceObj.transform.localPosition = new Vector3(0f, 0.08f, -0.06f);
            resourceObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            resourceRenderer = resourceObj.AddComponent<SpriteRenderer>();
            resourceRenderer.sprite = resourceSprite;
            resourceRenderer.enabled = false;
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
                if (resourceRenderer != null)
                {
                    resourceRenderer.color = new Color(resourceRenderer.color.r, resourceRenderer.color.g, resourceRenderer.color.b, 1f);
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
                if (resourceRenderer != null)
                {
                    resourceRenderer.color = new Color(resourceRenderer.color.r, resourceRenderer.color.g, resourceRenderer.color.b, 0.4f);
                }
                return;
            }

            spriteRenderer.color = new Color(0.05f, 0.05f, 0.05f, 1f);
            if (improvementRenderer != null)
            {
                improvementRenderer.color = new Color(improvementRenderer.color.r, improvementRenderer.color.g, improvementRenderer.color.b, 0f);
            }
            if (resourceRenderer != null)
            {
                resourceRenderer.color = new Color(resourceRenderer.color.r, resourceRenderer.color.g, resourceRenderer.color.b, 0f);
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

        public void SetResource(Color color, int sortingOrder, bool visible)
        {
            if (resourceRenderer == null)
            {
                return;
            }

            resourceRenderer.color = color;
            resourceRenderer.sortingOrder = sortingOrder;
            resourceRenderer.enabled = visible;
        }
    }
}
