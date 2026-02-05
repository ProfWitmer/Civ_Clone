using UnityEngine;

namespace CivClone.Presentation.Map
{
    public sealed class TileHoverHighlighter : MonoBehaviour
    {
        [SerializeField] private MapRenderer mapRenderer;
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private Color fillColor = new Color(1f, 1f, 0f, 0.15f);
        [SerializeField] private Color outlineColor = new Color(1f, 0.95f, 0.5f, 0.9f);
        [SerializeField] private float highlightHeight = 0.02f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.35f;

        private GameObject fillTile;
        private MeshRenderer fillRenderer;
        private LineRenderer outlineRenderer;
        private Material fillMaterial;
        private Material outlineMaterial;
        private bool isVisible;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }

            CreateHighlightObjects();
        }

        private void Update()
        {
            if (mapRenderer == null || targetCamera == null)
            {
                SetHighlightVisible(false);
                return;
            }

            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            if (!TryGetWorldPointOnGround(ray, out Vector3 hitPoint))
            {
                SetHighlightVisible(false);
                return;
            }

            Vector2Int mapSize = mapRenderer.CurrentSize;
            float tileSize = mapRenderer.TileSize;

            int x = Mathf.FloorToInt(hitPoint.x / tileSize);
            int z = Mathf.FloorToInt(hitPoint.z / tileSize);

            if (x < 0 || z < 0 || x >= mapSize.x || z >= mapSize.y)
            {
                SetHighlightVisible(false);
                return;
            }

            Vector3 center = new Vector3(x * tileSize + tileSize * 0.5f, highlightHeight, z * tileSize + tileSize * 0.5f);
            fillTile.transform.position = center;
            fillTile.transform.localScale = new Vector3(tileSize, tileSize, 1f);

            UpdateOutline(tileSize, center);
            UpdatePulse();

            SetHighlightVisible(true);
        }

        private void CreateHighlightObjects()
        {
            fillTile = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fillTile.name = "HoverHighlightFill";
            fillTile.transform.SetParent(transform, false);
            fillTile.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Collider collider = fillTile.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            fillRenderer = fillTile.GetComponent<MeshRenderer>();
            fillMaterial = new Material(Shader.Find("Unlit/Color"));
            fillMaterial.color = fillColor;
            fillRenderer.sharedMaterial = fillMaterial;

            outlineRenderer = fillTile.AddComponent<LineRenderer>();
            outlineRenderer.useWorldSpace = true;
            outlineRenderer.loop = true;
            outlineRenderer.positionCount = 4;
            outlineRenderer.widthMultiplier = 0.03f;
            outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
            outlineRenderer.alignment = LineAlignment.View;

            outlineMaterial = new Material(Shader.Find("Unlit/Color"));
            outlineMaterial.color = outlineColor;
            outlineRenderer.sharedMaterial = outlineMaterial;

            SetHighlightVisible(false);
        }

        private void UpdateOutline(float tileSize, Vector3 center)
        {
            float half = tileSize * 0.5f;
            float y = highlightHeight + 0.001f;

            Vector3 p0 = new Vector3(center.x - half, y, center.z - half);
            Vector3 p1 = new Vector3(center.x + half, y, center.z - half);
            Vector3 p2 = new Vector3(center.x + half, y, center.z + half);
            Vector3 p3 = new Vector3(center.x - half, y, center.z + half);

            outlineRenderer.SetPosition(0, p0);
            outlineRenderer.SetPosition(1, p1);
            outlineRenderer.SetPosition(2, p2);
            outlineRenderer.SetPosition(3, p3);
        }

        private void UpdatePulse()
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;

            Color fill = fillColor;
            fill.a = Mathf.Clamp01(fillColor.a * pulse);
            fillMaterial.color = fill;

            Color outline = outlineColor;
            outline.a = Mathf.Clamp01(outlineColor.a * pulse);
            outlineMaterial.color = outline;
        }

        private bool TryGetWorldPointOnGround(Ray ray, out Vector3 hitPoint)
        {
            if (Mathf.Abs(ray.direction.y) < 0.0001f)
            {
                hitPoint = Vector3.zero;
                return false;
            }

            float distance = -ray.origin.y / ray.direction.y;
            if (distance <= 0f)
            {
                hitPoint = Vector3.zero;
                return false;
            }

            hitPoint = ray.origin + ray.direction * distance;
            return true;
        }

        private void SetHighlightVisible(bool visible)
        {
            if (isVisible == visible)
            {
                return;
            }

            isVisible = visible;
            fillRenderer.enabled = visible;
            outlineRenderer.enabled = visible;
        }
    }
}
