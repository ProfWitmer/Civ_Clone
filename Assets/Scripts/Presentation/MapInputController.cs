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

            if (Input.GetKeyDown(endTurnKey))
            {
                EndTurn();
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

        private void RunAiTurns()
        {
            if (state?.ActivePlayer == null)
            {
                return;
            }

            int safety = 0;
            while (state.ActivePlayer != null && state.ActivePlayer.Id != humanPlayerId && safety < 10)
            {
                RunAiTurn(state.ActivePlayer);
                turnSystem.EndTurn();
                safety++;
            }
        }

        private void RunAiTurn(Player aiPlayer)
        {
            if (aiPlayer == null)
            {
                return;
            }

            foreach (var unit in new List<Unit>(aiPlayer.Units))
            {
                if (unit.UnitTypeId == "settler" && !CityExistsAt(unit.Position))
                {
                    var city = new City($"City {aiPlayer.Cities.Count + 1}", unit.Position, aiPlayer.Id, 1);
                    city.ProductionTargetId = productionOptions[0];
                    city.ProductionCost = GetProductionCost(city.ProductionTargetId);
                    aiPlayer.Cities.Add(city);
                    aiPlayer.Units.Remove(unit);
                    continue;
                }

                TryMoveUnitRandomly(unit);
            }
        }

        private void TryMoveUnitRandomly(Unit unit)
        {
            if (unit == null || state?.Map == null)
            {
                return;
            }

            var directions = new[]
            {
                new GridPosition(1, 0),
                new GridPosition(-1, 0),
                new GridPosition(0, 1),
                new GridPosition(0, -1)
            };

            var offset = directions[Random.Range(0, directions.Length)];
            var target = new GridPosition(unit.Position.X + offset.X, unit.Position.Y + offset.Y);
            var tile = state.Map.GetTile(target.X, target.Y);
            if (tile == null)
            {
                return;
            }

            int moveCost = GetMoveCost(target);
            if (unit.MovementRemaining < moveCost)
            {
                return;
            }

            var occupant = FindUnitAt(target);
            if (occupant != null && occupant.OwnerId != unit.OwnerId)
            {
                ResolveCombat(unit, occupant, moveCost);
                return;
            }

            if (occupant == null)
            {
                unit.Position = target;
                unit.MovementRemaining = Mathf.Max(0, unit.MovementRemaining - moveCost);
            }
        }

        private bool CityExistsAt(GridPosition position)
        {
            if (state?.Players == null)
            {
                return false;
            }

            foreach (var player in state.Players)
            {
                foreach (var city in player.Cities)
                {
                    if (city.Position.X == position.X && city.Position.Y == position.Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Unit FindUnitAt(GridPosition position)
        {
            if (state?.Players == null)
            {
                return null;
            }

            foreach (var player in state.Players)
            {
                foreach (var unit in player.Units)
                {
                    if (unit.Position.X == position.X && unit.Position.Y == position.Y)
                    {
                        return unit;
                    }
                }
            }

            return null;
        }

        private void ResolveCombat(Unit attacker, Unit defender, int moveCost)
        {
            if (attacker == null || defender == null || state == null)
            {
                return;
            }

            int attack = GetAttack(attacker);
            int defense = GetDefense(defender);
            defense += GetDefenseBonus(defender.Position);

            int attackRoll = attack + Random.Range(0, 6);
            int defenseRoll = defense + Random.Range(0, 6);

            int damage = Mathf.Clamp(attackRoll - defenseRoll + 1, 1, 6);
            if (attackRoll >= defenseRoll)
            {
                defender.Health = Mathf.Max(0, defender.Health - damage);
                UpdateHudSelection($"Hit for {damage}");
                if (defender.Health <= 0)
                {
                    RemoveUnit(defender);
                    attacker.Position = defender.Position;
                    attacker.MovementRemaining = Mathf.Max(0, attacker.MovementRemaining - moveCost);
                    unitPresenter.RenderUnits(state, mapPresenter);
                    SelectUnit(attacker);
                    UpdateHudSelection("Won combat");
                }
            }
            else
            {
                attacker.Health = Mathf.Max(0, attacker.Health - damage);
                UpdateHudSelection($"Took {damage}");
                if (attacker.Health <= 0)
                {
                    RemoveUnit(attacker);
                    selectedUnit = null;
                    unitPresenter.RenderUnits(state, mapPresenter);
                    UpdateHudSelection("Lost combat");
                }
            }
        }

        private int GetAttack(Unit unit)
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unit.UnitTypeId, out var unitType))
            {
                return Mathf.Max(0, unitType.Attack);
            }

            return 1;
        }

        private int GetDefense(Unit unit)
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unit.UnitTypeId, out var unitType))
            {
                return Mathf.Max(0, unitType.Defense);
            }

            return 1;
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
                return;
            }

            var movementLabel = $"MP {selectedUnit.MovementRemaining}/{selectedUnit.MovementPoints} HP {selectedUnit.Health}/{selectedUnit.MaxHealth}";
            if (selectedUnit.UnitTypeId == "worker")
            {
                movementLabel = $"{movementLabel} Work {selectedUnit.WorkRemaining}";
            }
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
            UpdateHudSelection();
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
