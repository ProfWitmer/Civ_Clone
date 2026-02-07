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
        [SerializeField] private KeyCode[] productionOptionKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
        [SerializeField] private KeyCode buildImprovementKey = KeyCode.B;
        [SerializeField] private KeyCode buildRoadKey = KeyCode.R;
        [SerializeField] private KeyCode cycleResearchKey = KeyCode.T;
        [SerializeField] private KeyCode techPanelKey = KeyCode.Y;
        [SerializeField] private KeyCode techTreeKey = KeyCode.H;
        [SerializeField] private KeyCode[] techSelectKeys = { KeyCode.J, KeyCode.K, KeyCode.L };
        [SerializeField] private KeyCode promotionKey = KeyCode.U;
        [SerializeField] private KeyCode[] promotionSelectKeys = { KeyCode.U, KeyCode.I, KeyCode.O };
        [SerializeField] private KeyCode civicPanelKey = KeyCode.C;
        [SerializeField] private KeyCode[] civicSelectKeys = { KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6 };
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
        private bool techSelectionOpen;
        private bool techTreeOpen;
        private bool civicSelectionOpen;
        private int civicCategoryIndex;
        private readonly List<string> availableTechs = new List<string>();
        private readonly List<string> availablePromotions = new List<string>();
        private readonly List<string> availableCivics = new List<string>();
        private readonly List<string> civicCategories = new List<string>();

        private readonly string[] productionOptions = { "scout", "worker", "settler", "swordsman" };
        private readonly string[] improvementOptions = { "farm", "mine" };
        private readonly Dictionary<string, string> improvementRequirements = new Dictionary<string, string>
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

            turnSystem?.RefreshTradeRoutes();

            UpdateResearchInfo();
            UpdateCivicInfo();
            UpdateResourceInfo();
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

            if (Input.GetKeyDown(buildRoadKey))
            {
                TryBuildRoad();
            }

            if (Input.GetKeyDown(cycleResearchKey))
            {
                CycleResearch();
            }

            if (Input.GetKeyDown(techPanelKey))
            {
                ToggleTechSelection();
            }

            if (Input.GetKeyDown(techTreeKey))
            {
                ToggleTechTree();
            }


            HandleTechSelection();

            if (Input.GetKeyDown(promotionKey))
            {
                TogglePromotionSelection();
            }

            HandlePromotionSelection();

            if (Input.GetKeyDown(civicPanelKey))
            {
                ToggleCivicSelection();
            }

            HandleCivicSelection();

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
            int range = GetUnitRange(selectedUnit);
            bool inRange = range > 1 && (Mathf.Abs(selectedUnit.Position.X - tileView.Position.X) + Mathf.Abs(selectedUnit.Position.Y - tileView.Position.Y)) <= range;
            if (!inRange)
            {
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
            }

            var occupant = FindUnitAt(tileView.Position);
            if (occupant != null && occupant.OwnerId != selectedUnit.OwnerId)
            {
                ResolveCombat(selectedUnit, occupant, moveCost);
                ApplyFog();
                return;
            }

            if (!inRange)
            {
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

            if (tile.HasRoad)
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

            bool isRangedAttack = GetUnitRange(attacker) > 1 && (Mathf.Abs(attacker.Position.X - defender.Position.X) + Mathf.Abs(attacker.Position.Y - defender.Position.Y)) <= GetUnitRange(attacker);
            int attack = GetAttack(attacker);
            int defense = GetDefense(defender, isRangedAttack);
            defense += GetDefenseBonus(defender.Position);

            int attackRoll = attack + Random.Range(0, 6);
            int defenseRoll = defense + Random.Range(0, 6);

            int damage = Mathf.Clamp(attackRoll - defenseRoll + 1, 1, 6);
            if (attackRoll >= defenseRoll)
            {
                int reduction = GetPromotionDamageReduction(defender);
                int finalDamage = Mathf.Max(1, damage - reduction);
                defender.Health = Mathf.Max(0, defender.Health - finalDamage);
                unitPresenter?.UpdateUnitVisual(defender);
                hudController?.SetEventMessage($"Hit for {finalDamage}");
                hudController?.LogCombat($"{attacker.UnitTypeId} hit {defender.UnitTypeId} for {finalDamage}");
                SpawnCombatText(defender.Position, $"-{finalDamage}", new Color(0.95f, 0.4f, 0.2f));
                if (defender.Health <= 0)
                {
                    RemoveUnit(defender);
                    if (!isRangedAttack)
                    {
                        attacker.Position = defender.Position;
                        attacker.MovementRemaining = Mathf.Max(0, attacker.MovementRemaining - moveCost);
                    }
                    unitPresenter.RenderUnits(state, mapPresenter);
                    SelectUnit(attacker);
                    UpdateHudSelection("Won combat");
                    hudController?.LogCombat($"{defender.UnitTypeId} defeated");
                }
            }
            else
            {
                int reduction = GetPromotionDamageReduction(attacker);
                int finalDamage = Mathf.Max(1, damage - reduction);
                attacker.Health = Mathf.Max(0, attacker.Health - finalDamage);
                unitPresenter?.UpdateUnitVisual(attacker);
                hudController?.SetEventMessage($"Took {finalDamage}");
                hudController?.LogCombat($"{attacker.UnitTypeId} took {finalDamage} from {defender.UnitTypeId}");
                SpawnCombatText(attacker.Position, $"-{finalDamage}", new Color(0.9f, 0.2f, 0.2f));
                if (attacker.Health <= 0)
                {
                    RemoveUnit(attacker);
                    selectedUnit = null;
                    unitPresenter.RenderUnits(state, mapPresenter);
                    UpdateHudSelection("Lost combat");
                    hudController?.LogCombat($"{attacker.UnitTypeId} defeated");
                }
            }
        }

        private int GetPromotionDamageReduction(Unit unit)
        {
            if (unit?.Promotions == null)
            {
                return 0;
            }

            int reduction = 0;
            foreach (var promo in unit.Promotions)
            {
                switch (promo)
                {
                    case "drill1":
                        reduction += 1;
                        break;
                }
            }

            return reduction;
        }

        private int GetPromotionAttackBonus(Unit unit)
        {
            if (unit?.Promotions == null)
            {
                return 0;
            }

            int bonus = 0;
            foreach (var promo in unit.Promotions)
            {
                switch (promo)
                {
                    case "combat1":
                        bonus += 1;
                        break;
                    case "combat2":
                        bonus += 2;
                        break;
                }
            }

            return bonus;
        }

        private int GetUnitRange(Unit unit)
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unit.UnitTypeId, out var unitType))
            {
                return Mathf.Max(1, unitType.Range);
            }

            return 1;
        }

        private int GetPromotionDefenseBonus(Unit unit, bool isRanged)
        {
            if (unit?.Promotions == null)
            {
                return 0;
            }

            int bonus = 0;
            foreach (var promo in unit.Promotions)
            {
                switch (promo)
                {
                    case "combat1":
                        bonus += 1;
                        break;
                    case "combat2":
                        bonus += 2;
                        break;
                    case "cover":
                        if (isRanged)
                        {
                            bonus += 1;
                        }
                        break;
                }
            }

            return bonus;
        }

        private int GetAttack(Unit unit)
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unit.UnitTypeId, out var unitType))
            {
                return Mathf.Max(0, unitType.Attack) + GetPromotionAttackBonus(unit);
            }

            return 1 + GetPromotionAttackBonus(unit);
        }

        private int GetDefense(Unit unit, bool isRanged)
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unit.UnitTypeId, out var unitType))
            {
                return Mathf.Max(0, unitType.Defense) + GetPromotionDefenseBonus(unit, isRanged);
            }

            return 1 + GetPromotionDefenseBonus(unit, isRanged);
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
            string optionsHint = productionOptions.Length >= 4 ? "[1-4] Select" : (productionOptions.Length >= 3 ? "[1-3] Select" : "");
            string queueInfo = string.Empty;
            if (selectedCity.ProductionQueue != null && selectedCity.ProductionQueue.Count > 0)
            {
                queueInfo = " Queue: " + string.Join(", ", selectedCity.ProductionQueue);
            }
            hudController.SetProductionInfo($"Production: {targetName} {selectedCity.ProductionStored}/{selectedCity.ProductionCost} (+{selectedCity.ProductionPerTurn}) {turns}t [P] Cycle {optionsHint}{queueInfo}");
        }

        private void EnqueueProduction(string unitTypeId)
        {
            if (selectedCity == null || string.IsNullOrWhiteSpace(unitTypeId))
            {
                return;
            }

            if (selectedCity.ProductionQueue == null)
            {
                selectedCity.ProductionQueue = new List<string>();
            }

            selectedCity.ProductionQueue.Add(unitTypeId);
            UpdateCityInfo();
        }

        private bool HasRequiredResource(Player player, string resourceId)
        {
            if (player == null || string.IsNullOrWhiteSpace(resourceId))
            {
                return true;
            }

            return player.AvailableResources != null && player.AvailableResources.Contains(resourceId);
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
            UnitType unitType = null;
            if (dataCatalog == null || !dataCatalog.TryGetUnitType(candidate, out unitType))
            {
                UpdateHudSelection("Unit type missing");
                return;
            }
            if (unitType != null && !HasRequiredResource(state?.ActivePlayer, unitType.RequiresResource))
            {
                UpdateHudSelection("Requires resource");
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                EnqueueProduction(candidate);
                return;
            }

            selectedCity.ProductionTargetId = candidate;
            selectedCity.ProductionCost = GetProductionCost(candidate);
            if (selectedCity.ProductionQueue != null)
            {
                selectedCity.ProductionQueue.Clear();
            }
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
                UnitType unitType = null;
                if (dataCatalog == null || !dataCatalog.TryGetUnitType(candidate, out unitType))
                {
                    continue;
                }

                if (dataCatalog != null)
                {
                    if (unitType != null && !HasRequiredResource(state?.ActivePlayer, unitType.RequiresResource))
                    {
                        continue;
                    }

                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        EnqueueProduction(candidate);
                        return;
                    }

                    selectedCity.ProductionTargetId = candidate;
                    selectedCity.ProductionCost = GetProductionCost(candidate);
                    if (selectedCity.ProductionQueue != null)
                    {
                        selectedCity.ProductionQueue.Clear();
                    }
                    break;
                }
            }

            UpdateCityInfo();
        }

        private void HandleProductionHotkeys()
        {
            if (selectedCity == null)
            {
                return;
            }

            for (int i = 0; i < productionOptionKeys.Length && i < productionOptions.Length; i++)
            {
                if (Input.GetKeyDown(productionOptionKeys[i]))
                {
                    SetCityProductionByIndex(i);
                    break;
                }
            }
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

        private void TryBuildRoad()
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

            if (tile.HasRoad)
            {
                UpdateHudSelection("Road already built");
                return;
            }

            int workCost = GetWorkCost(selectedUnit);
            selectedUnit.StartRoadWork(selectedUnit.Position, workCost);
            selectedUnit.MovementRemaining = 0;
            UpdateHudSelection($"Building road ({workCost} turns)");
        }

        private bool AreTechPrereqsMet(Player player, string prereqList)
        {
            if (player == null || string.IsNullOrWhiteSpace(prereqList))
            {
                return true;
            }

            var parts = prereqList.Split(,);
            foreach (var part in parts)
            {
                var id = part.Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (!player.KnownTechs.Contains(id))
                {
                    return false;
                }
            }

            return true;
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
                    if (dataCatalog != null && dataCatalog.TryGetTechType(candidate, out var tech) && !AreTechPrereqsMet(player, tech.Prerequisites))
                    {
                        continue;
                    }

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

        private void ToggleTechSelection()
        {
            if (hudController == null || dataCatalog == null || dataCatalog.TechTypes == null || dataCatalog.TechTypes.Length == 0)
            {
                return;
            }

            techSelectionOpen = !techSelectionOpen;
            if (!techSelectionOpen)
            {
                hudController.HideTechPanel();
                return;
            }

            RefreshTechOptions();
        }

        private void RefreshTechOptions()
        {
            if (hudController == null || state?.ActivePlayer == null)
            {
                return;
            }

            var player = state.ActivePlayer;
            availableTechs.Clear();

            if (dataCatalog != null && dataCatalog.TechTypes != null)
            {
                foreach (var tech in dataCatalog.TechTypes)
                {
                    if (tech == null || string.IsNullOrWhiteSpace(tech.Id))
                    {
                        continue;
                    }

                    if (player.KnownTechs.Contains(tech.Id))
                    {
                        continue;
                    }

                    if (!AreTechPrereqsMet(player, tech.Prerequisites))
                    {
                        continue;
                    }

                    availableTechs.Add(tech.Id);
                }
            }

            string option1 = "[J] -";
            string option2 = "[K] -";
            string option3 = "[L] -";
            if (availableTechs.Count > 0)
            {
                option1 = $"[J] {GetTechDisplayName(availableTechs[0])}";
            }
            if (availableTechs.Count > 1)
            {
                option2 = $"[K] {GetTechDisplayName(availableTechs[1])}";
            }
            if (availableTechs.Count > 2)
            {
                option3 = $"[L] {GetTechDisplayName(availableTechs[2])}";
            }

            hudController.ShowTechPanel("Choose Research", option1, option2, option3);
        }

        private void ToggleTechTree()
        {
            if (hudController == null)
            {
                return;
            }

            techTreeOpen = !techTreeOpen;
            if (!techTreeOpen)
            {
                hudController.HideTechTree();
                return;
            }

            hudController.ShowTechTree(BuildTechTreeText());
        }

        private string BuildTechTreeText()
        {
            if (state?.ActivePlayer == null || dataCatalog?.TechTypes == null)
            {
                return "No tech data.";
            }

            var player = state.ActivePlayer;
            var lines = new List<string>();
            foreach (var tech in dataCatalog.TechTypes)
            {
                if (tech == null || string.IsNullOrWhiteSpace(tech.Id))
                {
                    continue;
                }

                string name = !string.IsNullOrWhiteSpace(tech.DisplayName) ? tech.DisplayName : tech.Id;
                string prereq = string.IsNullOrWhiteSpace(tech.Prerequisites) ? "" : " (Requires: " + tech.Prerequisites + ")";
                string status;
                if (player.KnownTechs.Contains(tech.Id))
                {
                    status = "Known";
                }
                else if (player.CurrentTechId == tech.Id)
                {
                    status = "Researching";
                }
                else if (AreTechPrereqsMet(player, tech.Prerequisites))
                {
                    status = "Available";
                }
                else
                {
                    status = "Locked";
                }

                lines.Add( {name}{prereq}");
            }

            return string.Join("
", lines);
        }

        private void HandleTechSelection()
        {
            if (!techSelectionOpen || hudController == null)
            {
                return;
            }

            for (int i = 0; i < techSelectKeys.Length; i++)
            {
                if (Input.GetKeyDown(techSelectKeys[i]))
                {
                    SetTechByIndex(i);
                    break;
                }
            }
        }

        private void SetTechByIndex(int index)
        {
            var player = state?.ActivePlayer;
            if (player == null)
            {
                return;
            }

            if (index < 0 || index >= availableTechs.Count)
            {
                return;
            }

            string techId = availableTechs[index];
            player.CurrentTechId = techId;
            player.ResearchProgress = 0;
            techSelectionOpen = false;
            hudController.HideTechPanel();
            UpdateResearchInfo();
        }

        private string GetTechDisplayName(string techId)
        {
            if (string.IsNullOrWhiteSpace(techId))
            {
                return "-";
            }

            if (dataCatalog != null && dataCatalog.TryGetTechType(techId, out var tech) && !string.IsNullOrWhiteSpace(tech.DisplayName))
            {
                return tech.DisplayName;
            }

            return techId;
        }

        private bool ArePromotionPrereqsMet(Unit unit, string requires)
        {
            if (unit == null || string.IsNullOrWhiteSpace(requires))
            {
                return true;
            }

            var parts = requires.Split(,);
            foreach (var part in parts)
            {
                var id = part.Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (unit.Promotions == null || !unit.Promotions.Contains(id))
                {
                    return false;
                }
            }

            return true;
        }

        private void TogglePromotionSelection()
        {
            if (hudController == null || selectedUnit == null || dataCatalog == null || dataCatalog.PromotionTypes == null)
            {
                return;
            }

            promotionSelectionOpen = !promotionSelectionOpen;
            if (!promotionSelectionOpen)
            {
                hudController.HidePromotionPanel();
                return;
            }

            RefreshPromotionOptions();
        }

        private void RefreshPromotionOptions()
        {
            if (hudController == null || selectedUnit == null)
            {
                return;
            }

            availablePromotions.Clear();
            if (dataCatalog != null && dataCatalog.PromotionTypes != null)
            {
                foreach (var promotion in dataCatalog.PromotionTypes)
                {
                    if (promotion == null || string.IsNullOrWhiteSpace(promotion.Id))
                    {
                        continue;
                    }

                    if (selectedUnit.Promotions.Contains(promotion.Id))
                    {
                        continue;
                    }

                    if (!ArePromotionPrereqsMet(selectedUnit, promotion.Requires))
                    {
                        continue;
                    }

                    availablePromotions.Add(promotion.Id);
                }
            }

            string option1 = "[U] -";
            string option2 = "[I] -";
            string option3 = "[O] -";
            if (availablePromotions.Count > 0)
            {
                option1 = $"[U] {GetPromotionDisplayName(availablePromotions[0])}";
            }
            if (availablePromotions.Count > 1)
            {
                option2 = $"[I] {GetPromotionDisplayName(availablePromotions[1])}";
            }
            if (availablePromotions.Count > 2)
            {
                option3 = $"[O] {GetPromotionDisplayName(availablePromotions[2])}";
            }

            hudController.ShowPromotionPanel(option1, option2, option3);
        }

        private void HandlePromotionSelection()
        {
            if (!promotionSelectionOpen || hudController == null)
            {
                return;
            }

            for (int i = 0; i < promotionSelectKeys.Length; i++)
            {
                if (Input.GetKeyDown(promotionSelectKeys[i]))
                {
                    SetPromotionByIndex(i);
                    break;
                }
            }
        }

        private void SetPromotionByIndex(int index)
        {
            if (selectedUnit == null)
            {
                return;
            }

            if (index < 0 || index >= availablePromotions.Count)
            {
                return;
            }

            string promotionId = availablePromotions[index];
            if (!selectedUnit.Promotions.Contains(promotionId))
            {
                selectedUnit.Promotions.Add(promotionId);
            }

            promotionSelectionOpen = false;
            hudController.HidePromotionPanel();
            UpdatePromotionInfo();
            UpdateHudSelection();
        }

        private string GetPromotionDisplayName(string promotionId)
        {
            if (string.IsNullOrWhiteSpace(promotionId))
            {
                return "-";
            }

            if (dataCatalog != null && dataCatalog.TryGetPromotionType(promotionId, out var promotion) && !string.IsNullOrWhiteSpace(promotion.DisplayName))
            {
                return promotion.DisplayName;
            }

            return promotionId;
        }

        private void ToggleCivicSelection()
        {
            if (hudController == null || dataCatalog == null || dataCatalog.CivicTypes == null || dataCatalog.CivicTypes.Length == 0)
            {
                return;
            }

            civicSelectionOpen = !civicSelectionOpen;
            if (!civicSelectionOpen)
            {
                hudController.HideCivicPanel();
                return;
            }

            RefreshCivicCategories();
            RefreshCivicOptions();
        }

        private void RefreshCivicCategories()
        {
            civicCategories.Clear();
            if (dataCatalog == null || dataCatalog.CivicTypes == null)
            {
                return;
            }

            foreach (var civic in dataCatalog.CivicTypes)
            {
                if (civic == null || string.IsNullOrWhiteSpace(civic.Category))
                {
                    continue;
                }

                if (!civicCategories.Contains(civic.Category))
                {
                    civicCategories.Add(civic.Category);
                }
            }

            if (civicCategoryIndex >= civicCategories.Count)
            {
                civicCategoryIndex = 0;
            }
        }

        private void RefreshCivicOptions()
        {
            if (hudController == null || state?.ActivePlayer == null)
            {
                return;
            }

            var player = state.ActivePlayer;
            if (civicCategories.Count == 0)
            {
                hudController.ShowCivicPanel("Choose Civic", "[4] -", "[5] -", "[6] -");
                return;
            }

            string category = civicCategories[civicCategoryIndex];
            availableCivics.Clear();

            foreach (var civic in dataCatalog.CivicTypes)
            {
                if (civic == null || string.IsNullOrWhiteSpace(civic.Id))
                {
                    continue;
                }

                if (civic.Category != category)
                {
                    continue;
                }

                availableCivics.Add(civic.Id);
            }

            string option1 = "[4] -";
            string option2 = "[5] -";
            string option3 = "[6] -";

            if (availableCivics.Count > 0)
            {
                option1 = $"[4] {GetCivicDisplayName(availableCivics[0], player)}";
            }
            if (availableCivics.Count > 1)
            {
                option2 = $"[5] {GetCivicDisplayName(availableCivics[1], player)}";
            }
            if (availableCivics.Count > 2)
            {
                option3 = $"[6] {GetCivicDisplayName(availableCivics[2], player)}";
            }

            hudController.ShowCivicPanel($"Choose Civic ({category})", option1, option2, option3);
        }

        private void HandleCivicSelection()
        {
            if (!civicSelectionOpen || hudController == null)
            {
                return;
            }

            for (int i = 0; i < civicSelectKeys.Length; i++)
            {
                if (Input.GetKeyDown(civicSelectKeys[i]))
                {
                    SetCivicByIndex(i);
                    break;
                }
            }
        }

        private void SetCivicByIndex(int index)
        {
            var player = state?.ActivePlayer;
            if (player == null || civicCategories.Count == 0)
            {
                return;
            }

            if (index < 0 || index >= availableCivics.Count)
            {
                return;
            }

            string category = civicCategories[civicCategoryIndex];
            string civicId = availableCivics[index];

            bool updated = false;
            foreach (var civic in player.Civics)
            {
                if (civic.Category == category)
                {
                    civic.CivicId = civicId;
                    updated = true;
                    break;
                }
            }

            if (!updated)
            {
                player.Civics.Add(new CivicSelection { Category = category, CivicId = civicId });
            }

            civicSelectionOpen = false;
            hudController.HideCivicPanel();
            UpdateCivicInfo();
        }

        private string GetCivicDisplayName(string civicId, Player player)
        {
            if (string.IsNullOrWhiteSpace(civicId))
            {
                return "-";
            }

            string label = civicId;
            if (dataCatalog != null && dataCatalog.TryGetCivicType(civicId, out var civic) && !string.IsNullOrWhiteSpace(civic.DisplayName))
            {
                label = civic.DisplayName;
            }

            if (player != null)
            {
                foreach (var selected in player.Civics)
                {
                    if (selected.CivicId == civicId)
                    {
                        label = $"{label} (Active)";
                        break;
                    }
                }
            }

            return label;
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

            hudController.SetResearchInfo($"Research: {techName} {player.ResearchProgress}/{cost} [T] Cycle [Y] Choose [H] Tree");
        }

        private void UpdatePromotionInfo()
        {
            if (hudController == null)
            {
                return;
            }

            if (selectedUnit == null)
            {
                hudController.SetPromotionInfo("Promotions: None");
                hudController.SetPromotionDetail(string.Empty);
                return;
            }

            if (selectedUnit.Promotions == null || selectedUnit.Promotions.Count == 0)
            {
                hudController.SetPromotionInfo("Promotions: None");
                hudController.SetPromotionDetail("[U] Promote");
                return;
            }

            var labels = new List<string>();
            foreach (var promo in selectedUnit.Promotions)
            {
                labels.Add(GetPromotionDisplayName(promo));
            }

            hudController.SetPromotionInfo("Promotions: " + string.Join(", ", labels));
            hudController.SetPromotionDetail("[U] Promote");
        }

        private void UpdateCivicInfo()
        {
            if (hudController == null || state?.ActivePlayer == null)
            {
                return;
            }

            var player = state.ActivePlayer;
            if (player.Civics == null || player.Civics.Count == 0)
            {
                hudController.SetCivicInfo("Civics: None [C] Choose");
                return;
            }

            var entries = new List<string>();
            foreach (var civic in player.Civics)
            {
                if (civic == null || string.IsNullOrWhiteSpace(civic.CivicId))
                {
                    continue;
                }

                string name = GetCivicDisplayName(civic.CivicId, null);
                if (!string.IsNullOrWhiteSpace(civic.Category))
                {
                    entries.Add($"{civic.Category}: {name}");
                }
                else
                {
                    entries.Add(name);
                }
            }

            if (entries.Count == 0)
            {
                hudController.SetCivicInfo("Civics: None [C] Choose");
                return;
            }

            hudController.SetCivicInfo("Civics: " + string.Join(", ", entries) + " [C] Choose");
        }

        private void UpdateResourceInfo()
        {
            if (hudController == null || state?.ActivePlayer == null)
            {
                return;
            }

            var player = state.ActivePlayer;
            if (player.AvailableResources == null || player.AvailableResources.Count == 0)
            {
                hudController.SetResourceInfo("Resources: None");
            }
            else
            {
                var names = new List<string>();
                foreach (var resourceId in player.AvailableResources)
                {
                    if (string.IsNullOrWhiteSpace(resourceId))
                    {
                        continue;
                    }

                    if (dataCatalog != null && dataCatalog.TryGetResourceType(resourceId, out var resource) && !string.IsNullOrWhiteSpace(resource.DisplayName))
                    {
                        names.Add(resource.DisplayName);
                    }
                    else
                    {
                        names.Add(resourceId);
                    }
                }

                hudController.SetResourceInfo("Resources: " + string.Join(", ", names));
            }

            if (player.TradeRoutes == null || player.TradeRoutes.Count == 0)
            {
                hudController.SetTradeInfo("Trade Routes: None");
            }
            else
            {
                var entries = new List<string>();
                foreach (var route in player.TradeRoutes)
                {
                    if (route == null || string.IsNullOrWhiteSpace(route.CityA) || string.IsNullOrWhiteSpace(route.CityB))
                    {
                        continue;
                    }

                    entries.Add($"{route.CityA} <-> {route.CityB}");
                }

                hudController.SetTradeInfo(entries.Count == 0 ? "Trade Routes: None" : "Trade Routes: " + string.Join(", ", entries));
            }
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
                else if (selectedUnit.WorkRemaining > 0 && selectedUnit.WorkTargetIsRoad)
                {
                    movementLabel = $"{movementLabel} Work {selectedUnit.WorkRemaining} (Road)";
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
            UpdateResearchInfo();
            UpdateCivicInfo();
            UpdateResourceInfo();
            hudController?.Refresh();
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
            UpdateCivicInfo();
            UpdateResourceInfo();
        }
    }
}
