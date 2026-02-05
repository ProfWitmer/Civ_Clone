using CivClone.Infrastructure;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public class MapPresenter : MonoBehaviour
    {
        public float TileSize => tileSize;

        public Vector3 GridToWorld(CivClone.Simulation.GridPosition position)
        {
            return new Vector3(position.X * tileSize, position.Y * tileSize, 0f);
        }

        [SerializeField] private float tileSize = 1f;
        [SerializeField] private Transform tileRoot;
        [SerializeField] private Color defaultColor = new Color(0.23f, 0.56f, 0.27f);
        [SerializeField] private Color hillsColor = new Color(0.4f, 0.5f, 0.25f);

        private Sprite _tileSprite;

        public void Render(CivClone.Simulation.WorldMap map, GameDataCatalog dataCatalog)
        {
            if (map == null)
            {
                return;
            }

            if (tileRoot == null)
            {
                var root = new GameObject("Tiles");
                root.transform.SetParent(transform, false);
                tileRoot = root.transform;
            }

            if (_tileSprite == null)
            {
                _tileSprite = BuildTileSprite();
            }

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                Tile tile = map.Tiles[i];
                var tileObject = new GameObject($"Tile {tile.Position.X},{tile.Position.Y}");
                tileObject.transform.SetParent(tileRoot, false);
                tileObject.transform.localPosition = GridToWorld(tile.Position);

                var renderer = tileObject.AddComponent<SpriteRenderer>();
                renderer.sprite = _tileSprite;
                renderer.color = ResolveTerrainColor(tile.TerrainId, dataCatalog);

                var collider = tileObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(tileSize, tileSize);

                var view = tileObject.AddComponent<TileView>();
                view.Bind(tile.Position);
            }
        }

        private Color ResolveTerrainColor(string terrainId, GameDataCatalog dataCatalog)
        {
            if (dataCatalog != null && dataCatalog.TryGetTerrainColor(terrainId, out var color))
            {
                return color;
            }

            return terrainId == "hills" ? hillsColor : defaultColor;
        }

        private Sprite BuildTileSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
