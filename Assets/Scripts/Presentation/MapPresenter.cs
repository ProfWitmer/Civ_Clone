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
        [SerializeField] private Color roadColor = new Color(0.75f, 0.65f, 0.45f, 0.9f);

        private readonly System.Collections.Generic.Dictionary<string, Sprite> tileSprites = new System.Collections.Generic.Dictionary<string, Sprite>();
        private readonly System.Collections.Generic.Dictionary<string, Material> tileMaterials = new System.Collections.Generic.Dictionary<string, Material>();
        private readonly System.Collections.Generic.Dictionary<GridPosition, TileView> tileViews = new System.Collections.Generic.Dictionary<GridPosition, TileView>();
        private Sprite improvementSprite;
        private Sprite resourceSprite;
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
                var root = new GameObject(Tiles);
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
                var tileObject = new GameObject($Tile
