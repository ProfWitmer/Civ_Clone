using CivClone.Infrastructure;
using CivClone.Presentation;
using CivClone.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace CivClone.Presentation.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class MiniMapPresenter : MonoBehaviour
    {
        private const string MiniMapImageName = "minimap-image";
        private const string MiniMapViewportName = "minimap-viewport";

        [SerializeField] private int width = 200;
        [SerializeField] private int height = 120;
        [SerializeField] private Color unexploredColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        [SerializeField] private Color exploredColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        [SerializeField] private bool allowClickToPan = true;
        [SerializeField] private MapPresenter mapPresenter;
        [SerializeField] private UnityEngine.Camera targetCamera;

        private Image minimapImage;
        private VisualElement minimapViewport;
        private Texture2D minimapTexture;
        private GameDataCatalog dataCatalog;
        private GameState state;

        private void Awake()
        {
            var document = GetComponent<UIDocument>();
            minimapImage = document.rootVisualElement.Q<Image>(MiniMapImageName);
            minimapViewport = document.rootVisualElement.Q<VisualElement>(MiniMapViewportName);

            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }

            if (mapPresenter == null)
            {
                mapPresenter = FindFirstObjectByType<MapPresenter>();
            }

            if (minimapImage != null)
            {
                minimapImage.pickingMode = PickingMode.Position;
                minimapImage.RegisterCallback<PointerDownEvent>(OnMinimapPointerDown);
            }

            if (minimapViewport != null)
            {
                minimapViewport.pickingMode = PickingMode.Ignore;
            }
        }

        public void Bind(GameState gameState, GameDataCatalog catalog)
        {
            state = gameState;
            dataCatalog = catalog;
            Redraw();
        }

        public void Redraw()
        {
            if (state == null || state.Map == null || minimapImage == null)
            {
                return;
            }

            if (minimapTexture == null || minimapTexture.width != width || minimapTexture.height != height)
            {
                minimapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                minimapTexture.filterMode = FilterMode.Point;
                minimapImage.image = minimapTexture;
                minimapImage.scaleMode = ScaleMode.StretchToFill;
            }

            int mapWidth = state.Map.Width;
            int mapHeight = state.Map.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int mapX = Mathf.FloorToInt((float)x / width * mapWidth);
                    int mapY = Mathf.FloorToInt((float)y / height * mapHeight);
                    var tile = state.Map.GetTile(mapX, mapY);

                    Color color = unexploredColor;
                    if (tile != null)
                    {
                        if (tile.Visible)
                        {
                            color = ResolveTerrainColor(tile.TerrainId);
                        }
                        else if (tile.Explored)
                        {
                            color = exploredColor;
                        }
                    }

                    minimapTexture.SetPixel(x, y, color);
                }
            }

            minimapTexture.Apply();
            UpdateViewportBox();
        }

        private Color ResolveTerrainColor(string terrainId)
        {
            if (dataCatalog != null && dataCatalog.TryGetTerrainColor(terrainId, out var color))
            {
                return color;
            }

            return new Color(0.3f, 0.5f, 0.3f, 1f);
        }

        private void OnMinimapPointerDown(PointerDownEvent evt)
        {
            if (!allowClickToPan || state?.Map == null || minimapImage == null || mapPresenter == null || targetCamera == null)
            {
                return;
            }

            var bounds = minimapImage.contentRect;
            if (bounds.width <= 0.01f || bounds.height <= 0.01f)
            {
                return;
            }

            Vector2 local = evt.localPosition;
            float normalizedX = Mathf.Clamp01(local.x / bounds.width);
            float normalizedY = Mathf.Clamp01(1f - (local.y / bounds.height));

            int mapX = Mathf.Clamp(Mathf.FloorToInt(normalizedX * state.Map.Width), 0, state.Map.Width - 1);
            int mapY = Mathf.Clamp(Mathf.FloorToInt(normalizedY * state.Map.Height), 0, state.Map.Height - 1);

            var world = mapPresenter.GridToWorld(new GridPosition(mapX, mapY));
            var position = targetCamera.transform.position;
            position.x = world.x;
            position.y = world.y;
            targetCamera.transform.position = position;
            UpdateViewportBox();
        }

        private void LateUpdate()
        {
            UpdateViewportBox();
        }

        private void UpdateViewportBox()
        {
            if (minimapViewport == null || minimapImage == null || state?.Map == null || targetCamera == null)
            {
                return;
            }

            if (mapPresenter == null)
            {
                mapPresenter = FindFirstObjectByType<MapPresenter>();
                if (mapPresenter == null)
                {
                    return;
                }
            }

            if (!mapPresenter.TryGetWorldBounds(out var mapBounds))
            {
                return;
            }

            var imageRect = minimapImage.contentRect;
            if (imageRect.width <= 0.01f || imageRect.height <= 0.01f)
            {
                return;
            }

            float mapWidth = state.Map.Width;
            float mapHeight = state.Map.Height;

            float tileWidth = mapPresenter.TileWidth;
            float tileHeight = mapPresenter.TileHeight;

            float worldWidth = Mathf.Max(0.01f, mapWidth * tileWidth);
            float worldHeight = Mathf.Max(0.01f, mapHeight * tileHeight);

            float cameraHeight = targetCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * targetCamera.aspect;

            float normalizedW = Mathf.Clamp01(cameraWidth / worldWidth);
            float normalizedH = Mathf.Clamp01(cameraHeight / worldHeight);

            var camPos = targetCamera.transform.position;
            float normalizedX = Mathf.InverseLerp(mapBounds.min.x, mapBounds.max.x, camPos.x);
            float normalizedY = Mathf.InverseLerp(mapBounds.min.y, mapBounds.max.y, camPos.y);

            float viewportWidth = imageRect.width * normalizedW;
            float viewportHeight = imageRect.height * normalizedH;

            float left = imageRect.xMin + (imageRect.width * normalizedX) - viewportWidth * 0.5f;
            float top = imageRect.yMin + (imageRect.height * (1f - normalizedY)) - viewportHeight * 0.5f;

            minimapViewport.style.left = left;
            minimapViewport.style.top = top;
            minimapViewport.style.width = viewportWidth;
            minimapViewport.style.height = viewportHeight;
        }
    }
}
