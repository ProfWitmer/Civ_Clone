using System.Collections.Generic;
using CivClone.Infrastructure;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public class MapPresenter : MonoBehaviour
    {
        public float TileWidth => tileWidth;
        public float TileHeight => tileHeight;

        public Vector3 GridToWorld(GridPosition position)
        {
            float x = (position.X - position.Y) * (tileWidth * 0.5f);
            float y = (position.X + position.Y) * (tileHeight * 0.5f);
            return new Vector3(x, y, 0f);
        }

        public int GetSortingOrder(GridPosition position)
        {
            return -(position.X + position.Y) * 10;
        }

        private enum TilePattern
        {
            Flat,
            Ridges
        }

        [SerializeField] private float tileWidth = 1f;
        [SerializeField] private float tileHeight = 0.5f;
        [SerializeField] private float hillsElevation = 0.12f;
        [SerializeField] private float outlineThickness = 1.2f;
        [SerializeField] private float outlineDarken = 0.55f;
        [SerializeField] private Transform tileRoot;
        [SerializeField] private Color defaultColor = new Color(0.23f, 0.56f, 0.27f);
        [SerializeField] private Color hillsColor = new Color(0.4f, 0.5f, 0.25f);
        [SerializeField] private Color roadColor = new Color(0.75f, 0.65f, 0.45f, 0.9f);

        private readonly Dictionary<string, Sprite> tileSprites = new Dictionary<string, Sprite>();
        private readonly Dictionary<GridPosition, TileView> tileViews = new Dictionary<GridPosition, TileView>();
        private Sprite improvementSprite;
        private Sprite resourceSprite;
        private int mapWidth;
        private int mapHeight;
        private GameDataCatalog dataCatalog;

        private void ClearTiles()
        {
            tileViews.Clear();
            tileSprites.Clear();

            if (tileRoot == null)
            {
                return;
            }

            for (int i = tileRoot.childCount - 1; i >= 0; i--)
            {
                var child = tileRoot.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        public void Render(WorldMap map, GameDataCatalog catalog)
        {
            if (map == null)
            {
                return;
            }

            mapWidth = map.Width;
            mapHeight = map.Height;
            dataCatalog = catalog;

            if (tileRoot == null)
            {
                var root = new GameObject("Tiles");
                root.transform.SetParent(transform, false);
                tileRoot = root.transform;
            }

            ClearTiles();

            if (tileSprites.Count == 0)
            {
                var baseSprite = BuildTileSprite(defaultColor, defaultColor * 0.9f, TilePattern.Flat);
                tileSprites[string.Empty] = baseSprite;
            }

            if (improvementSprite == null)
            {
                improvementSprite = BuildTileSprite(new Color(1f, 1f, 1f, 0.7f), new Color(1f, 1f, 1f, 0.3f), TilePattern.Flat);
            }

            if (resourceSprite == null)
            {
                resourceSprite = BuildTileSprite(new Color(1f, 1f, 1f, 0.9f), new Color(1f, 1f, 1f, 0.4f), TilePattern.Ridges);
            }

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                Tile tile = map.Tiles[i];
                if (tile == null)
                {
                    continue;
                }

                var tileObject = new GameObject($"Tile {tile.Position.X},{tile.Position.Y}");
                tileObject.transform.SetParent(tileRoot, false);

                Vector3 position = GridToWorld(tile.Position);
                if (tile.TerrainId == "hills")
                {
                    position.y += hillsElevation;
                }

                tileObject.transform.localPosition = position;
                tileObject.transform.localScale = new Vector3(tileWidth, tileHeight, 1f);

                var renderer = tileObject.AddComponent<SpriteRenderer>();
                renderer.sprite = GetTileSprite(tile.TerrainId);
                renderer.color = GetTileColor(tile);
                renderer.sortingOrder = GetSortingOrder(tile.Position);

                var collider = tileObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, 1f);

                var view = tileObject.AddComponent<TileView>();
                view.Bind(tile.Position, renderer, improvementSprite, resourceSprite);
                tileViews[tile.Position] = view;

                ApplyTileVisibility(tile, view);
            }

            UpdateImprovements(map, catalog);
        }

        public void UpdateFog(WorldMap map)
        {
            if (map == null)
            {
                return;
            }

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                var tile = map.Tiles[i];
                if (tile == null)
                {
                    continue;
                }

                if (tileViews.TryGetValue(tile.Position, out var view) && view != null)
                {
                    ApplyTileVisibility(tile, view);
                }
            }
        }

        public void UpdateImprovements(WorldMap map, GameDataCatalog catalog)
        {
            if (map == null)
            {
                return;
            }

            dataCatalog = catalog;

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                var tile = map.Tiles[i];
                if (tile == null)
                {
                    continue;
                }

                if (!tileViews.TryGetValue(tile.Position, out var view) || view == null)
                {
                    continue;
                }

                ApplyTileVisibility(tile, view);

                if (!string.IsNullOrWhiteSpace(tile.ImprovementId))
                {
                    Color improvementColor = Color.white;
                    if (dataCatalog != null && dataCatalog.TryGetImprovementColor(tile.ImprovementId, out var color))
                    {
                        improvementColor = color;
                    }

                    view.SetImprovement(improvementColor, GetSortingOrder(tile.Position) + 1, tile.Explored);
                }
                else
                {
                    view.SetImprovement(Color.white, 0, false);
                }

                if (!string.IsNullOrWhiteSpace(tile.ResourceId))
                {
                    Color resourceColor = Color.white;
                    if (dataCatalog != null && dataCatalog.TryGetResourceColor(tile.ResourceId, out var color))
                    {
                        resourceColor = color;
                    }

                    view.SetResource(resourceColor, GetSortingOrder(tile.Position) + 2, tile.Explored);
                }
                else
                {
                    view.SetResource(Color.white, 0, false);
                }
            }
        }

        private void ApplyTileVisibility(Tile tile, TileView view)
        {
            view.SetVisibility(tile.Visible, tile.Explored);
        }

        private Color GetTileColor(Tile tile)
        {
            if (tile == null)
            {
                return defaultColor;
            }

            Color color = defaultColor;
            if (dataCatalog != null && dataCatalog.TryGetTerrainColor(tile.TerrainId, out var terrainColor))
            {
                color = terrainColor;
            }
            else if (tile.TerrainId == "hills")
            {
                color = hillsColor;
            }

            if (tile.HasRoad)
            {
                color = Color.Lerp(color, roadColor, 0.35f);
            }

            return color;
        }

        private Sprite GetTileSprite(string terrainId)
        {
            if (tileSprites.TryGetValue(terrainId ?? string.Empty, out var sprite) && sprite != null)
            {
                return sprite;
            }

            Color baseColor = defaultColor;
            if (dataCatalog != null && dataCatalog.TryGetTerrainColor(terrainId, out var terrainColor))
            {
                baseColor = terrainColor;
            }
            else if (terrainId == "hills")
            {
                baseColor = hillsColor;
            }

            var newSprite = BuildTileSprite(baseColor, baseColor * 0.9f, TilePattern.Flat);
            tileSprites[terrainId ?? string.Empty] = newSprite;
            return newSprite;
        }

        private Sprite BuildTileSprite(Color baseColor, Color accentColor, TilePattern pattern)
        {
            int width = 64;
            int height = 32;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            var clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Abs(x - width * 0.5f) / (width * 0.5f);
                    float dy = Mathf.Abs(y - height * 0.5f) / (height * 0.5f);
                    float dist = dx + dy;
                    if (dist > 1f)
                    {
                        texture.SetPixel(x, y, clear);
                        continue;
                    }

                    bool isOutline = dist >= 1f - (outlineThickness / Mathf.Max(width, height));
                    Color color = baseColor;
                    if (pattern == TilePattern.Ridges && ((x / 6 + y / 6) % 2 == 0))
                    {
                        color = accentColor;
                    }

                    if (isOutline)
                    {
                        color *= outlineDarken;
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        public bool TryGetWorldBounds(out Bounds bounds)
        {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            if (mapWidth <= 0 || mapHeight <= 0)
            {
                return false;
            }

            var corners = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(mapWidth - 1, 0),
                new GridPosition(0, mapHeight - 1),
                new GridPosition(mapWidth - 1, mapHeight - 1)
            };

            Vector3 min = GridToWorld(corners[0]);
            Vector3 max = min;
            for (int i = 1; i < corners.Length; i++)
            {
                var world = GridToWorld(corners[i]);
                min = Vector3.Min(min, world);
                max = Vector3.Max(max, world);
            }

            float padX = tileWidth * 0.6f;
            float padY = tileHeight * 0.6f;
            min -= new Vector3(padX, padY, 0f);
            max += new Vector3(padX, padY, 0f);

            bounds = new Bounds((min + max) * 0.5f, max - min);
            return true;
        }
    }
}
