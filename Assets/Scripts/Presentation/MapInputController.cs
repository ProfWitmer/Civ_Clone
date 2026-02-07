using System.Collections.Generic;
using CivClone.Infrastructure;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class MapInputController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera sceneCamera;
        [SerializeField] private KeyCode endTurnKey = KeyCode.Return;
        [SerializeField] private KeyCode foundCityKey = KeyCode.F;
        [SerializeField] private KeyCode cycleProductionKey = KeyCode.P;
        [SerializeField] private KeyCode[] productionOptionKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
        [SerializeField] private KeyCode buildImprovementKey = KeyCode.B;
        [SerializeField] private KeyCode cycleResearchKey = KeyCode.R;
        [SerializeField] private KeyCode promotionKey = KeyCode.U;
        [SerializeField] private KeyCode[] promotionSelectKeys = { KeyCode.U, KeyCode.I, KeyCode.O };
        [SerializeField] private int humanPlayerId = 0;

        private GameState state;
        private TurnSystem turnSystem;
        private FogOfWarSystem fogOfWar;
        private GameDataCatalog dataCatalog;
        private MapPresenter mapPresenter;
        private UnitPresenter unitPresenter;
        private CityPresenter cityPresenter;
        private HudController hudController;
        private Map.IsometricTileHighlighter tileHighlighter;
        private Unit selectedUnit;
        private City selectedCity;
        private bool promotionSelectionOpen;
        private readonly System.Collections.Generic.List<string> availablePromotions = new System.Collections.Generic.List<string>();

        private readonly string[] productionOptions = { "scout", "worker", "settler" };
        private readonly string[] improvementOptions = { "farm", "mine" };
        private readonly System.Collections.Generic.Dictionary<string, string> improvementRequirements = new System.Collections.Generic.Dictionary<string, string>
        {
            { "farm", "agriculture" },
            { "mine", "mining" }
        };

        public void Bind(GameState gameState, TurnSystem turnSystemRef, FogOfWarSystem fogOfWarRef, GameDataCatalog dataCatalogRef, MapPresenter mapPresenterRef, UnitPresenter unitPresenterRef, CityPresenter cityPresenterRef, HudController hudControllerRef, UnityEngine.Camera cameraRef)
        {
            state = gameState;
            turnSystem = turnSystemRef;
            fogOfWar = fogOfWarRef;
            dataCatalog = dataCatalogRef;
            mapPresenter = mapPresenterRef;
            unitPresenter = unitPresenterRef;
            cityPresenter = cityPresenterRef;
            hudController = hudControllerRef;
            sceneCamera = cameraRef != null ? cameraRef : sceneCamera;

            if (mapPresenter != null)
            {
                mapPresenter.TryGetComponent(out tileHighlighter);
            }
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

            if (Input.GetKeyDown(foundCityKey))
            {
                TryFoundCity();
            }

            if (Input.GetKeyDown(cycleProductionKey))
            {
                CycleCityProduction();
            }

            HandleProductionHotkeys();

            if (Input.GetKeyDown(buildImprovementKey))
            {
                TryBuildImprovement();
            }

            if (Input.GetKeyDown(cycleResearchKey))
            {
                CycleResearch();
            }

            if (Input.GetKeyDown(promotionKey))
            {
                TogglePromotionSelection();
            }

            HandlePromotionSelection();

            if (Input.GetKeyDown(endTurnKey))
            {
                EndTurn();
            }
        }

        private void SpawnCombatText(GridPosition position, string text, Color color)
        {
            if (mapPresenter == null)
            {
                return;
            }

            var world = mapPresenter.GridToWorld(position) + new Vector3(0f, 0.2f, -0.2f);
            var obj = new GameObject("CombatText");
            obj.transform.SetParent(mapPresenter.transform, false);
            obj.transform.position = world;

            var popup = obj.AddComponent<CombatTextPopup>();
            int sorting = mapPresenter.GetSortingOrder(position) + 5;
            popup.Initialize(text, color, sorting);
        }

        private void TogglePromotionSelection()
        {
            if (promotionSelectionOpen)
            {
                promotionSelectionOpen = false;
                hudController?.HidePromotionPanel();
                return;
            }

            if (selectedUnit == null || dataCatalog == null || dataCatalog.PromotionTypes == null)
            {
                hudController?.SetEventMessage("No promotions available");
                return;
            }

            availablePromotions.Clear();
            foreach (var promo in dataCatalog.PromotionTypes)
            {
                if (promo == null || string.IsNullOrWhiteSpace(promo.Id))
                {
                    continue;
                }

                if (selectedUnit.Promotions.Contains(promo.Id))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(promo.Requires) && !selectedUnit.Promotions.Contains(promo.Requires))
                {
                    continue;
                }

                availablePromotions.Add(promo.Id);
                if (availablePromotions.Count >= 3)
                {
                    break;
                }
            }

            if (availablePromotions.Count == 0)
            {
                hudController?.SetEventMessage("All promotions already granted");
                return;
            }

            promotionSelectionOpen = true;
            string option1 = GetPromotionOptionLabel(0, "[U]");
            string option2 = GetPromotionOptionLabel(1, "[I]");
            string option3 = GetPromotionOptionLabel(2, "[O]");
            hudController?.ShowPromotionPanel(option1, option2, option3);
        }

        private string GetPromotionOptionLabel(int index, string hotkey)
        {
            if (index >= availablePromotions.Count)
            {
                return $"{hotkey} -";
            }

            string promoId = availablePromotions[index];
            if (dataCatalog != null && dataCatalog.TryGetPromotionType(promoId, out var promo) && !string.IsNullOrWhiteSpace(promo.DisplayName))
            {
                string requires = string.Empty;
                if (!string.IsNullOrWhiteSpace(promo.Requires))
                {
                    if (dataCatalog != null && dataCatalog.TryGetPromotionType(promo.Requires, out var req) && !string.IsNullOrWhiteSpace(req.DisplayName))
                    {
                        requires = $" (Req: {req.DisplayName})";
                    }
                    else
                    {
                        requires = $" (Req: {promo.Requires})";
                    }
                }

                return $"{hotkey} {promo.DisplayName}{requires}";
            }

            return $"{hotkey} {promoId}";
        }

        private void HandlePromotionSelection()
        {
            if (!promotionSelectionOpen || selectedUnit == null)
            {
                return;
            }

            int count = Mathf.Min(promotionSelectKeys.Length, availablePromotions.Count);
            for (int i = 0; i < count; i++)
            {
                if (Input.GetKeyDown(promotionSelectKeys[i]))
                {
                    string promoId = availablePromotions[i];
                    selectedUnit.Promotions.Add(promoId);
                    promotionSelectionOpen = false;
                    hudController?.HidePromotionPanel();
                    UpdateHudSelection();
                    UpdatePromotionInfo();
                    hudController?.SetEventMessage("Promotion granted");
                    return;
                }
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
            if (tileView == null)
            {
                return;
            }

            SelectCityAt(tileView.Position);

            if (selectedUnit == null)
            {
                return;
            }

            if (selectedUnit.MovementRemaining <= 0)
            {
                UpdateHudSelection("No movement remaining");
                return;
            }

            if (selectedUnit.Position.X == tileView.Position.X && selectedUnit.Position.Y == tileView.Position.Y)
            {
                return;
            }

            int moveCost = GetMoveCost(tileView.Position);
            if (moveCost >= 99)
            {
                UpdateHudSelection("Impassable terrain");
                return;
            }

            if (selectedUnit.MovementRemaining < moveCost)
            {
                UpdateHudSelection("Insufficient movement");
                return;
            }

            var occupant = FindUnitAt(tileView.Position);
            if (occupant != null && occupant.OwnerId != selectedUnit.OwnerId)
            {
                ResolveCombat(selectedUnit, occupant, moveCost);
                ApplyFog();
                return;
            }

            selectedUnit.Position = tileView.Position;
            selectedUnit.MovementRemaining = Mathf.Max(0, selectedUnit.MovementRemaining - moveCost);
            unitPresenter.UpdateUnitPosition(selectedUnit);
            UpdateHudSelection();

            if (tileHighlighter != null)
            {
                tileHighlighter.SetSelectedUnitTile(selectedUnit.Position);
            }

            ApplyFog();
        }

                private int GetWorkCost(Unit unit)
        {
            if (unit == null)
            {
                return 2;
            }

            if (dataCatalog != null && dataCatalog.TryGetUnitType(unit.UnitTypeId, out var unitType))
            {
                return Mathf.Max(1, unitType.WorkCost);
            }

            return 2;
        }

private int GetMoveCost(GridPosition position)
        {
            if (state?.Map == null)
            {
                return 1;
            }

            var tile = state.Map.GetTile(position.X, position.Y);
            if (tile == null)
            {
                return 1;
            }

            if (dataCatalog != null && dataCatalog.TryGetTerrainType(tile.TerrainId, out var terrain))
            {
                return terrain.MovementCost <= 0 ? 99 : terrain.MovementCost;
            }

            return 1;
        }

        private int GetDefense(Unit unit)
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unit.UnitTypeId, out var unitType))
            {
                return Mathf.Max(0, unitType.Defense) + GetPromotionDefenseBonus(unit);
            }

            return 1 + GetPromotionDefenseBonus(unit);
        }

        private void RemoveUnit(Unit unit)
        {
            if (unit == null || state?.Players == null)
            {
                return;
            }

            foreach (var player in state.Players)
            {
                if (player.Units.Remove(unit))
                {
                    return;
                }
            }
        }

        private void SelectCityAt(GridPosition position)
        {
            selectedCity = null;
            var activePlayer = state?.ActivePlayer;
            if (activePlayer == null)
            {
                UpdateCityInfo();
                return;
            }

            foreach (var city in activePlayer.Cities)
            {
                if (city.Position.X == position.X && city.Position.Y == position.Y)
                {
                    selectedCity = city;
                    break;
                }
            }

            UpdateCityInfo();
        }

        private void SelectUnit(Unit unit)
        {
            if (unit == null || state?.ActivePlayer == null || unit.OwnerId != state.ActivePlayer.Id)
            {
                return;
            }

            selectedUnit = unit;
            selectedCity = null;
            UpdateHudSelection();
            UpdatePromotionInfo();
            UpdateCityInfo();

            if (tileHighlighter != null)
            {
                tileHighlighter.SetSelectedUnitTile(unit.Position);
            }
        }

        private void UpdateCityInfo()
        {
            if (hudController == null)
            {
                return;
            }

            if (selectedCity == null)
            {
                hudController.SetCityInfo("City: None");
                hudController.SetProductionInfo("Production: None");
                return;
            }

            int foodNeeded = 5 + selectedCity.Population * 2;
            hudController.SetCityInfo($"City: {selectedCity.Name} (Pop {selectedCity.Population}) Food {selectedCity.FoodStored}/{foodNeeded} (+{selectedCity.FoodPerTurn})");

            int remaining = Mathf.Max(0, selectedCity.ProductionCost - selectedCity.ProductionStored);
            int turns = selectedCity.ProductionPerTurn > 0 ? Mathf.CeilToInt(remaining / (float)selectedCity.ProductionPerTurn) : 0;
            string targetName = selectedCity.ProductionTargetId;
            if (dataCatalog != null && dataCatalog.TryGetUnitType(selectedCity.ProductionTargetId, out var unitType) && !string.IsNullOrWhiteSpace(unitType.DisplayName))
            {
                targetName = unitType.DisplayName;
            }
            string optionsHint = productionOptions.Length >= 3 ? "[1-3] Select" : "";
            hudController.SetProductionInfo($"Production: {targetName} {selectedCity.ProductionStored}/{selectedCity.ProductionCost} (+{selectedCity.ProductionPerTurn}) {turns}t [P] Cycle {optionsHint}");
        }

        private void SetCityProductionByIndex(int index)
        {
            if (selectedCity == null)
            {
                return;
            }

            if (index < 0 || index >= productionOptions.Length)
            {
                return;
            }

            string candidate = productionOptions[index];
            if (dataCatalog != null && !dataCatalog.TryGetUnitType(candidate, out _))
            {
                UpdateHudSelection("Unit type missing");
                return;
            }

            selectedCity.ProductionTargetId = candidate;
            selectedCity.ProductionCost = GetProductionCost(candidate);
            UpdateCityInfo();
        }

        private void CycleCityProduction()
        {
            if (selectedCity == null || productionOptions.Length == 0)
            {
                return;
            }

            int currentIndex = -1;
            for (int i = 0; i < productionOptions.Length; i++)
            {
                if (productionOptions[i] == selectedCity.ProductionTargetId)
                {
                    currentIndex = i;
                    break;
                }
            }

            for (int offset = 1; offset <= productionOptions.Length; offset++)
            {
                int idx = (currentIndex + offset + productionOptions.Length) % productionOptions.Length;
                string candidate = productionOptions[idx];
                if (dataCatalog == null || dataCatalog.TryGetUnitType(candidate, out _))
                {
                    selectedCity.ProductionTargetId = candidate;
                    selectedCity.ProductionCost = GetProductionCost(candidate);
                    break;
                }
            }

            UpdateCityInfo();
        }

        private void TryBuildImprovement()
        {
            if (selectedUnit == null || selectedUnit.UnitTypeId != "worker" || state?.Map == null)
            {
                return;
            }

            var tile = state.Map.GetTile(selectedUnit.Position.X, selectedUnit.Position.Y);
            if (tile == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(tile.ImprovementId))
            {
                UpdateHudSelection("Tile already improved");
                return;
            }

            var activePlayer = state.ActivePlayer;
            if (activePlayer == null)
            {
                return;
            }

            string improvementId = null;
            for (int i = 0; i < improvementOptions.Length; i++)
            {
                var candidate = improvementOptions[i];
                if (improvementRequirements.TryGetValue(candidate, out var techReq) && !string.IsNullOrWhiteSpace(techReq))
                {
                    if (!activePlayer.KnownTechs.Contains(techReq))
                    {
                        continue;
                    }
                }

                improvementId = candidate;
                break;
            }

            if (string.IsNullOrWhiteSpace(improvementId))
            {
                UpdateHudSelection("No available improvements");
                return;
            }

            int workCost = GetWorkCost(selectedUnit);
            selectedUnit.StartWork(selectedUnit.Position, improvementId, workCost);
            selectedUnit.MovementRemaining = 0;
            UpdateHudSelection($"Working on {improvementId} ({workCost} turns)");
        }

        private void CycleResearch()
        {
            var player = state?.ActivePlayer;
            if (player == null || dataCatalog == null || dataCatalog.TechTypes == null || dataCatalog.TechTypes.Length == 0)
            {
                return;
            }

            var techIds = new List<string>();
            foreach (var tech in dataCatalog.TechTypes)
            {
                if (tech != null && !string.IsNullOrWhiteSpace(tech.Id))
                {
                    techIds.Add(tech.Id);
                }
            }

            if (techIds.Count == 0)
            {
                return;
            }

            int currentIndex = techIds.IndexOf(player.CurrentTechId);
            for (int offset = 1; offset <= techIds.Count; offset++)
            {
                int idx = (currentIndex + offset + techIds.Count) % techIds.Count;
                string candidate = techIds[idx];
                if (!player.KnownTechs.Contains(candidate))
                {
                    player.CurrentTechId = candidate;
                    player.ResearchProgress = 0;
                    UpdateResearchInfo();
                    return;
                }
            }

            player.CurrentTechId = techIds[(currentIndex + 1 + techIds.Count) % techIds.Count];
            player.ResearchProgress = 0;
            UpdateResearchInfo();
        }

        private int GetProductionCost(string unitTypeId)
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unitTypeId, out var unitType))
            {
                return Mathf.Max(1, unitType.ProductionCost);
            }

            return 10;
        }

        private int GetDefenseBonus(GridPosition position)
        {
            if (state?.Map == null)
            {
                return 0;
            }

            var tile = state.Map.GetTile(position.X, position.Y);
            if (tile == null)
            {
                return 0;
            }

            if (dataCatalog != null && dataCatalog.TryGetTerrainType(tile.TerrainId, out var terrain))
            {
                return Mathf.Max(0, terrain.DefenseBonus);
            }

            return 0;
        }
        private void UpdateResearchInfo()
        {
            if (hudController == null || state?.ActivePlayer == null)
            {
                return;
            }

            var player = state.ActivePlayer;
            if (string.IsNullOrWhiteSpace(player.CurrentTechId))
            {
                hudController.SetResearchInfo("Research: None");
                return;
            }

            int cost = 0;
            string techName = player.CurrentTechId;
            if (dataCatalog != null && dataCatalog.TryGetTechType(player.CurrentTechId, out var tech))
            {
                cost = tech.Cost;
                if (!string.IsNullOrWhiteSpace(tech.DisplayName))
                {
                    techName = tech.DisplayName;
                }
            }

            hudController.SetResearchInfo($"Research: {techName} {player.ResearchProgress}/{cost} [R] Cycle");
        }

