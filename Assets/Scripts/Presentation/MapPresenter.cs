using CivClone.Infrastructure;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public class MapPresenter : MonoBehaviour
    {
        public float TileWidth => tileWidth;
        public float TileHeight => tileHeight;

        public Vector3 GridToWorld(CivClone.Simulation.GridPosition position)
        {
            float x = (position.X - position.Y) * (tileWidth * 0.5f);
            float y = (position.X + position.Y) * (tileHeight * 0.5f);
            return new Vector3(x, y, 0f);
        }

        public int GetSortingOrder(CivClone.Simulation.GridPosition position)
        {
            return -(position.X + position.Y) * 10;
        }

        [SerializeField] private float tileWidth = 1f;
        [SerializeField] private float tileHeight = 0.5f;
        [SerializeField] private float hillsElevation = 0.12f;
        [SerializeField] private float outlineThickness = 1.2f;
        [SerializeField] private float outlineDarken = 0.55f;
        [SerializeField] private Transform tileRoot;
        [SerializeField] private Color defaultColor = new Color(0.23f, 0.56f, 0.27f);
        [SerializeField] private Color hillsColor = new Color(0.4f, 0.5f, 0.25f);

        private readonly System.Collections.Generic.Dictionary<string, Sprite> tileSprites = new System.Collections.Generic.Dictionary<string, Sprite>();
        private readonly System.Collections.Generic.Dictionary<string, Material> tileMaterials = new System.Collections.Generic.Dictionary<string, Material>();
        private readonly System.Collections.Generic.Dictionary<GridPosition, TileView> tileViews = new System.Collections.Generic.Dictionary<GridPosition, TileView>();
        private Sprite improvementSprite;
        private int mapWidth;
        private int mapHeight;
        private GameDataCatalog dataCatalog;

                private void ClearTiles()
        {
            tileViews.Clear();
            tileSprites.Clear();
            tileMaterials.Clear();

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

public void Render(CivClone.Simulation.WorldMap map, GameDataCatalog dataCatalog)
        {
            if (map == null)
            {
                return;
            }

            mapWidth = map.Width;
            mapHeight = map.Height;
            this.dataCatalog = dataCatalog;

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

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                Tile tile = map.Tiles[i];
                var tileObject = new GameObject($"Tile {tile.Position.X},{tile.Position.Y}");
                tileObject.transform.SetParent(tileRoot, false);
                tileObject.transform.localPosition = GridToWorld(tile.Position);

                var renderer = tileObject.AddComponent<SpriteRenderer>();
                renderer.sprite = ResolveTerrainSprite(tile.TerrainId, dataCatalog);
                renderer.sortingOrder = GetSortingOrder(tile.Position);
                renderer.sharedMaterial = ResolveTerrainMaterial(tile.TerrainId, dataCatalog);

                var collider = tileObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(tileWidth, tileHeight);

                var view = tileObject.AddComponent<TileView>();
                view.Bind(tile.Position, renderer, improvementSprite);
                tileViews[tile.Position] = view;
                view.SetVisibility(tile.Visible, tile.Explored);
                UpdateImprovementVisual(tile, view, dataCatalog);

                if (tile.TerrainId == "hills")
                {
                    tileObject.transform.localPosition += new Vector3(0f, hillsElevation, 0f);
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
            foreach (var tile in map.Tiles)
            {
                if (tileViews.TryGetValue(tile.Position, out var view))
                {
                    UpdateImprovementVisual(tile, view, catalog);
                }
            }
        }

        private void UpdateImprovementVisual(Tile tile, TileView view, GameDataCatalog catalog)
        {
            if (tile == null || view == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(tile.ImprovementId))
            {
                view.SetImprovement(Color.white, GetSortingOrder(tile.Position) + 1, false);
                return;
            }

            var color = new Color(0.95f, 0.9f, 0.5f, 1f);
            if (catalog != null && catalog.TryGetImprovementColor(tile.ImprovementId, out var improvementColor))
            {
                color = improvementColor;
            }

            view.SetImprovement(color, GetSortingOrder(tile.Position) + 1, true);
        }

        public void UpdateFog(WorldMap map)
        {
            if (map == null)
            {
                return;
            }

            foreach (var tile in map.Tiles)
            {
                if (tileViews.TryGetValue(tile.Position, out var view))
                {
                    view.SetVisibility(tile.Visible, tile.Explored);
                    UpdateImprovementVisual(tile, view, dataCatalog);
                }
            }
        }

        private Material ResolveTerrainMaterial(string terrainId, GameDataCatalog dataCatalog)
        {
            if (tileMaterials.TryGetValue(terrainId, out var material))
            {
                return material;
            }

            var baseColor = ResolveTerrainColor(terrainId, dataCatalog);
            var outlineColor = baseColor * outlineDarken;
            outlineColor.a = 1f;

            var newMaterial = new Material(Shader.Find("Sprites/Outlined"));
            newMaterial.SetColor("_Color", Color.white);
            newMaterial.SetColor("_OutlineColor", outlineColor);
            newMaterial.SetFloat("_OutlineSize", outlineThickness);

            tileMaterials[terrainId] = newMaterial;
            return newMaterial;
        }

        private Sprite ResolveTerrainSprite(string terrainId, GameDataCatalog dataCatalog)
        {
            if (tileSprites.TryGetValue(terrainId, out var sprite))
            {
                return sprite;
            }

            var baseColor = ResolveTerrainColor(terrainId, dataCatalog);
            var edgeColor = baseColor * 0.75f;
            var pattern = terrainId == "hills" ? TilePattern.Ridges : (terrainId == "water" ? TilePattern.Waves : TilePattern.Flat);
            var newSprite = BuildTileSprite(baseColor, edgeColor, pattern);
            tileSprites[terrainId] = newSprite;
            return newSprite;
        }

        private Color ResolveTerrainColor(string terrainId, GameDataCatalog dataCatalog)
        {
            if (dataCatalog != null && dataCatalog.TryGetTerrainColor(terrainId, out var color))
            {
                return color;
            }

            return terrainId == "hills" ? hillsColor : defaultColor;
        }

        public bool TryGetWorldBounds(out Bounds bounds)
        {
            bounds = default;
            if (mapWidth <= 0 || mapHeight <= 0)
            {
                return false;
            }

            float minX = -((mapHeight - 1) * tileWidth * 0.5f);
            float maxX = ((mapWidth - 1) * tileWidth * 0.5f);
            float minY = 0f;
            float maxY = ((mapWidth - 1 + mapHeight - 1) * tileHeight * 0.5f);

            bounds = new Bounds();
            bounds.SetMinMax(new Vector3(minX, minY, 0f), new Vector3(maxX, maxY, 0f));
            return true;
        }

        private enum TilePattern
        {
            Flat,
            Ridges,
            Waves
        }

        private Sprite BuildTileSprite(Color baseColor, Color edgeColor, TilePattern pattern)
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
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, 0f));
                        continue;
                    }

                    float edgeFactor = Mathf.Clamp01((dx + dy - 0.92f) / 0.08f);
                    float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) * 0.08f;

                    float patternValue = 0f;
                    if (pattern == TilePattern.Ridges)
                    {
                        float ridge = Mathf.PerlinNoise((x + 7f) * 0.18f, (y + 3f) * 0.12f);
                        patternValue = (ridge - 0.5f) * 0.12f;
                    }
                    else if (pattern == TilePattern.Waves)
                    {
                        float wave = Mathf.Sin((x + y) * 0.25f) * 0.06f;
                        float ripple = Mathf.PerlinNoise(x * 0.22f, y * 0.22f) * 0.06f;
                        patternValue = wave + (ripple - 0.03f);
                    }
                    else
                    {
                        float grain = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                        patternValue = (grain - 0.5f) * 0.05f;
                    }

                    var color = Color.Lerp(baseColor, edgeColor, edgeFactor) + new Color(noise + patternValue, noise + patternValue, noise + patternValue, 0f);
                    color.a = 1f;
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();

            var pixelsPerUnit = width / Mathf.Max(0.01f, tileWidth);
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
