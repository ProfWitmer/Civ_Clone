using CivClone.Presentation.UI;
using UnityEngine;

namespace CivClone.Presentation.Map
{
    public sealed class TileHoverHighlighter : MonoBehaviour
    {
        [SerializeField] private MapRenderer mapRenderer;
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private TileInfoPanel tileInfoPanel;
        [SerializeField] private Color hoverFillColor = new Color(1f, 1f, 0f, 0.15f);
        [SerializeField] private Color hoverOutlineColor = new Color(1f, 0.95f, 0.5f, 0.9f);
        [SerializeField] private Color selectedFillColor = new Color(0.25f, 0.8f, 1f, 0.2f);
        [SerializeField] private Color selectedOutlineColor = new Color(0.4f, 0.9f, 1f, 0.95f);
        [SerializeField] private float highlightHeight = 0.02f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.35f;

        private Highlight hoverHighlight;
        private Highlight selectedHighlight;

        private bool hasSelection;
        private int selectedX;
        private int selectedZ;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }

            if (tileInfoPanel != null)
            {
                tileInfoPanel.Clear();
            }

            hoverHighlight = CreateHighlight("HoverHighlight", hoverFillColor, hoverOutlineColor, true);
            selectedHighlight = CreateHighlight("SelectedHighlight", selectedFillColor, selectedOutlineColor, false);
        }

        private void Update()
        {
            if (mapRenderer == null || targetCamera == null)
            {
                hoverHighlight.SetVisible(false);
                selectedHighlight.SetVisible(false);
                return;
            }

            Vector2Int mapSize = mapRenderer.CurrentSize;
            float tileSize = mapRenderer.TileSize;

            bool hasHover = TryGetHoveredTile(tileSize, mapSize, out int hoverX, out int hoverZ);
            if (hasHover)
            {
                Vector3 center = GetTileCenter(tileSize, hoverX, hoverZ);
                hoverHighlight.SetTile(center, tileSize, highlightHeight, pulseSpeed, pulseIntensity);
                hoverHighlight.SetVisible(true);

                if (tileInfoPanel != null)
                {
                    tileInfoPanel.SetCoordinates(hoverX, hoverZ);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    hasSelection = true;
                    selectedX = hoverX;
                    selectedZ = hoverZ;
                }
            }
            else
            {
                hoverHighlight.SetVisible(false);

                if (tileInfoPanel != null)
                {
                    if (hasSelection)
                    {
                        tileInfoPanel.SetCoordinates(selectedX, selectedZ);
                    }
                    else
                    {
                        tileInfoPanel.Clear();
                    }
                }
            }

            if (hasSelection)
            {
                Vector3 selectedCenter = GetTileCenter(tileSize, selectedX, selectedZ);
                selectedHighlight.SetTile(selectedCenter, tileSize, highlightHeight, 0f, 0f);
                selectedHighlight.SetVisible(true);
            }
            else
            {
                selectedHighlight.SetVisible(false);
            }
        }

        private bool TryGetHoveredTile(float tileSize, Vector2Int mapSize, out int x, out int z)
        {
            x = 0;
            z = 0;

            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            if (!TryGetWorldPointOnGround(ray, out Vector3 hitPoint))
            {
                return false;
            }

            x = Mathf.FloorToInt(hitPoint.x / tileSize);
            z = Mathf.FloorToInt(hitPoint.z / tileSize);

            if (x < 0 || z < 0 || x >= mapSize.x || z >= mapSize.y)
            {
                return false;
            }

            return true;
        }

        private Vector3 GetTileCenter(float tileSize, int x, int z)
        {
            return new Vector3(x * tileSize + tileSize * 0.5f, highlightHeight, z * tileSize + tileSize * 0.5f);
        }

        private Highlight CreateHighlight(string name, Color fillColor, Color outlineColor, bool pulsing)
        {
            GameObject fillTile = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fillTile.name = name;
            fillTile.transform.SetParent(transform, false);
            fillTile.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Collider collider = fillTile.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            MeshRenderer renderer = fillTile.GetComponent<MeshRenderer>();
            Material fillMaterial = new Material(Shader.Find("Unlit/Color"));
            fillMaterial.color = fillColor;
            renderer.sharedMaterial = fillMaterial;

            LineRenderer lineRenderer = fillTile.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
            lineRenderer.positionCount = 4;
            lineRenderer.widthMultiplier = 0.03f;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.alignment = LineAlignment.View;

            Material outlineMaterial = new Material(Shader.Find("Unlit/Color"));
            outlineMaterial.color = outlineColor;
            lineRenderer.sharedMaterial = outlineMaterial;

            Highlight highlight = new Highlight(fillTile.transform, renderer, lineRenderer, fillMaterial, outlineMaterial, fillColor, outlineColor, pulsing);
            highlight.SetVisible(false);
            return highlight;
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

        private sealed class Highlight
        {
            private readonly Transform root;
            private readonly MeshRenderer fillRenderer;
            private readonly LineRenderer outlineRenderer;
            private readonly Material fillMaterial;
            private readonly Material outlineMaterial;
            private readonly Color baseFill;
            private readonly Color baseOutline;
            private readonly bool pulsing;
            private bool visible;

            public Highlight(Transform root, MeshRenderer fillRenderer, LineRenderer outlineRenderer, Material fillMaterial, Material outlineMaterial, Color baseFill, Color baseOutline, bool pulsing)
            {
                this.root = root;
                this.fillRenderer = fillRenderer;
                this.outlineRenderer = outlineRenderer;
                this.fillMaterial = fillMaterial;
                this.outlineMaterial = outlineMaterial;
                this.baseFill = baseFill;
                this.baseOutline = baseOutline;
                this.pulsing = pulsing;
            }

            public void SetTile(Vector3 center, float tileSize, float height, float pulseSpeed, float pulseIntensity)
            {
                root.position = center;
                root.localScale = new Vector3(tileSize, tileSize, 1f);

                float half = tileSize * 0.5f;
                float y = height + 0.001f;

                outlineRenderer.SetPosition(0, new Vector3(center.x - half, y, center.z - half));
                outlineRenderer.SetPosition(1, new Vector3(center.x + half, y, center.z - half));
                outlineRenderer.SetPosition(2, new Vector3(center.x + half, y, center.z + half));
                outlineRenderer.SetPosition(3, new Vector3(center.x - half, y, center.z + half));

                float pulse = 1f;
                if (pulsing)
                {
                    pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                }

                Color fill = baseFill;
                Color outline = baseOutline;

                fill.a = Mathf.Clamp01(fill.a * pulse);
                outline.a = Mathf.Clamp01(outline.a * pulse);

                fillMaterial.color = fill;
                outlineMaterial.color = outline;
            }

            public void SetVisible(bool value)
            {
                if (visible == value)
                {
                    return;
                }

                visible = value;
                fillRenderer.enabled = value;
                outlineRenderer.enabled = value;
            }
        }
    }
}
