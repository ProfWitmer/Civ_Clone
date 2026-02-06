using CivClone.Presentation;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation.Map
{
    public sealed class IsometricTileHighlighter : MonoBehaviour
    {
        [SerializeField] private MapPresenter mapPresenter;
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private Color hoverFill = new Color(1f, 0.95f, 0.4f, 0.18f);
        [SerializeField] private Color hoverOutline = new Color(1f, 0.95f, 0.7f, 0.9f);
        [SerializeField] private Color selectedFill = new Color(0.25f, 0.8f, 1f, 0.2f);
        [SerializeField] private Color selectedOutline = new Color(0.4f, 0.9f, 1f, 0.95f);
        [SerializeField] private float hoverPulseSpeed = 2.2f;
        [SerializeField] private float hoverPulseIntensity = 0.3f;
        [SerializeField] private float outlineThickness = 0.08f;

        private Sprite hoverSprite;
        private Sprite selectedSprite;
        private Sprite outlineSprite;

        private SpriteRenderer hoverRenderer;
        private SpriteRenderer hoverOutlineRenderer;
        private SpriteRenderer selectedRenderer;
        private SpriteRenderer selectedOutlineRenderer;

        private GridPosition? selectedTile;
        private GridPosition? unitSelection;
        [SerializeField] private bool allowTileSelection = true;

        public void Bind(MapPresenter presenter, UnityEngine.Camera camera)
        {
            mapPresenter = presenter;
            targetCamera = camera;
        }

        public void SetSelectedUnitTile(GridPosition? position)
        {
            unitSelection = position;
        }

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }

            CreateSprites();
            CreateRenderers();
            SetVisible(false, false);
        }

        private void Update()
        {
            if (mapPresenter == null || targetCamera == null)
            {
                SetVisible(false, false);
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (allowTileSelection)
                {
                    selectedTile = null;
                }
            }

            var activeSelected = selectedTile ?? unitSelection;
            bool hasSelected = activeSelected.HasValue;

            if (TryGetHoveredTile(out var hovered))
            {
                if (!hasSelected)
                {
                    UpdateHover(hovered);
                }
                else
                {
                    SetVisible(false, true);
                }

                if (allowTileSelection && Input.GetMouseButtonDown(0))
                {
                    selectedTile = hovered;
                }
            }
            else
            {
                SetVisible(false, hasSelected);
            }

            if (hasSelected)
            {
                UpdateSelected(activeSelected.Value);
            }
        }

        private void UpdateHover(GridPosition position)
        {
            var world = mapPresenter.GridToWorld(position) + new Vector3(0f, 0.02f, 0f);
            var scale = new Vector3(mapPresenter.TileWidth, mapPresenter.TileHeight, 1f);

            float pulse = 1f + Mathf.Sin(Time.time * hoverPulseSpeed) * hoverPulseIntensity;
            var fillColor = hoverFill;
            fillColor.a = Mathf.Clamp01(hoverFill.a * pulse);
            var outlineColor = hoverOutline;
            outlineColor.a = Mathf.Clamp01(hoverOutline.a * pulse);

            hoverRenderer.color = fillColor;
            hoverOutlineRenderer.color = outlineColor;

            hoverRenderer.transform.position = world;
            hoverOutlineRenderer.transform.position = world;
            hoverRenderer.transform.localScale = scale;
            hoverOutlineRenderer.transform.localScale = scale;

            int sorting = mapPresenter.GetSortingOrder(position) + 3;
            hoverRenderer.sortingOrder = sorting;
            hoverOutlineRenderer.sortingOrder = sorting + 1;

            SetVisible(true, selectedTile.HasValue);
        }

        private void UpdateSelected(GridPosition position)
        {
            var world = mapPresenter.GridToWorld(position) + new Vector3(0f, 0.02f, 0f);
            var scale = new Vector3(mapPresenter.TileWidth, mapPresenter.TileHeight, 1f);

            selectedRenderer.color = selectedFill;
            selectedOutlineRenderer.color = selectedOutline;

            selectedRenderer.transform.position = world;
            selectedOutlineRenderer.transform.position = world;
            selectedRenderer.transform.localScale = scale;
            selectedOutlineRenderer.transform.localScale = scale;

            int sorting = mapPresenter.GetSortingOrder(position) + 2;
            selectedRenderer.sortingOrder = sorting;
            selectedOutlineRenderer.sortingOrder = sorting + 1;

            SetVisible(hoverRenderer.enabled, true);
        }

        private void SetVisible(bool hoverVisible, bool selectedVisible)
        {
            if (hoverRenderer != null)
            {
                hoverRenderer.enabled = hoverVisible;
                hoverOutlineRenderer.enabled = hoverVisible;
            }

            if (selectedRenderer != null)
            {
                selectedRenderer.enabled = selectedVisible;
                selectedOutlineRenderer.enabled = selectedVisible;
            }
        }

        private bool TryGetHoveredTile(out GridPosition position)
        {
            position = default;
            Vector3 world = targetCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point = new Vector2(world.x, world.y);
            var hit = Physics2D.OverlapPoint(point);
            if (hit == null)
            {
                return false;
            }

            var view = hit.GetComponent<TileView>();
            if (view == null)
            {
                return false;
            }

            position = view.Position;
            return true;
        }

        private void CreateSprites()
        {
            hoverSprite = BuildDiamondSprite(hoverFill, hoverOutline, outlineThickness);
            selectedSprite = BuildDiamondSprite(selectedFill, selectedOutline, outlineThickness);
            outlineSprite = BuildOutlineSprite(Color.white, outlineThickness);
        }

        private void CreateRenderers()
        {
            var hover = new GameObject("HoverHighlight");
            hover.transform.SetParent(transform, false);
            hoverRenderer = hover.AddComponent<SpriteRenderer>();
            hoverRenderer.sprite = hoverSprite;

            var hoverOutlineObj = new GameObject("HoverOutline");
            hoverOutlineObj.transform.SetParent(transform, false);
            hoverOutlineRenderer = hoverOutlineObj.AddComponent<SpriteRenderer>();
            hoverOutlineRenderer.sprite = outlineSprite;

            var selected = new GameObject("SelectedHighlight");
            selected.transform.SetParent(transform, false);
            selectedRenderer = selected.AddComponent<SpriteRenderer>();
            selectedRenderer.sprite = selectedSprite;

            var selectedOutlineObj = new GameObject("SelectedOutline");
            selectedOutlineObj.transform.SetParent(transform, false);
            selectedOutlineRenderer = selectedOutlineObj.AddComponent<SpriteRenderer>();
            selectedOutlineRenderer.sprite = outlineSprite;
        }

        private Sprite BuildDiamondSprite(Color fill, Color edge, float edgeWidth)
        {
            const int width = 64;
            const int height = 32;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            var cx = (width - 1) * 0.5f;
            var cy = (height - 1) * 0.5f;
            var rx = (width - 1) * 0.5f;
            var ry = (height - 1) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var dx = Mathf.Abs(x - cx) / rx;
                    var dy = Mathf.Abs(y - cy) / ry;
                    var inside = (dx + dy) <= 1f;
                    if (!inside)
                    {
                        texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                        continue;
                    }

                    float edgeFactor = Mathf.Clamp01((dx + dy - (1f - edgeWidth)) / edgeWidth);
                    var color = Color.Lerp(fill, edge, edgeFactor);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            var pixelsPerUnit = width / Mathf.Max(0.01f, mapPresenter != null ? mapPresenter.TileWidth : 1f);
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private Sprite BuildOutlineSprite(Color color, float edgeWidth)
        {
            const int width = 64;
            const int height = 32;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            var cx = (width - 1) * 0.5f;
            var cy = (height - 1) * 0.5f;
            var rx = (width - 1) * 0.5f;
            var ry = (height - 1) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var dx = Mathf.Abs(x - cx) / rx;
                    var dy = Mathf.Abs(y - cy) / ry;
                    float dist = dx + dy;
                    bool inside = dist <= 1f;
                    bool onEdge = dist >= (1f - edgeWidth);

                    if (inside && onEdge)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    }
                }
            }

            texture.Apply();
            var pixelsPerUnit = width / Mathf.Max(0.01f, mapPresenter != null ? mapPresenter.TileWidth : 1f);
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
