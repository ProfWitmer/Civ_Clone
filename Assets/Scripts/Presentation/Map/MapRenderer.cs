using UnityEngine;

namespace CivClone.Presentation.Map
{
    public sealed class MapRenderer : MonoBehaviour
    {
        public enum MapSize
        {
            Small,
            Medium,
            Large
        }

        [Header("Map")]
        [SerializeField] private MapSize mapSize = MapSize.Medium;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private Vector2Int smallSize = new Vector2Int(40, 25);
        [SerializeField] private Vector2Int mediumSize = new Vector2Int(60, 40);
        [SerializeField] private Vector2Int largeSize = new Vector2Int(80, 60);

        [Header("Tiles")]
        [SerializeField] private Color lightColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color darkColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private bool generateOnStart = true;

        private Material lightMaterial;
        private Material darkMaterial;

        private void Start()
        {
            if (generateOnStart)
            {
                RenderGrid();
            }
        }

        [ContextMenu("Render Grid")]
        public void RenderGrid()
        {
            EnsureMaterials();
            ClearChildren();

            Vector2Int size = GetMapSize();

            for (int z = 0; z < size.y; z++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tile.name = $"Tile_{x}_{z}";
                    tile.transform.SetParent(transform, false);
                    tile.transform.localPosition = new Vector3(x * tileSize + tileSize * 0.5f, 0f, z * tileSize + tileSize * 0.5f);
                    tile.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    tile.transform.localScale = new Vector3(tileSize, tileSize, 1f);

                    Collider collider = tile.GetComponent<Collider>();
                    if (collider != null)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(collider);
                        }
                        else
                        {
                            DestroyImmediate(collider);
                        }
                    }

                    MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = (x + z) % 2 == 0 ? lightMaterial : darkMaterial;
                }
            }
        }

        private void EnsureMaterials()
        {
            if (lightMaterial == null)
            {
                lightMaterial = new Material(Shader.Find("Standard"));
                lightMaterial.color = lightColor;
            }

            if (darkMaterial == null)
            {
                darkMaterial = new Material(Shader.Find("Standard"));
                darkMaterial.color = darkColor;
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private Vector2Int GetMapSize()
        {
            return mapSize switch
            {
                MapSize.Small => smallSize,
                MapSize.Medium => mediumSize,
                MapSize.Large => largeSize,
                _ => mediumSize
            };
        }
    }
}
