using System.Collections.Generic;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class UnitPresenter : MonoBehaviour
    {
        [SerializeField] private Color unitColor = new Color(0.2f, 0.6f, 0.95f);
        [SerializeField] private Color damagedColor = new Color(0.8f, 0.25f, 0.2f);
        [SerializeField] private float unitScale = 0.6f;

        private readonly Dictionary<Unit, UnitView> unitViews = new Dictionary<Unit, UnitView>();
        private Sprite unitSprite;
        private MapPresenter mapPresenter;
        private HudController hudController;

        public void RenderUnits(GameState state, MapPresenter presenter)
        {
            mapPresenter = presenter;
            ClearUnits();

            if (state == null || state.Players == null || mapPresenter == null)
            {
                return;
            }

            if (hudController == null)
            {
                hudController = FindFirstObjectByType<HudController>();
            }

            if (unitSprite == null)
            {
                unitSprite = BuildSprite();
            }

            foreach (var player in state.Players)
            {
                foreach (var unit in player.Units)
                {
                    var unitObject = new GameObject($"Unit {unit.UnitTypeId}");
                    unitObject.transform.SetParent(transform, false);
                    unitObject.transform.localPosition = mapPresenter.GridToWorld(unit.Position) + new Vector3(0f, 0f, -0.1f);
                    unitObject.transform.localScale = new Vector3(unitScale, unitScale, 1f);

                    var renderer = unitObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = unitSprite;
                    renderer.color = unitColor;
                    renderer.sortingOrder = mapPresenter.GetSortingOrder(unit.Position) + 1;

                    var collider = unitObject.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(1f, 1f);

                    var view = unitObject.AddComponent<UnitView>();
                    view.Bind(unit, renderer);

                    var hover = unitObject.AddComponent<UnitHover>();
                    hover.Bind(hudController, unit);

                    unitViews[unit] = view;
                    UpdateUnitVisual(unit);
                }
            }
        }

        public void UpdateUnitVisual(Unit unit)
        {
            if (unit == null)
            {
                return;
            }

            if (unitViews.TryGetValue(unit, out var view))
            {
                var renderer = view.GetComponent<SpriteRenderer>();
                float max = Mathf.Max(1f, unit.MaxHealth);
                float healthPct = Mathf.Clamp01(unit.Health / max);
                if (renderer != null)
                {
                    renderer.color = Color.Lerp(damagedColor, unitColor, healthPct);
                }

                view.SetHealth(healthPct);
            }
        }

        public void PlayHit(Unit unit, Vector3 direction)
        {
            if (unit == null)
            {
                return;
            }

            if (unitViews.TryGetValue(unit, out var view))
            {
                view.PlayHit(direction);
            }
        }

        public void UpdateUnitPosition(Unit unit)
        {
            if (unit == null || mapPresenter == null)
            {
                return;
            }

            if (unitViews.TryGetValue(unit, out var view))
            {
                view.transform.localPosition = mapPresenter.GridToWorld(unit.Position) + new Vector3(0f, 0f, -0.1f);
                var renderer = view.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = mapPresenter.GetSortingOrder(unit.Position) + 1;
                }
                UpdateUnitVisual(unit);
            }
        }

        private void ClearUnits()
        {
            foreach (var view in unitViews.Values)
            {
                if (view != null)
                {
                    if (Application.isPlaying)
                {
                    Destroy(view.gameObject);
                }
                else
                {
                    DestroyImmediate(view.gameObject);
                }
                }
            }

            unitViews.Clear();
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
