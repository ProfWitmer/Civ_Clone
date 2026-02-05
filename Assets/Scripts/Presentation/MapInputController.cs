using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class MapInputController : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;

        private GameState state;
        private TurnSystem turnSystem;
        private MapPresenter mapPresenter;
        private UnitPresenter unitPresenter;
        private HudController hudController;
        private Unit selectedUnit;

        public void Bind(GameState gameState, TurnSystem turnSystemRef, MapPresenter mapPresenterRef, UnitPresenter unitPresenterRef, HudController hudControllerRef, Camera cameraRef)
        {
            state = gameState;
            turnSystem = turnSystemRef;
            mapPresenter = mapPresenterRef;
            unitPresenter = unitPresenterRef;
            hudController = hudControllerRef;
            sceneCamera = cameraRef != null ? cameraRef : sceneCamera;
        }

        private void Update()
        {
            if (sceneCamera == null || state == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }

        private void HandleClick()
        {
            Vector3 world = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point = new Vector2(world.x, world.y);
            RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero);

            if (hit.collider == null)
            {
                return;
            }

            var unitView = hit.collider.GetComponent<UnitView>();
            if (unitView != null)
            {
                SelectUnit(unitView.Unit);
                return;
            }

            var tileView = hit.collider.GetComponent<TileView>();
            if (tileView != null && selectedUnit != null)
            {
                selectedUnit.Position = tileView.Position;
                unitPresenter.UpdateUnitPosition(selectedUnit);
                UpdateHudSelection();
            }
        }

        private void SelectUnit(Unit unit)
        {
            selectedUnit = unit;
            UpdateHudSelection();
        }

        private void UpdateHudSelection()
        {
            if (hudController == null)
            {
                return;
            }

            if (selectedUnit == null)
            {
                hudController.SetSelection("Selection: None");
                return;
            }

            hudController.SetSelection($"Selection: {selectedUnit.UnitTypeId} ({selectedUnit.Position.X}, {selectedUnit.Position.Y})");
        }
    }
}
