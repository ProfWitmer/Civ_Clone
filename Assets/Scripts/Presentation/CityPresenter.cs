using System.Collections.Generic;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class CityPresenter : MonoBehaviour
    {
        [SerializeField] private Color cityColor = new Color(0.9f, 0.8f, 0.35f);
        [SerializeField] private float cityScale = 0.5f;

        private readonly Dictionary<City, GameObject> cityViews = new Dictionary<City, GameObject>();
        private Sprite citySprite;
        private MapPresenter mapPresenter;

        public void RenderCities(GameState state, MapPresenter presenter)
        {
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

            foreach (var player in state.Players)
            {
                foreach (var city in player.Cities)
                {
                    var cityObject = new GameObject($"City {city.Name}");
                    cityObject.transform.SetParent(transform, false);
                    cityObject.transform.localPosition = mapPresenter.GridToWorld(city.Position) + new Vector3(0f, 0.15f, -0.15f);
                    cityObject.transform.localScale = new Vector3(cityScale, cityScale, 1f);

                    var renderer = cityObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = citySprite;
                    renderer.color = cityColor;
                    renderer.sortingOrder = mapPresenter.GetSortingOrder(city.Position) + 2;

                    cityViews[city] = cityObject;
                }
            }
        }

        private void ClearCities()
        {
            foreach (var view in cityViews.Values)
            {
                if (view != null)
                {
                    if (Application.isPlaying)
                {
                    Destroy(view);
                }
                else
                {
                    DestroyImmediate(view);
                }
                }
            }

            cityViews.Clear();
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