private void UpdateHudSelection(string warning = null)
        {
            if (hudController == null)
            {
                return;
            }

            if (selectedUnit == null)
            {
                hudController.SetSelection("Selection: None");
                hudController.SetPromotionInfo("Promotions: None");
                hudController.SetPromotionDetail(string.Empty);
                return;
            }

            var movementLabel = $"MP {selectedUnit.MovementRemaining}/{selectedUnit.MovementPoints} HP {selectedUnit.Health}/{selectedUnit.MaxHealth}";
            if (selectedUnit.UnitTypeId == "worker")
            {
                if (selectedUnit.WorkRemaining > 0 && !string.IsNullOrWhiteSpace(selectedUnit.WorkTargetImprovementId))
                {
                    movementLabel = $"{movementLabel} Work {selectedUnit.WorkRemaining} ({selectedUnit.WorkTargetImprovementId})";
                }
                else
                {
                    movementLabel = $"{movementLabel} Work {selectedUnit.WorkRemaining}";
                }
            }
            if (selectedUnit.Promotions != null && selectedUnit.Promotions.Count > 0)
            {
                movementLabel = $"{movementLabel} Promos {selectedUnit.Promotions.Count}";
            }

            movementLabel = $"{movementLabel} [U] Promote";
            if (!string.IsNullOrWhiteSpace(warning))
            {
                movementLabel = $"{movementLabel} - {warning}";
            }

            hudController.SetSelection($"Selection: {selectedUnit.UnitTypeId} ({selectedUnit.Position.X}, {selectedUnit.Position.Y}) {movementLabel}");
        }

        private void TryFoundCity()
        {
            if (selectedUnit == null || selectedUnit.UnitTypeId != "settler")
            {
                return;
            }

            var activePlayer = state?.ActivePlayer;
            if (activePlayer == null)
            {
                return;
            }

            string cityName = $"City {activePlayer.Cities.Count + 1}";
            var city = new City(cityName, selectedUnit.Position, activePlayer.Id, 1);
            city.ProductionTargetId = productionOptions[0];
            city.ProductionCost = GetProductionCost(city.ProductionTargetId);
            activePlayer.Cities.Add(city);

            activePlayer.Units.Remove(selectedUnit);
            selectedUnit = null;
            selectedCity = city;
            unitPresenter.RenderUnits(state, mapPresenter);

            if (cityPresenter != null)
            {
                cityPresenter.RenderCities(state, mapPresenter);
            }

            if (tileHighlighter != null)
            {
                tileHighlighter.SetSelectedUnitTile(null);
            }

            ApplyFog();
            UpdateHudSelection();
            UpdatePromotionInfo();
            UpdateCityInfo();
        }

        public void RequestEndTurn()
        {
            EndTurn();
        }

        private void EndTurn()
        {
            if (turnSystem == null)
            {
                return;
            }

            turnSystem.EndTurn();
            RunAiTurns();
            selectedUnit = null;
            selectedCity = null;
            unitPresenter.RenderUnits(state, mapPresenter);
            if (cityPresenter != null)
            {
                cityPresenter.RenderCities(state, mapPresenter);
            }
            ApplyFog();
            mapPresenter?.UpdateImprovements(state.Map, dataCatalog);
            UpdateHudSelection();
            UpdatePromotionInfo();
            UpdateCityInfo();
            hudController?.Refresh();
            UpdateResearchInfo();
        }

        private void ApplyFog()
        {
            if (fogOfWar == null || mapPresenter == null || state == null)
            {
                return;
            }

            fogOfWar.Apply(state);
            mapPresenter.UpdateFog(state.Map);
            mapPresenter.UpdateImprovements(state.Map, dataCatalog);
            var minimap = GetComponent<UI.MiniMapPresenter>();
            if (minimap != null)
            {
                minimap.Redraw();
            }
            UpdateResearchInfo();
        }
    }
}
