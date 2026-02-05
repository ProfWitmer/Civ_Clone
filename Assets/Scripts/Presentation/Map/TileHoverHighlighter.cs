using UnityEngine;

namespace CivClone.Presentation.Map
{
    public sealed class TileHoverHighlighter : MonoBehaviour
    {
        [SerializeField] private MapRenderer mapRenderer;
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.35f);
        [SerializeField] private float highlightHeight = 0.02f;

        private GameObject highlightTile;
        private MeshRenderer highlightRenderer;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }

            CreateHighlightTile();
        }

        private void Update()
        {
            if (mapRenderer == null || targetCamera == null)
            {
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
            highlightTile.transform.position = center;
            highlightTile.transform.localScale = new Vector3(tileSize, tileSize, 1f);
            SetHighlightVisible(true);
        }

        private void CreateHighlightTile()
        {
            highlightTile = GameObject.CreatePrimitive(PrimitiveType.Quad);
            highlightTile.name = "HoverHighlight";
            highlightTile.transform.SetParent(transform, false);
            highlightTile.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Collider collider = highlightTile.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            highlightRenderer = highlightTile.GetComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = highlightColor;
            highlightRenderer.sharedMaterial = material;

            SetHighlightVisible(false);
        }

        private bool TryGetWorldPointOnGround(Ray ray, out Vector3 hitPoint)
        {
            float distance = 0f;
            if (Mathf.Abs(ray.direction.y) < 0.0001f)
            {
                hitPoint = Vector3.zero;
                return false;
            }

            distance = -ray.origin.y / ray.direction.y;
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
            if (highlightRenderer != null)
            {
                highlightRenderer.enabled = visible;
            }
        }
    }
}
