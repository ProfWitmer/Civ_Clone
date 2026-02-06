using CivClone.Infrastructure;
using CivClone.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace CivClone.Presentation.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class MiniMapPresenter : MonoBehaviour
    {
        private const string MiniMapImageName = "minimap-image";

        [SerializeField] private int width = 200;
        [SerializeField] private int height = 120;
        [SerializeField] private Color unexploredColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        [SerializeField] private Color exploredColor = new Color(0.25f, 0.25f, 0.28f, 1f);

        private Image minimapImage;
        private Texture2D minimapTexture;
        private GameDataCatalog dataCatalog;
        private GameState state;

        private void Awake()
        {
            var document = GetComponent<UIDocument>();
            minimapImage = document.rootVisualElement.Q<Image>(MiniMapImageName);
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
        }

        private Color ResolveTerrainColor(string terrainId)
        {
            if (dataCatalog != null && dataCatalog.TryGetTerrainColor(terrainId, out var color))
            {
                return color;
            }

            return new Color(0.3f, 0.5f, 0.3f, 1f);
        }
    }
}
