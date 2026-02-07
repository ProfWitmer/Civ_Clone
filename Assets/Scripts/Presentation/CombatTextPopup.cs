using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class CombatTextPopup : MonoBehaviour
    {
        [SerializeField] private float floatSpeed = 0.4f;
        [SerializeField] private float lifetime = 1.2f;

        private float elapsed;
        private TextMesh textMesh;
        private Color baseColor;

        public void Initialize(string text, Color color, int sortingOrder)
        {
            textMesh = GetComponent<TextMesh>();
            if (textMesh == null)
            {
                textMesh = gameObject.AddComponent<TextMesh>();
            }

            textMesh.text = text;
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.05f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = color;
            baseColor = color;

            var renderer = textMesh.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += new Vector3(0f, floatSpeed * Time.deltaTime, 0f);

            float t = Mathf.Clamp01(elapsed / lifetime);
            if (textMesh != null)
            {
                var color = baseColor;
                color.a = 1f - t;
                textMesh.color = color;
            }

            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
