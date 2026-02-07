using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class UnitView : MonoBehaviour
    {
        public Unit Unit { get; private set; }

        public void Bind(Unit unit)
        {
            Unit = unit;
            EnsureHealthBar();
        }

        public void SetHealth(float normalized)
        {
            if (healthBarFill == null || healthBarBack == null)
            {
                return;
            }

            normalized = Mathf.Clamp01(normalized);
            healthBarFill.transform.localScale = new Vector3(normalized, 1f, 1f);
        }

        private void EnsureHealthBar()
        {
            if (healthBarBack != null)
            {
                return;
            }

            var backObj = new GameObject("HealthBack");
            backObj.transform.SetParent(transform, false);
            backObj.transform.localPosition = new Vector3(0f, 0.45f, -0.2f);
            backObj.transform.localScale = new Vector3(0.7f, 0.08f, 1f);
            healthBarBack = backObj.AddComponent<SpriteRenderer>();
            healthBarBack.sprite = BuildBarSprite();
            healthBarBack.color = new Color(0f, 0f, 0f, 0.6f);
            healthBarBack.sortingOrder = 200;

            var fillObj = new GameObject("HealthFill");
            fillObj.transform.SetParent(backObj.transform, false);
            fillObj.transform.localPosition = Vector3.zero;
            fillObj.transform.localScale = Vector3.one;
            healthBarFill = fillObj.AddComponent<SpriteRenderer>();
            healthBarFill.sprite = BuildBarSprite();
            healthBarFill.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
            healthBarFill.sortingOrder = 201;
        }

        private Sprite BuildBarSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
