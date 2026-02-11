using System.Collections.Generic;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class CityPresenter : MonoBehaviour
    {
        [SerializeField] private Color cityColor = new Color(0.9f, 0.8f, 0.35f);
        [SerializeField] private float cityScale = 0.5f;
        [SerializeField] private Color hpColor = new Color(0.9f, 0.25f, 0.25f);
        [SerializeField] private Color hpCriticalColor = new Color(0.95f, 0.55f, 0.2f);
        [SerializeField] private Color outlineColor = new Color(0.2f, 0.16f, 0.08f);
        [SerializeField] private float outlineScale = 1.12f;
        [SerializeField] private int hpFontSize = 32;
        [SerializeField] private float hpCharacterSize = 0.05f;
        [SerializeField] private Vector3 hpOffset = new Vector3(0f, 0.28f, -0.12f);
        [SerializeField] private Vector3 hpBarOffset = new Vector3(0f, 0.45f, -0.2f);
        [SerializeField] private Vector3 hpBarScale = new Vector3(0.55f, 0.07f, 1f);
        [SerializeField] private Color hpBarBackColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color hpBarFillColor = new Color(0.25f, 0.8f, 0.3f, 0.9f);
        [SerializeField] private float hpBarLerpSpeed = 2.5f;
        [SerializeField] private Color defenseColor = new Color(0.75f, 0.85f, 0.95f);
        [SerializeField] private Color defenseCriticalColor = new Color(0.95f, 0.65f, 0.25f);
        [SerializeField] private Vector3 defenseOffset = new Vector3(0.22f, 0.12f, -0.12f);
        [SerializeField] private float defenseScale = 0.18f;
        [SerializeField] private Color siegeColor = new Color(0.95f, 0.35f, 0.2f);
        [SerializeField] private Vector3 siegeOffset = new Vector3(-0.22f, 0.12f, -0.12f);
        [SerializeField] private float siegeScale = 0.16f;

        private sealed class CityView
        {
            public GameObject Root;
            public SpriteRenderer OutlineRenderer;
            public TextMesh HpLabel;
            public SpriteRenderer DefenseIcon;
            public SpriteRenderer SiegeIcon;
            public SpriteRenderer HpBarBack;
            public SpriteRenderer HpBarFill;
            public float HpDisplay;
        }

        private readonly Dictionary<City, CityView> cityViews = new Dictionary<City, CityView>();
        private readonly Dictionary<City, string> cityHoverInfo = new Dictionary<City, string>();
        private Sprite citySprite;
        private Sprite defenseSprite;
        private Sprite siegeSprite;
        private MapPresenter mapPresenter;
        private HudController hudController;

        public void RenderCities(GameState state, MapPresenter presenter)
        {
            if (hudController == null)
            {
                hudController = FindFirstObjectByType<HudController>();
            }

            mapPresenter = presenter;
            ClearCities();

            if (state == null || state.Players == null || mapPresenter == null)
            {
                return;
            }

            if (citySprite == null)
            {
                citySprite = BuildSprite();
            }
            if (defenseSprite == null)
            {
                defenseSprite = BuildShieldSprite();
            }
            if (siegeSprite == null)
            {
                siegeSprite = BuildSiegeSprite();
            }

            foreach (var player in state.Players)
            {
                foreach (var city in player.Cities)
                {
                    var cityObject = new GameObject($"City {city.Name}");
                    cityObject.transform.SetParent(transform, false);
                    cityObject.transform.localPosition = mapPresenter.GridToWorld(city.Position) + new Vector3(0f, 0.15f, -0.15f);
                    cityObject.transform.localScale = new Vector3(cityScale, cityScale, 1f);

                    var outlineObject = new GameObject($"City {city.Name} Outline");
                    outlineObject.transform.SetParent(cityObject.transform, false);
                    outlineObject.transform.localPosition = Vector3.zero;
                    outlineObject.transform.localScale = new Vector3(outlineScale, outlineScale, 1f);

                    var outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
                    outlineRenderer.sprite = citySprite;
                    outlineRenderer.color = outlineColor;
                    outlineRenderer.sortingOrder = mapPresenter.GetSortingOrder(city.Position) + 1;

                    var collider = cityObject.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(0.9f, 0.6f);
                    collider.isTrigger = true;

                    var hover = cityObject.AddComponent<CityHover>();
                    hover.Bind(this, city);

                    var renderer = cityObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = citySprite;
                    renderer.color = cityColor;
                    renderer.sortingOrder = mapPresenter.GetSortingOrder(city.Position) + 2;

                    var hpObject = new GameObject($"City {city.Name} HP");
                    hpObject.transform.SetParent(cityObject.transform, false);
                    hpObject.transform.localPosition = hpOffset;
                    hpObject.transform.localScale = Vector3.one;

                    var hpText = hpObject.AddComponent<TextMesh>();
                    hpText.fontSize = hpFontSize;
                    hpText.characterSize = hpCharacterSize;
                    hpText.anchor = TextAnchor.MiddleCenter;
                    hpText.text = BuildHpLabel(city);
                    hpText.color = GetHpColor(city);

                    var hpRenderer = hpText.GetComponent<MeshRenderer>();
                    if (hpRenderer != null)
                    {
                        hpRenderer.sortingOrder = mapPresenter.GetSortingOrder(city.Position) + 4;
                    }

                    var hpBack = new GameObject($"City {city.Name} HP Back");
                    hpBack.transform.SetParent(cityObject.transform, false);
                    hpBack.transform.localPosition = hpBarOffset;
                    hpBack.transform.localScale = hpBarScale;
                    var hpBackRenderer = hpBack.AddComponent<SpriteRenderer>();
                    hpBackRenderer.sprite = citySprite;
                    hpBackRenderer.color = hpBarBackColor;
                    hpBackRenderer.sortingOrder = mapPresenter.GetSortingOrder(city.Position) + 4;

                    var hpFill = new GameObject($"City {city.Name} HP Fill");
                    hpFill.transform.SetParent(hpBack.transform, false);
                    hpFill.transform.localPosition = Vector3.zero;
                    hpFill.transform.localScale = Vector3.one;
                    var hpFillRenderer = hpFill.AddComponent<SpriteRenderer>();
                    hpFillRenderer.sprite = citySprite;
                    hpFillRenderer.color = hpBarFillColor;
                    hpFillRenderer.sortingOrder = mapPresenter.GetSortingOrder(city.Position) + 5;

                    var defenseObject = new GameObject($"City {city.Name} Defense");
                    defenseObject.transform.SetParent(cityObject.transform, false);
                    defenseObject.transform.localPosition = defenseOffset;
                    defenseObject.transform.localScale = new Vector3(defenseScale, defenseScale, 1f);

                    var defenseRenderer = defenseObject.AddComponent<SpriteRenderer>();
                    defenseRenderer.sprite = defenseSprite;
                    defenseRenderer.color = GetDefenseColor(city);
                    defenseRenderer.sortingOrder = mapPresenter.GetSortingOrder(city.Position) + 3;

                    var siegeObject = new GameObject($"City {city.Name} Siege");
                    siegeObject.transform.SetParent(cityObject.transform, false);
                    siegeObject.transform.localPosition = siegeOffset;
                    siegeObject.transform.localScale = new Vector3(siegeScale, siegeScale, 1f);

                    var siegeRenderer = siegeObject.AddComponent<SpriteRenderer>();
                    siegeRenderer.sprite = siegeSprite;
                    siegeRenderer.color = siegeColor;
                    siegeRenderer.sortingOrder = mapPresenter.GetSortingOrder(city.Position) + 3;
                    siegeRenderer.enabled = city.UnderSiege;

                    cityViews[city] = new CityView
                    {
                        Root = cityObject,
                        OutlineRenderer = outlineRenderer,
                        HpLabel = hpText,
                        DefenseIcon = defenseRenderer,
                        SiegeIcon = siegeRenderer,
                        HpBarBack = hpBackRenderer,
                        HpBarFill = hpFillRenderer,
                        HpDisplay = city.MaxHealth > 0 ? city.Health / (float)city.MaxHealth : 1f
                    };

                    cityHoverInfo[city] = BuildHoverInfo(city);
                }
            }
        }

        private void ClearCities()
        {
            foreach (var view in cityViews.Values)
            {
                if (view?.Root != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(view.Root);
                    }
                    else
                    {
                        DestroyImmediate(view.Root);
                    }
                }
            }

            cityViews.Clear();
            cityHoverInfo.Clear();
        }

        private void Update()
        {
            if (cityViews.Count == 0)
            {
                return;
            }

            foreach (var pair in cityViews)
            {
                var city = pair.Key;
                var view = pair.Value;
                if (city == null || view == null)
                {
                    continue;
                }

                if (view.HpLabel != null)
                {
                    view.HpLabel.text = BuildHpLabel(city);
                    view.HpLabel.color = GetHpColor(city);
                }

                UpdateHpBar(city, view);

                if (view.DefenseIcon != null)
                {
                    view.DefenseIcon.color = GetDefenseColor(city);
                }

                if (view.SiegeIcon != null)
                {
                    view.SiegeIcon.enabled = city.UnderSiege;
                }

                if (view.OutlineRenderer != null)
                {
                    view.OutlineRenderer.color = GetOutlineColor(city);
                }

                cityHoverInfo[city] = BuildHoverInfo(city);
            }
        }

        private string BuildHpLabel(City city)
        {
            if (city == null)
            {
                return string.Empty;
            }

            return $"{city.Health}/{city.MaxHealth}";
        }

        private Color GetHpColor(City city)
        {
            if (city == null || city.MaxHealth <= 0)
            {
                return hpColor;
            }

            float ratio = city.Health / (float)city.MaxHealth;
            return ratio < 0.35f ? hpCriticalColor : hpColor;
        }

        private Color GetOutlineColor(City city)
        {
            if (city == null || city.MaxHealth <= 0)
            {
                return outlineColor;
            }

            float ratio = city.Health / (float)city.MaxHealth;
            return ratio < 0.35f ? hpCriticalColor : outlineColor;
        }

        private Color GetDefenseColor(City city)
        {
            if (city == null || city.MaxHealth <= 0)
            {
                return defenseColor;
            }

            int defense = 2 + Mathf.Max(0, city.Population / 2);
            if (city.Health < city.MaxHealth / 2)
            {
                defense = Mathf.Max(1, defense - 1);
            }

            float t = Mathf.Clamp01(defense / 6f);
            var baseColor = Color.Lerp(defenseColor * 0.6f, defenseColor, t);
            if (city.Health / (float)city.MaxHealth < 0.35f)
            {
                baseColor = Color.Lerp(defenseCriticalColor * 0.7f, defenseCriticalColor, t);
            }
            return baseColor;
        }

        private void UpdateHpBar(City city, CityView view)
        {
            if (view.HpBarFill == null || view.HpBarBack == null || city == null)
            {
                return;
            }

            float target = city.MaxHealth > 0 ? Mathf.Clamp01(city.Health / (float)city.MaxHealth) : 1f;
            view.HpDisplay = Mathf.MoveTowards(view.HpDisplay, target, hpBarLerpSpeed * Time.deltaTime);
            var fillScale = view.HpBarFill.transform.localScale;
            fillScale.x = Mathf.Max(0.05f, view.HpDisplay);
            view.HpBarFill.transform.localScale = fillScale;
        }

        private string BuildHoverInfo(City city)
        {
            if (city == null)
            {
                return string.Empty;
            }

            int defense = 2 + Mathf.Max(0, city.Population / 2);
            if (city.MaxHealth > 0 && city.Health < city.MaxHealth / 2)
            {
                defense = Mathf.Max(1, defense - 1);
            }

            return $"{city.Name} (Pop {city.Population})\\nHP {city.Health}/{city.MaxHealth}  Def {defense}";
        }

        public void ShowCityHover(City city)
        {
            if (hudController == null)
            {
                return;
            }

            if (city == null)
            {
                hudController.SetCityHoverInfo(string.Empty);
                return;
            }

            if (cityHoverInfo.TryGetValue(city, out var info))
            {
                hudController.SetCityHoverInfo(info);
            }
        }

        public void ClearCityHover(City city)
        {
            if (hudController == null)
            {
                return;
            }

            hudController.SetCityHoverInfo(string.Empty);
        }

        private Sprite BuildShieldSprite()
        {
            var texture = new Texture2D(6, 7, TextureFormat.RGBA32, false);
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            var fill = Color.white;
            int[,] mask =
            {
                {0,1,1,1,1,0},
                {1,1,1,1,1,1},
                {1,1,1,1,1,1},
                {1,1,1,1,1,1},
                {0,1,1,1,1,0},
                {0,0,1,1,0,0},
                {0,0,0,0,0,0}
            };

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    if (mask[texture.height - 1 - y, x] == 1)
                    {
                        texture.SetPixel(x, y, fill);
                    }
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 10f);
        }

        private Sprite BuildSiegeSprite()
        {
            var texture = new Texture2D(6, 6, TextureFormat.RGBA32, false);
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            var fill = Color.white;
            int[,] mask =
            {
                {0,0,1,1,0,0},
                {0,1,1,1,1,0},
                {1,1,1,1,1,1},
                {1,1,1,1,1,1},
                {0,1,1,1,1,0},
                {0,0,1,1,0,0}
            };

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    if (mask[texture.height - 1 - y, x] == 1)
                    {
                        texture.SetPixel(x, y, fill);
                    }
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 10f);
        }

        private Sprite BuildSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
