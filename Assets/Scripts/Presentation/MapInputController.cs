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
        [SerializeField] private KeyCode diplomacyPanelKey = KeyCode.G;
        [SerializeField] private KeyCode[] diplomacySelectKeys = { KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };
        [SerializeField] private KeyCode diplomacyPrevKey = KeyCode.LeftBracket;
        [SerializeField] private KeyCode diplomacyNextKey = KeyCode.RightBracket;
        [SerializeField] private int humanPlayerId = 0;
        [SerializeField] private bool autoEndTurnWhenReady = false;
        [SerializeField] private float autoEndTurnDelay = 0.2f;

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
        private bool diplomacySelectionOpen;
        private int diplomacyPageIndex;
        private int civicCategoryIndex;
        private float autoEndTurnTimer;
        private readonly List<string> availableTechs = new List<string>();
        private readonly List<string> availablePromotions = new List<string>();
        private readonly List<string> availableCivics = new List<string>();
        private readonly List<string> civicCategories = new List<string>();
        private readonly List<int> diplomacyOptionIds = new List<int>();
        private const int DiplomacyPageSize = 3;

        private readonly string[] productionOptions = { "scout", "worker", "settler", "swordsman", "chariot", "axeman" };
        private readonly string[] improvementOptions = { "farm", "mine", "pasture", "camp" };
        private readonly Dictionary<string, string> improvementRequirements = new Dictionary<string, string>
        {
            { "farm", "agriculture" },
            { "mine", "mining" },
            { "pasture", "animal_husbandry" },
            { "camp", "archery" }
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
            UpdateTopBarInfo();
            UpdateDiplomacyInfo();
            if (state?.ActivePlayer != null)
            {
                EnsureDiplomacyState(state.ActivePlayer);
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

            if (Input.GetKeyDown(diplomacyPanelKey))
            {
                ToggleDiplomacySelection();
            }

            HandleDiplomacySelection();

            if (Input.GetKeyDown(endTurnKey))
            {
                bool force = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                EndTurn(force);
            }

            UpdateHoverTooltip();
            UpdateAlertsAndEndTurnState();
            HandleAutoEndTurn();
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

            if (inRange)
            {
                var rangedCity = FindCityAt(tileView.Position);
                if (rangedCity != null && rangedCity.OwnerId != selectedUnit.OwnerId && GetUnitRange(selectedUnit) > 1)
                {
                    if (ResolveCityCombat(selectedUnit, rangedCity, 1, true))
                    {
                        ApplyFog();
                        return;
                    }
                }
            }

            if (!inRange)
            {
                var targetCity = FindCityAt(tileView.Position);
                if (targetCity != null && targetCity.OwnerId != selectedUnit.OwnerId)
                {
                    if (ResolveCityCombat(selectedUnit, targetCity, moveCost, false))
                    {
                        ApplyFog();
                        return;
                    }
                }
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

            UpdateCitySiegeStates();

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

            EnsureDiplomacyState(aiPlayer);
            ConsiderSeekingPeace(aiPlayer);
            ChooseResearch(aiPlayer);
            EnsureCityProduction(aiPlayer);

            foreach (var unit in new List<Unit>(aiPlayer.Units))
            {
            if (unit.UnitTypeId == "settler" && !CityExistsAt(unit.Position))
            {
                var city = new City($"City {aiPlayer.Cities.Count + 1}", unit.Position, aiPlayer.Id, 1);
                city.ProductionTargetId = productionOptions[0];
                city.ProductionCost = GetProductionCost(city.ProductionTargetId);
                aiPlayer.Cities.Add(city);
                aiPlayer.Units.Remove(unit);
                cityPresenter?.RenderCities(state, mapPresenter);
                continue;
            }

                if (unit.UnitTypeId == "worker")
                {
                    if (TryAiBuildImprovementOrRoad(aiPlayer, unit))
                    {
                        continue;
                    }
                }

                if (TryMoveUnitTowardsWarTarget(aiPlayer, unit))
                {
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

        private bool TryMoveUnitTowardsWarTarget(Player aiPlayer, Unit unit)
        {
            if (aiPlayer == null || unit == null)
            {
                return false;
            }

            var target = FindNearestWarTarget(aiPlayer, unit.Position);
            if (!target.HasValue)
            {
                return false;
            }

            var targetCity = FindCityAt(target.Value);
            if (targetCity != null)
            {
                if (TryRangedAttackCity(unit, targetCity))
                {
                    return true;
                }

                int distance = Mathf.Abs(unit.Position.X - targetCity.Position.X) + Mathf.Abs(unit.Position.Y - targetCity.Position.Y);
                if (distance == 1 && unit.MovementRemaining < unit.MovementPoints)
                {
                    return true;
                }
            }

            return TryMoveUnitTowards(unit, target.Value);
        }

        private GridPosition? FindNearestWarTarget(Player aiPlayer, GridPosition origin)
        {
            if (state?.Players == null)
            {
                return null;
            }

            int bestDistance = int.MaxValue;
            GridPosition best = default;
            bool found = false;

            foreach (var other in state.Players)
            {
                if (other == null || other.Id == aiPlayer.Id)
                {
                    continue;
                }

                if (!IsAtWar(aiPlayer, other.Id))
                {
                    continue;
                }

                foreach (var city in other.Cities)
                {
                    int distance = Mathf.Abs(city.Position.X - origin.X) + Mathf.Abs(city.Position.Y - origin.Y);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = city.Position;
                        found = true;
                    }
                }

                foreach (var unit in other.Units)
                {
                    int distance = Mathf.Abs(unit.Position.X - origin.X) + Mathf.Abs(unit.Position.Y - origin.Y);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = unit.Position;
                        found = true;
                    }
                }
            }

            return found ? best : (GridPosition?)null;
        }

        private bool TryMoveUnitTowards(Unit unit, GridPosition target)
        {
            if (unit == null || state?.Map == null)
            {
                return false;
            }

            var directions = new[]
            {
                new GridPosition(1, 0),
                new GridPosition(-1, 0),
                new GridPosition(0, 1),
                new GridPosition(0, -1)
            };

            int bestDistance = int.MaxValue;
            GridPosition best = unit.Position;
            int bestCost = 0;

            foreach (var offset in directions)
            {
                var candidate = new GridPosition(unit.Position.X + offset.X, unit.Position.Y + offset.Y);
                var tile = state.Map.GetTile(candidate.X, candidate.Y);
                if (tile == null)
                {
                    continue;
                }

                int moveCost = GetMoveCost(candidate);
                if (unit.MovementRemaining < moveCost)
                {
                    continue;
                }

                int distance = Mathf.Abs(candidate.X - target.X) + Mathf.Abs(candidate.Y - target.Y);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                    bestCost = moveCost;
                }
            }

            if (bestDistance == int.MaxValue)
            {
                return false;
            }

            return TryMoveUnitTo(unit, best, bestCost);
        }

        private bool TryMoveUnitTo(Unit unit, GridPosition target, int moveCost)
        {
            if (unit == null)
            {
                return false;
            }

            var city = FindCityAt(target);
            if (city != null && city.OwnerId != unit.OwnerId)
            {
                return ResolveCityCombat(unit, city, moveCost, false);
            }

            var occupant = FindUnitAt(target);
            if (occupant != null)
            {
                if (occupant.OwnerId == unit.OwnerId)
                {
                    return false;
                }

                ResolveCombat(unit, occupant, moveCost);
                return true;
            }

            unit.Position = target;
            unit.MovementRemaining = Mathf.Max(0, unit.MovementRemaining - moveCost);
            return true;
        }

        private void UpdateCitySiegeStates()
        {
            if (state?.Players == null)
            {
                return;
            }

            foreach (var player in state.Players)
            {
                foreach (var city in player.Cities)
                {
                    city.UnderSiege = false;
                }
            }

            foreach (var player in state.Players)
            {
                foreach (var unit in player.Units)
                {
                    foreach (var otherPlayer in state.Players)
                    {
                        if (otherPlayer == null || otherPlayer.Id == player.Id)
                        {
                            continue;
                        }

                        if (!IsAtWar(player, otherPlayer.Id))
                        {
                            continue;
                        }

                        foreach (var city in otherPlayer.Cities)
                        {
                            int distance = Mathf.Abs(unit.Position.X - city.Position.X) + Mathf.Abs(unit.Position.Y - city.Position.Y);
                            if (distance <= 1)
                            {
                                city.UnderSiege = true;
                            }
                        }
                    }
                }
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

        private City FindCityAt(GridPosition position)
        {
            if (state?.Players == null)
            {
                return null;
            }

            foreach (var player in state.Players)
            {
                foreach (var city in player.Cities)
                {
                    if (city.Position.X == position.X && city.Position.Y == position.Y)
                    {
                        return city;
                    }
                }
            }

            return null;
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

        private Player GetPlayerById(int playerId)
        {
            if (state?.Players == null)
            {
                return null;
            }

            foreach (var player in state.Players)
            {
                if (player != null && player.Id == playerId)
                {
                    return player;
                }
            }

            return null;
        }

        private void EnsureDiplomacyState(Player player)
        {
            if (player == null || state?.Players == null)
            {
                return;
            }

            if (player.Diplomacy == null)
            {
                player.Diplomacy = new List<DiplomacyStatus>();
            }

            foreach (var other in state.Players)
            {
                if (other == null || other.Id == player.Id)
                {
                    continue;
                }

                bool exists = false;
                foreach (var status in player.Diplomacy)
                {
                    if (status != null && status.OtherPlayerId == other.Id)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    player.Diplomacy.Add(new DiplomacyStatus
                    {
                        OtherPlayerId = other.Id,
                        AtWar = false
                    });
                }
            }
        }

        private bool IsAtWar(Player player, int otherPlayerId)
        {
            if (player?.Diplomacy == null)
            {
                return false;
            }

            foreach (var status in player.Diplomacy)
            {
                if (status != null && status.OtherPlayerId == otherPlayerId)
                {
                    return status.AtWar;
                }
            }

            return false;
        }

        private void SetWarStatus(Player player, int otherPlayerId, bool atWar)
        {
            if (player == null)
            {
                return;
            }

            EnsureDiplomacyState(player);
            foreach (var status in player.Diplomacy)
            {
                if (status != null && status.OtherPlayerId == otherPlayerId)
                {
                    status.AtWar = atWar;
                    return;
                }
            }
        }

        private void ConsiderSeekingPeace(Player aiPlayer)
        {
            if (aiPlayer == null || state?.Players == null)
            {
                return;
            }

            foreach (var other in state.Players)
            {
                if (other == null || other.Id == aiPlayer.Id)
                {
                    continue;
                }

                if (!IsAtWar(aiPlayer, other.Id))
                {
                    continue;
                }

                bool weakerUnits = aiPlayer.Units.Count < other.Units.Count;
                bool fewerCities = aiPlayer.Cities.Count < other.Cities.Count;
                if (weakerUnits && fewerCities && Random.Range(0, 100) < 35)
                {
                    SetWarStatus(aiPlayer, other.Id, false);
                    SetWarStatus(other, aiPlayer.Id, false);
                }
            }
        }

        private bool ResolveCityCombat(Unit unit, City city, int moveCost, bool isRanged)
        {
            if (unit == null || city == null || state == null)
            {
                return false;
            }

            var attackerPlayer = GetPlayerById(unit.OwnerId);
            var defenderPlayer = GetPlayerById(city.OwnerId);
            if (attackerPlayer == null || defenderPlayer == null)
            {
                return false;
            }

            if (!IsAtWar(attackerPlayer, defenderPlayer.Id))
            {
                SetWarStatus(attackerPlayer, defenderPlayer.Id, true);
                SetWarStatus(defenderPlayer, attackerPlayer.Id, true);
                hudController?.LogCombat($"War declared between {attackerPlayer.Name} and {defenderPlayer.Name}");
                UpdateDiplomacyInfo();
            }

            int attack = GetAttack(unit);
            int defense = GetCityDefense(city);
            int attackRoll = attack + Random.Range(0, 6);
            int defenseRoll = defense + Random.Range(0, 6);
            int damage = Mathf.Clamp(attackRoll - defenseRoll + 1, 1, 6);
            int nextHealth = Mathf.Max(0, city.Health - damage);
            if (isRanged && nextHealth <= 0)
            {
                nextHealth = 1;
            }
            city.Health = nextHealth;
            hudController?.SetEventMessage($"City hit for {damage}");
            hudController?.LogCombat($"{unit.UnitTypeId} hit {city.Name} for {damage}");

            unit.MovementRemaining = Mathf.Max(0, unit.MovementRemaining - moveCost);

            if (city.Health > 0)
            {
                UpdateCityInfo();
                return true;
            }

            CaptureCity(attackerPlayer, defenderPlayer, unit, city, moveCost);
            return true;
        }

        private bool TryRangedAttackCity(Unit unit, City city)
        {
            if (unit == null || city == null)
            {
                return false;
            }

            int range = GetUnitRange(unit);
            if (range <= 1)
            {
                return false;
            }

            int distance = Mathf.Abs(unit.Position.X - city.Position.X) + Mathf.Abs(unit.Position.Y - city.Position.Y);
            if (distance > range || unit.MovementRemaining <= 0)
            {
                return false;
            }

            return ResolveCityCombat(unit, city, 1, true);
        }

        private int GetCityDefense(City city)
        {
            if (city == null)
            {
                return 1;
            }

            int defense = 2 + Mathf.Max(0, city.Population / 2);
            if (city.Health < city.MaxHealth / 2)
            {
                defense = Mathf.Max(1, defense - 1);
            }
            return defense;
        }

        private void CaptureCity(Player attackerPlayer, Player defenderPlayer, Unit unit, City city, int moveCost)
        {
            if (attackerPlayer == null || defenderPlayer == null || unit == null || city == null)
            {
                return;
            }

            if (!defenderPlayer.Cities.Remove(city))
            {
                return;
            }

            city.OwnerId = attackerPlayer.Id;
            if (city.MaxHealth <= 0)
            {
                city.MaxHealth = City.GetDefaultMaxHealth(city.Population);
            }
            city.Health = city.MaxHealth;
            city.UnderSiege = false;
            attackerPlayer.Cities.Add(city);
            unit.Position = city.Position;
            unit.MovementRemaining = Mathf.Max(0, unit.MovementRemaining - moveCost);

            unitPresenter?.RenderUnits(state, mapPresenter);
            cityPresenter?.RenderCities(state, mapPresenter);
            UpdateHudSelection($"Captured {city.Name}");
            UpdateCityInfo();
            UpdateDiplomacyInfo();
        }

        private void ChooseResearch(Player player)
        {
            if (player == null || dataCatalog?.TechTypes == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(player.CurrentTechId) && dataCatalog.TryGetTechType(player.CurrentTechId, out var existingTech))
            {
                if (AreTechPrereqsMet(player, existingTech.Prerequisites))
                {
                    return;
                }
            }

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

                player.CurrentTechId = tech.Id;
                player.ResearchProgress = 0;
                return;
            }
        }

        private void EnsureCityProduction(Player player)
        {
            if (player == null)
            {
                return;
            }

            foreach (var city in player.Cities)
            {
                if (city == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(city.ProductionTargetId))
                {
                    continue;
                }

                string chosen = ChooseBestProductionId(player);
                if (!string.IsNullOrWhiteSpace(chosen))
                {
                    city.ProductionTargetId = chosen;
                    city.ProductionCost = GetProductionCost(chosen);
                }
            }
        }

        private string ChooseBestProductionId(Player player)
        {
            if (player == null || dataCatalog?.UnitTypes == null)
            {
                return productionOptions.Length > 0 ? productionOptions[0] : string.Empty;
            }

            foreach (var option in productionOptions)
            {
                if (dataCatalog.TryGetUnitType(option, out var unit) && unit != null)
                {
                    if (!HasRequiredResource(player, unit.RequiresResource))
                    {
                        continue;
                    }
                    if (!HasRequiredTech(player, unit.RequiresTech))
                    {
                        continue;
                    }
                    return option;
                }
            }

            return productionOptions.Length > 0 ? productionOptions[0] : string.Empty;
        }

        private bool TryAiBuildImprovementOrRoad(Player player, Unit unit)
        {
            if (unit == null || player == null || state?.Map == null)
            {
                return false;
            }

            if (unit.WorkRemaining > 0)
            {
                return false;
            }

            var tile = state.Map.GetTile(unit.Position.X, unit.Position.Y);
            if (tile == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(tile.ImprovementId))
            {
                string improvementId = null;
                for (int i = 0; i < improvementOptions.Length; i++)
                {
                    var candidate = improvementOptions[i];
                    if (improvementRequirements.TryGetValue(candidate, out var techReq) && !string.IsNullOrWhiteSpace(techReq))
                    {
                        if (!player.KnownTechs.Contains(techReq))
                        {
                            continue;
                        }
                    }

                    improvementId = candidate;
                    break;
                }

                if (!string.IsNullOrWhiteSpace(improvementId))
                {
                    int workCost = GetWorkCost(unit);
                    unit.StartWork(unit.Position, improvementId, workCost);
                    unit.MovementRemaining = 0;
                    return true;
                }
            }

            if (!tile.HasRoad)
            {
                int workCost = GetWorkCost(unit);
                unit.StartRoadWork(unit.Position, workCost);
                unit.MovementRemaining = 0;
                return true;
            }

            return false;
        }

        private void ResolveCombat(Unit attacker, Unit defender, int moveCost)
        {
            if (attacker == null || defender == null || state == null)
            {
                return;
            }

            var attackerPlayer = GetPlayerById(attacker.OwnerId);
            var defenderPlayer = GetPlayerById(defender.OwnerId);
            if (attackerPlayer != null && defenderPlayer != null && !IsAtWar(attackerPlayer, defenderPlayer.Id))
            {
                SetWarStatus(attackerPlayer, defenderPlayer.Id, true);
                SetWarStatus(defenderPlayer, attackerPlayer.Id, true);
                hudController?.LogCombat($"War declared between {attackerPlayer.Name} and {defenderPlayer.Name}");
                UpdateDiplomacyInfo();
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
                hudController.SetCityPanel("Name: None", "Population: -", "Growth: -", "Production: -", "Defense: -");
                return;
            }

            int foodNeeded = 5 + selectedCity.Population * 2;
            hudController.SetCityInfo($"City: {selectedCity.Name} (Pop {selectedCity.Population}) HP {selectedCity.Health}/{selectedCity.MaxHealth} Food {selectedCity.FoodStored}/{foodNeeded} (+{selectedCity.FoodPerTurn})");

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
            string resourceHint = BuildProductionResourceHint(state?.ActivePlayer);
            if (!string.IsNullOrWhiteSpace(resourceHint))
            {
                resourceHint = " " + resourceHint;
            }
            hudController.SetProductionInfo($"Production: {targetName} {selectedCity.ProductionStored}/{selectedCity.ProductionCost} (+{selectedCity.ProductionPerTurn}) {turns}t [P] Cycle {optionsHint}{queueInfo}{resourceHint}");
            hudController.SetProductionTooltip(BuildProductionTooltip(state?.ActivePlayer));

            int defense = 2 + Mathf.Max(0, selectedCity.Population / 2);
            if (selectedCity.MaxHealth > 0 && selectedCity.Health < selectedCity.MaxHealth / 2)
            {
                defense = Mathf.Max(1, defense - 1);
            }

            hudController.SetCityPanel(
                $"Name: {selectedCity.Name}",
                $"Population: {selectedCity.Population}",
                $"Growth: {selectedCity.FoodStored}/{foodNeeded} (+{selectedCity.FoodPerTurn})",
                $"Production: {targetName} {selectedCity.ProductionStored}/{selectedCity.ProductionCost}",
                $"Defense: {defense}"
            );
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

        private bool HasRequiredTech(Player player, string techId)
        {
            if (player == null || string.IsNullOrWhiteSpace(techId))
            {
                return true;
            }

            return player.KnownTechs != null && player.KnownTechs.Contains(techId);
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
            if (unitType != null && !HasRequiredTech(state?.ActivePlayer, unitType.RequiresTech))
            {
                UpdateHudSelection("Requires tech");
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
                    if (unitType != null && !HasRequiredTech(state?.ActivePlayer, unitType.RequiresTech))
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

            var parts = prereqList.Split(',');
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
            hudController.SetTechTreeTooltip("Tech Tree: columns are tiers by prerequisites. [K]=Known [R]=Researching [A]=Available [L]=Locked.");
        }

        private string BuildTechTreeText()
        {
            if (state?.ActivePlayer == null || dataCatalog?.TechTypes == null)
            {
                return "No tech data.";
            }

            var player = state.ActivePlayer;
            var lines = new List<string>();
            lines.Add("Legend: [K] Known [R] Researching [A] Available [L] Locked");
            if (!string.IsNullOrWhiteSpace(player.CurrentTechId) && dataCatalog.TryGetTechType(player.CurrentTechId, out var currentTech))
            {
                string currentName = string.IsNullOrWhiteSpace(currentTech.DisplayName) ? currentTech.Id : currentTech.DisplayName;
                lines.Add($"Current: {currentName} {player.ResearchProgress}/{currentTech.Cost}");
            }
            var tiers = BuildTechTiers();
            if (tiers.Count == 0)
            {
                lines.Add("No tech tiers.");
                return string.Join("\n", lines);
            }

            var columnWidths = BuildTechColumnWidths(tiers);
            lines.Add(BuildTechHeaderRow(tiers.Count, columnWidths));
            lines.Add(BuildTechDividerRow(tiers.Count, columnWidths));

            int maxRows = 0;
            for (int i = 0; i < tiers.Count; i++)
            {
                if (tiers[i].Count > maxRows)
                {
                    maxRows = tiers[i].Count;
                }
            }

            for (int row = 0; row < maxRows; row++)
            {
                var rowLine = new System.Text.StringBuilder();
                var connectorLine = new System.Text.StringBuilder();
                bool rowHasCurrent = false;
                bool rowHasConnector = false;
                for (int col = 0; col < tiers.Count; col++)
                {
                    var tier = tiers[col];
                    string cell = string.Empty;
                    bool hasPrereq = false;
                    if (row < tier.Count)
                    {
                        var tech = tier[row];
                        string name = !string.IsNullOrWhiteSpace(tech.DisplayName) ? tech.DisplayName : tech.Id;
                        string prereq = string.IsNullOrWhiteSpace(tech.Prerequisites) ? "" : " <- " + tech.Prerequisites;
                        string status = GetTechStatusTag(player, tech);
                        cell = $"{status} {name}{prereq}";
                        hasPrereq = !string.IsNullOrWhiteSpace(tech.Prerequisites);
                        if (player.CurrentTechId == tech.Id)
                        {
                            rowHasCurrent = true;
                        }
                    }

                    rowLine.Append(PadColumn(cell, columnWidths[col]));
                    string connectorCell = hasPrereq ? "  ^" : string.Empty;
                    connectorLine.Append(PadColumn(connectorCell, columnWidths[col]));
                    if (hasPrereq)
                    {
                        rowHasConnector = true;
                    }
                    if (col < tiers.Count - 1)
                    {
                        rowLine.Append("  ");
                        connectorLine.Append("  ");
                    }
                }
                string rowPrefix = rowHasCurrent ? ">> " : "   ";
                lines.Add(rowPrefix + rowLine);
                if (rowHasConnector)
                {
                    lines.Add("   " + connectorLine);
                }
            }

            return string.Join("\n", lines);
        }

        private List<List<TechType>> BuildTechTiers()
        {
            var tiers = new List<List<TechType>>();
            if (dataCatalog?.TechTypes == null)
            {
                return tiers;
            }

            var techLookup = new Dictionary<string, TechType>();
            foreach (var tech in dataCatalog.TechTypes)
            {
                if (tech == null || string.IsNullOrWhiteSpace(tech.Id))
                {
                    continue;
                }

                techLookup[tech.Id] = tech;
            }

            var memo = new Dictionary<string, int>();
            foreach (var tech in techLookup.Values)
            {
                int tier = GetTechTier(tech.Id, techLookup, memo);
                while (tiers.Count <= tier)
                {
                    tiers.Add(new List<TechType>());
                }
                tiers[tier].Add(tech);
            }

            foreach (var tier in tiers)
            {
                tier.Sort((a, b) => string.Compare(a.DisplayName ?? a.Id, b.DisplayName ?? b.Id, System.StringComparison.Ordinal));
            }

            return tiers;
        }

        private string GetTechStatusTag(Player player, TechType tech)
        {
            if (player.KnownTechs.Contains(tech.Id))
            {
                return "[K]";
            }
            if (player.CurrentTechId == tech.Id)
            {
                return "[R]";
            }
            if (AreTechPrereqsMet(player, tech.Prerequisites))
            {
                return "[A]";
            }
            return "[L]";
        }

        private int[] BuildTechColumnWidths(List<List<TechType>> tiers)
        {
            var widths = new int[tiers.Count];
            for (int i = 0; i < tiers.Count; i++)
            {
                int max = $"Tier {i}".Length;
                foreach (var tech in tiers[i])
                {
                    if (tech == null)
                    {
                        continue;
                    }

                    string name = !string.IsNullOrWhiteSpace(tech.DisplayName) ? tech.DisplayName : tech.Id;
                    string prereq = string.IsNullOrWhiteSpace(tech.Prerequisites) ? "" : " <- " + tech.Prerequisites;
                    int len = "[L] ".Length + name.Length + prereq.Length;
                    if (len > max)
                    {
                        max = len;
                    }
                }
                widths[i] = Mathf.Max(10, max);
            }
            return widths;
        }

        private string BuildTechHeaderRow(int columns, int[] widths)
        {
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < columns; i++)
            {
                string title = $"Tier {i}";
                builder.Append(PadColumn(title, widths[i]));
                if (i < columns - 1)
                {
                    builder.Append("  ");
                }
            }
            return builder.ToString();
        }

        private string BuildTechDividerRow(int columns, int[] widths)
        {
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < columns; i++)
            {
                builder.Append(new string('-', widths[i]));
                if (i < columns - 1)
                {
                    builder.Append("  ");
                }
            }
            return builder.ToString();
        }

        private string PadColumn(string value, int width)
        {
            value = value ?? string.Empty;
            if (value.Length >= width)
            {
                return value;
            }
            return value.PadRight(width);
        }

        private int GetTechTier(string techId, Dictionary<string, TechType> lookup, Dictionary<string, int> memo)
        {
            if (string.IsNullOrWhiteSpace(techId))
            {
                return 0;
            }

            if (memo.TryGetValue(techId, out var cached))
            {
                return cached;
            }

            if (!lookup.TryGetValue(techId, out var tech) || string.IsNullOrWhiteSpace(tech.Prerequisites))
            {
                memo[techId] = 0;
                return 0;
            }

            int maxTier = 0;
            var parts = tech.Prerequisites.Split(',');
            foreach (var part in parts)
            {
                var prereqId = part.Trim();
                if (string.IsNullOrWhiteSpace(prereqId))
                {
                    continue;
                }

                int prereqTier = GetTechTier(prereqId, lookup, memo);
                if (prereqTier + 1 > maxTier)
                {
                    maxTier = prereqTier + 1;
                }
            }

            memo[techId] = maxTier;
            return maxTier;
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

            var parts = requires.Split(',');
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

        private void ToggleDiplomacySelection()
        {
            if (hudController == null || state?.ActivePlayer == null || state.Players == null || state.Players.Count <= 1)
            {
                return;
            }

            diplomacySelectionOpen = !diplomacySelectionOpen;
            if (!diplomacySelectionOpen)
            {
                hudController.HideDiplomacyPanel();
                return;
            }

            diplomacyPageIndex = 0;
            RefreshDiplomacyOptions();
        }

        private void RefreshDiplomacyOptions()
        {
            if (hudController == null || state?.ActivePlayer == null || state.Players == null)
            {
                return;
            }

            var player = state.ActivePlayer;
            EnsureDiplomacyState(player);
            diplomacyOptionIds.Clear();

            var rivals = new List<Player>();
            foreach (var other in state.Players)
            {
                if (other == null || other.Id == player.Id)
                {
                    continue;
                }

                rivals.Add(other);
            }

            int totalPages = Mathf.Max(1, Mathf.CeilToInt(rivals.Count / (float)DiplomacyPageSize));
            diplomacyPageIndex = Mathf.Clamp(diplomacyPageIndex, 0, totalPages - 1);
            int startIndex = diplomacyPageIndex * DiplomacyPageSize;

            string option1 = "[7] -";
            string option2 = "[8] -";
            string option3 = "[9] -";

            for (int i = 0; i < DiplomacyPageSize; i++)
            {
                int idx = startIndex + i;
                if (idx >= rivals.Count)
                {
                    break;
                }

                var other = rivals[idx];
                bool atWar = IsAtWar(player, other.Id);
                string status = atWar ? "War" : "Peace";
                string action = atWar ? "Make Peace" : "Declare War";
                string name = string.IsNullOrWhiteSpace(other.Name) ? $"Player {other.Id}" : other.Name;
                string optionText = $"[{7 + i}] {name} ({status}) - {action}";

                if (i == 0) option1 = optionText;
                else if (i == 1) option2 = optionText;
                else if (i == 2) option3 = optionText;

                diplomacyOptionIds.Add(other.Id);
            }

            string title = $"Diplomacy ({diplomacyPageIndex + 1}/{totalPages}) [[/]]";
            hudController.ShowDiplomacyPanel(title, option1, option2, option3);
        }

        private void HandleDiplomacySelection()
        {
            if (!diplomacySelectionOpen || hudController == null)
            {
                return;
            }

            if (Input.GetKeyDown(diplomacyPrevKey))
            {
                diplomacyPageIndex = Mathf.Max(0, diplomacyPageIndex - 1);
                RefreshDiplomacyOptions();
                return;
            }

            if (Input.GetKeyDown(diplomacyNextKey))
            {
                diplomacyPageIndex += 1;
                RefreshDiplomacyOptions();
                return;
            }

            for (int i = 0; i < diplomacySelectKeys.Length; i++)
            {
                if (Input.GetKeyDown(diplomacySelectKeys[i]))
                {
                    SetDiplomacyByIndex(i);
                    break;
                }
            }
        }

        private void SetDiplomacyByIndex(int index)
        {
            var player = state?.ActivePlayer;
            if (player == null)
            {
                return;
            }

            if (index < 0 || index >= diplomacyOptionIds.Count)
            {
                return;
            }

            int otherId = diplomacyOptionIds[index];
            var other = GetPlayerById(otherId);
            if (other == null)
            {
                return;
            }

            bool atWar = IsAtWar(player, otherId);
            bool newStatus = !atWar;
            SetWarStatus(player, otherId, newStatus);
            SetWarStatus(other, player.Id, newStatus);

            string message = newStatus ? $"War declared with {other.Name}" : $"Peace declared with {other.Name}";
            hudController.SetEventMessage(message);

            diplomacySelectionOpen = false;
            hudController.HideDiplomacyPanel();
            UpdateDiplomacyInfo();
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

            var resourceUnits = BuildResourceUnitList(player);
            hudController.SetResourceUnitInfo(string.IsNullOrWhiteSpace(resourceUnits) ? "Resource Units: None" : "Resource Units: " + resourceUnits);
            hudController.SetResourceUnitTooltip(BuildResourceUnitTooltip(player));

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

            UpdateTopBarInfo();
        }

        private void UpdateDiplomacyInfo()
        {
            if (hudController == null || state?.ActivePlayer == null)
            {
                return;
            }

            var player = state.ActivePlayer;
            EnsureDiplomacyState(player);

            var wars = new List<string>();
            foreach (var status in player.Diplomacy)
            {
                if (status == null || !status.AtWar)
                {
                    continue;
                }

                var other = GetPlayerById(status.OtherPlayerId);
                if (other == null)
                {
                    continue;
                }

                wars.Add(string.IsNullOrWhiteSpace(other.Name) ? $"Player {other.Id}" : other.Name);
            }

            if (wars.Count == 0)
            {
                hudController.SetDiplomacyStatus("Diplomacy: Peace");
            }
            else
            {
                hudController.SetDiplomacyStatus("Diplomacy: At war with " + string.Join(", ", wars));
            }
        }

        private string BuildResourceUnitList(Player player)
        {
            if (player == null || dataCatalog?.UnitTypes == null)
            {
                return string.Empty;
            }

            var unlocked = new List<string>();
            var locked = new List<string>();
            foreach (var unit in dataCatalog.UnitTypes)
            {
                if (unit == null || string.IsNullOrWhiteSpace(unit.RequiresResource))
                {
                    continue;
                }

                string name = string.IsNullOrWhiteSpace(unit.DisplayName) ? unit.Id : unit.DisplayName;
                bool hasResource = HasRequiredResource(player, unit.RequiresResource);
                bool hasTech = HasRequiredTech(player, unit.RequiresTech);
                if (hasResource && hasTech)
                {
                    unlocked.Add(name);
                }
                else
                {
                    var reasons = new List<string>();
                    if (!hasResource)
                    {
                        reasons.Add(unit.RequiresResource);
                    }
                    if (!hasTech)
                    {
                        reasons.Add(unit.RequiresTech);
                    }
                    locked.Add($"{name} ({string.Join("/", reasons)})");
                }
            }

            if (unlocked.Count == 0 && locked.Count == 0)
            {
                return string.Empty;
            }

            if (unlocked.Count == 0)
            {
                return "Locked: " + string.Join(", ", locked);
            }

            if (locked.Count == 0)
            {
                return "Unlocked: " + string.Join(", ", unlocked);
            }

            return "Unlocked: " + string.Join(", ", unlocked) + " | Locked: " + string.Join(", ", locked);
        }

        private string BuildResourceUnitTooltip(Player player)
        {
            if (player == null || dataCatalog?.UnitTypes == null)
            {
                return string.Empty;
            }

            var lines = new List<string> { "Units with resource requirements:" };
            foreach (var unit in dataCatalog.UnitTypes)
            {
                if (unit == null || string.IsNullOrWhiteSpace(unit.RequiresResource))
                {
                    continue;
                }

                string name = string.IsNullOrWhiteSpace(unit.DisplayName) ? unit.Id : unit.DisplayName;
                bool hasResource = HasRequiredResource(player, unit.RequiresResource);
                bool hasTech = HasRequiredTech(player, unit.RequiresTech);
                string status = (hasResource && hasTech) ? "Unlocked" : "Locked";
                lines.Add($"{name} - {status} (Res: {unit.RequiresResource}, Tech: {unit.RequiresTech})");
            }

            return string.Join("\n", lines);
        }

        private string BuildProductionResourceHint(Player player)
        {
            if (player == null || dataCatalog?.UnitTypes == null)
            {
                return string.Empty;
            }

            var unlocked = new List<string>();
            var locked = new List<string>();
            foreach (var unit in dataCatalog.UnitTypes)
            {
                if (unit == null || string.IsNullOrWhiteSpace(unit.RequiresResource))
                {
                    continue;
                }

                string name = string.IsNullOrWhiteSpace(unit.DisplayName) ? unit.Id : unit.DisplayName;
                bool hasResource = HasRequiredResource(player, unit.RequiresResource);
                bool hasTech = HasRequiredTech(player, unit.RequiresTech);
                if (hasResource && hasTech)
                {
                    unlocked.Add(name);
                }
                else
                {
                    var reasons = new List<string>();
                    if (!hasResource)
                    {
                        reasons.Add(unit.RequiresResource);
                    }
                    if (!hasTech)
                    {
                        reasons.Add(unit.RequiresTech);
                    }
                    locked.Add($"{name}({string.Join("/", reasons)})");
                }
            }

            if (unlocked.Count == 0 && locked.Count == 0)
            {
                return string.Empty;
            }

            if (unlocked.Count == 0)
            {
                return "Units: Locked " + string.Join(", ", locked);
            }

            if (locked.Count == 0)
            {
                return "Units: Unlocked " + string.Join(", ", unlocked);
            }

            return "Units: +" + string.Join(", ", unlocked) + " / -" + string.Join(", ", locked);
        }

        private string BuildProductionTooltip(Player player)
        {
            if (player == null || dataCatalog?.UnitTypes == null)
            {
                return string.Empty;
            }

            var lines = new List<string> { "Production options:" };
            foreach (var unitId in productionOptions)
            {
                if (dataCatalog == null || !dataCatalog.TryGetUnitType(unitId, out var unitType) || unitType == null)
                {
                    continue;
                }

                bool hasResource = HasRequiredResource(player, unitType.RequiresResource);
                bool hasTech = HasRequiredTech(player, unitType.RequiresTech);
                string status = (hasResource && hasTech) ? "OK" : "Locked";
                string name = string.IsNullOrWhiteSpace(unitType.DisplayName) ? unitType.Id : unitType.DisplayName;
                string req = string.Empty;
                if (!string.IsNullOrWhiteSpace(unitType.RequiresResource) || !string.IsNullOrWhiteSpace(unitType.RequiresTech))
                {
                    req = $" (Res: {unitType.RequiresResource}, Tech: {unitType.RequiresTech})";
                }
                lines.Add($"{name} - {status}{req}");
            }

            return string.Join("\n", lines);
        }

        private void UpdateTopBarInfo()
        {
            if (hudController == null || state?.ActivePlayer == null)
            {
                return;
            }

            int foodStored = 0;
            int foodPerTurn = 0;
            int prodStored = 0;
            int prodPerTurn = 0;

            foreach (var city in state.ActivePlayer.Cities)
            {
                foodStored += Mathf.Max(0, city.FoodStored);
                foodPerTurn += Mathf.Max(0, city.FoodPerTurn);
                prodStored += Mathf.Max(0, city.ProductionStored);
                prodPerTurn += Mathf.Max(0, city.ProductionPerTurn);
            }

            hudController.SetTopBarYields(
                "Gold: 0 (+0)",
                "Science: 0 (+0)",
                "Culture: 0 (+0)",
                $"Food: {foodStored} (+{foodPerTurn})",
                $"Production: {prodStored} (+{prodPerTurn})"
            );

            hudController.SetTopBarTooltips(
                "Gold per turn is not implemented yet.",
                "Science per turn is not implemented yet.",
                "Culture per turn is not implemented yet.",
                BuildFoodTooltip(state.ActivePlayer),
                BuildProductionYieldTooltip(state.ActivePlayer)
            );
        }

        private string BuildFoodTooltip(Player player)
        {
            if (player == null)
            {
                return string.Empty;
            }

            var lines = new List<string> { "Food per city:" };
            foreach (var city in player.Cities)
            {
                int needed = 5 + city.Population * 2;
                lines.Add($"{city.Name}: {city.FoodStored}/{needed} (+{city.FoodPerTurn})");
            }
            return string.Join("\n", lines);
        }

        private string BuildProductionYieldTooltip(Player player)
        {
            if (player == null)
            {
                return string.Empty;
            }

            var lines = new List<string> { "Production per city:" };
            foreach (var city in player.Cities)
            {
                string targetName = city.ProductionTargetId;
                if (dataCatalog != null && dataCatalog.TryGetUnitType(city.ProductionTargetId, out var unitType) && !string.IsNullOrWhiteSpace(unitType.DisplayName))
                {
                    targetName = unitType.DisplayName;
                }
                lines.Add($"{city.Name}: {city.ProductionStored}/{city.ProductionCost} (+{city.ProductionPerTurn}) {targetName}");
            }
            return string.Join("\n", lines);
        }

        private void UpdateHoverTooltip()
        {
            if (hudController == null || sceneCamera == null || state?.Map == null)
            {
                return;
            }

            Vector3 world = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point = new Vector2(world.x, world.y);
            RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero);

            if (hit.collider == null)
            {
                hudController.SetTooltip(string.Empty);
                return;
            }

            var unitView = hit.collider.GetComponent<UnitView>();
            if (unitView != null && unitView.Unit != null)
            {
                hudController.SetTooltip(BuildUnitTooltip(unitView.Unit));
                return;
            }

            var cityHover = hit.collider.GetComponent<CityHover>();
            if (cityHover != null && cityHover.City != null)
            {
                hudController.SetTooltip(BuildCityTooltip(cityHover.City));
                return;
            }

            var tileView = hit.collider.GetComponent<TileView>();
            if (tileView != null)
            {
                hudController.SetTooltip(BuildTileTooltip(tileView.Position));
                return;
            }

            hudController.SetTooltip(string.Empty);
        }

        private string BuildUnitTooltip(Unit unit)
        {
            if (unit == null)
            {
                return string.Empty;
            }

            string name = unit.UnitTypeId;
            int attack = 0;
            int defense = 0;
            int range = 1;
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unit.UnitTypeId, out var type) && type != null)
            {
                if (!string.IsNullOrWhiteSpace(type.DisplayName))
                {
                    name = type.DisplayName;
                }
                attack = type.Attack;
                defense = type.Defense;
                range = type.Range;
            }

            string promos = unit.Promotions != null && unit.Promotions.Count > 0 ? string.Join(", ", unit.Promotions) : "None";
            return $"{name}\nHP {unit.Health}/{unit.MaxHealth}  MP {unit.MovementRemaining}/{unit.MovementPoints}\nAtk {attack}  Def {defense}  Rng {range}\nPromos: {promos}";
        }

        private string BuildCityTooltip(City city)
        {
            if (city == null)
            {
                return string.Empty;
            }

            int foodNeeded = 5 + city.Population * 2;
            int defense = 2 + Mathf.Max(0, city.Population / 2);
            if (city.MaxHealth > 0 && city.Health < city.MaxHealth / 2)
            {
                defense = Mathf.Max(1, defense - 1);
            }

            string production = city.ProductionTargetId;
            if (dataCatalog != null && dataCatalog.TryGetUnitType(city.ProductionTargetId, out var unitType) && !string.IsNullOrWhiteSpace(unitType.DisplayName))
            {
                production = unitType.DisplayName;
            }

            return $"{city.Name}\nPop {city.Population}  HP {city.Health}/{city.MaxHealth}  Def {defense}\nFood {city.FoodStored}/{foodNeeded} (+{city.FoodPerTurn})\nProd {production} {city.ProductionStored}/{city.ProductionCost} (+{city.ProductionPerTurn})";
        }

        private string BuildTileTooltip(GridPosition position)
        {
            if (state?.Map == null)
            {
                return string.Empty;
            }

            var tile = state.Map.GetTile(position.X, position.Y);
            if (tile == null)
            {
                return string.Empty;
            }

            if (!tile.Explored && !tile.Visible)
            {
                return $"Tile ({position.X},{position.Y})\nUnexplored";
            }

            string terrainName = tile.TerrainId;
            int moveCost = 1;
            int defense = 0;
            if (dataCatalog != null && dataCatalog.TryGetTerrainType(tile.TerrainId, out var terrain))
            {
                if (!string.IsNullOrWhiteSpace(terrain.DisplayName))
                {
                    terrainName = terrain.DisplayName;
                }
                moveCost = Mathf.Max(1, terrain.MovementCost);
                defense = terrain.DefenseBonus;
            }

            string improvement = string.Empty;
            if (!string.IsNullOrWhiteSpace(tile.ImprovementId))
            {
                improvement = tile.ImprovementId;
                if (dataCatalog != null && dataCatalog.TryGetImprovementType(tile.ImprovementId, out var imp) && !string.IsNullOrWhiteSpace(imp.DisplayName))
                {
                    improvement = imp.DisplayName;
                }
            }

            string resource = string.Empty;
            if (!string.IsNullOrWhiteSpace(tile.ResourceId))
            {
                resource = tile.ResourceId;
                if (dataCatalog != null && dataCatalog.TryGetResourceType(tile.ResourceId, out var res) && !string.IsNullOrWhiteSpace(res.DisplayName))
                {
                    resource = res.DisplayName;
                }
            }

            string road = tile.HasRoad ? "Yes" : "No";
            string visibility = tile.Visible ? "Visible" : (tile.Explored ? "Explored" : "Unexplored");

            return $"Tile ({position.X},{position.Y})\n{terrainName}  Move {moveCost}  Def {defense}\nImprovement: {(string.IsNullOrWhiteSpace(improvement) ? "None" : improvement)}\nResource: {(string.IsNullOrWhiteSpace(resource) ? "None" : resource)}  Road: {road}\n{visibility}";
        }

        private void UpdateAlertsAndEndTurnState()
        {
            if (hudController == null || state?.ActivePlayer == null)
            {
                return;
            }

            var alerts = BuildAlerts();
            if (alerts.Count == 0)
            {
                hudController.SetAlertInfo(string.Empty);
            }
            else
            {
                hudController.SetAlertInfo("Alerts: " + string.Join(" | ", alerts));
            }

            var blockers = GetEndTurnBlockers();
            string tooltip = blockers.Count == 0 ? "All clear." : "Pending: " + string.Join(", ", blockers) + ". Hold Shift+Enter to force.";
            hudController.SetEndTurnState(blockers.Count > 0, tooltip);
        }

        private List<string> BuildAlerts()
        {
            var alerts = new List<string>();

            if (state?.ActivePlayer == null)
            {
                return alerts;
            }

            var player = state.ActivePlayer;
            if (string.IsNullOrWhiteSpace(player.CurrentTechId))
            {
                alerts.Add("Choose research");
            }
            else if (IsResearchCompletingNextTurn(player))
            {
                alerts.Add("Research completes next turn");
            }

            int idleUnits = 0;
            foreach (var unit in player.Units)
            {
                if (unit.MovementRemaining > 0)
                {
                    idleUnits++;
                }
            }
            if (idleUnits > 0)
            {
                alerts.Add($"Idle units: {idleUnits}");
            }

            foreach (var city in player.Cities)
            {
                if (string.IsNullOrWhiteSpace(city.ProductionTargetId))
                {
                    alerts.Add($"Choose production ({city.Name})");
                    break;
                }
            }

            foreach (var city in player.Cities)
            {
                if (city.ProductionCost > 0 && city.ProductionStored >= city.ProductionCost)
                {
                    alerts.Add($"Production ready ({city.Name})");
                    break;
                }
            }

            foreach (var city in player.Cities)
            {
                int foodNeeded = 5 + city.Population * 2;
                if (city.FoodStored + city.FoodPerTurn >= foodNeeded)
                {
                    alerts.Add($"City growth next turn ({city.Name})");
                    break;
                }
            }

            foreach (var city in player.Cities)
            {
                if (city.ProductionPerTurn > 0 && city.ProductionStored + city.ProductionPerTurn >= city.ProductionCost)
                {
                    alerts.Add($"Production completes next turn ({city.Name})");
                    break;
                }
            }

            var wars = GetWarOpponents(player);
            if (wars.Count > 0)
            {
                alerts.Add("At war: " + string.Join(", ", wars));
            }

            return alerts;
        }

        private List<string> GetEndTurnBlockers()
        {
            var blockers = new List<string>();

            if (state?.ActivePlayer == null)
            {
                return blockers;
            }

            var player = state.ActivePlayer;
            if (string.IsNullOrWhiteSpace(player.CurrentTechId))
            {
                blockers.Add("Choose research");
            }

            bool hasIdleUnits = false;
            foreach (var unit in player.Units)
            {
                if (unit.MovementRemaining > 0)
                {
                    hasIdleUnits = true;
                    break;
                }
            }
            if (hasIdleUnits)
            {
                blockers.Add("Idle units");
            }

            foreach (var city in player.Cities)
            {
                if (string.IsNullOrWhiteSpace(city.ProductionTargetId))
                {
                    blockers.Add($"Production ({city.Name})");
                    break;
                }
            }

            return blockers;
        }

        private List<string> GetWarOpponents(Player player)
        {
            var wars = new List<string>();
            if (player?.Diplomacy == null)
            {
                return wars;
            }

            foreach (var status in player.Diplomacy)
            {
                if (status == null || !status.AtWar)
                {
                    continue;
                }

                var other = GetPlayerById(status.OtherPlayerId);
                if (other == null)
                {
                    continue;
                }

                wars.Add(string.IsNullOrWhiteSpace(other.Name) ? $"Player {other.Id}" : other.Name);
            }

            return wars;
        }

        private bool IsResearchCompletingNextTurn(Player player)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.CurrentTechId))
            {
                return false;
            }

            if (dataCatalog == null || !dataCatalog.TryGetTechType(player.CurrentTechId, out var tech))
            {
                return false;
            }

            int science = GetSciencePerTurn(player);
            return player.ResearchProgress + science >= tech.Cost;
        }

        private int GetSciencePerTurn(Player player)
        {
            if (player == null)
            {
                return 0;
            }

            int science = 1 + player.Cities.Count;
            science += GetCivicScienceBonus(player);
            science += GetResourceScienceBonus(player);
            return science;
        }

        private int GetCivicScienceBonus(Player player)
        {
            if (player?.Civics == null)
            {
                return 0;
            }

            foreach (var civic in player.Civics)
            {
                if (civic != null && civic.CivicId == "republic")
                {
                    return player.Cities.Count;
                }
            }

            return 0;
        }

        private int GetResourceScienceBonus(Player player)
        {
            if (player?.AvailableResources == null || dataCatalog == null)
            {
                return 0;
            }

            int bonus = 0;
            foreach (var resourceId in player.AvailableResources)
            {
                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    continue;
                }

                if (dataCatalog.TryGetResourceType(resourceId, out var resource))
                {
                    bonus += resource.ScienceBonus;
                }
            }

            return bonus;
        }

        private void HandleAutoEndTurn()
        {
            if (!autoEndTurnWhenReady || hudController == null || state?.ActivePlayer == null)
            {
                autoEndTurnTimer = 0f;
                return;
            }

            if (promotionSelectionOpen || techSelectionOpen || techTreeOpen || civicSelectionOpen || diplomacySelectionOpen)
            {
                autoEndTurnTimer = 0f;
                return;
            }

            var blockers = GetEndTurnBlockers();
            if (blockers.Count > 0)
            {
                autoEndTurnTimer = 0f;
                return;
            }

            autoEndTurnTimer += Time.unscaledDeltaTime;
            if (autoEndTurnTimer >= autoEndTurnDelay)
            {
                autoEndTurnTimer = 0f;
                EndTurn(false);
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
                hudController.SetUnitPanel("Name: None", "HP: -", "Movement: -", "Strength: -");
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

            string unitName = selectedUnit.UnitTypeId;
            string strengthText = "Strength: -";
            if (dataCatalog != null && dataCatalog.TryGetUnitType(selectedUnit.UnitTypeId, out var unitType) && unitType != null)
            {
                if (!string.IsNullOrWhiteSpace(unitType.DisplayName))
                {
                    unitName = unitType.DisplayName;
                }
                strengthText = $"Strength: {unitType.Attack}/{unitType.Defense}";
            }

            hudController.SetUnitPanel(
                $"Name: {unitName}",
                $"HP: {selectedUnit.Health}/{selectedUnit.MaxHealth}",
                $"Movement: {selectedUnit.MovementRemaining}/{selectedUnit.MovementPoints}",
                strengthText
            );
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
            EndTurn(false);
        }

        private void EndTurn(bool force)
        {
            if (turnSystem == null)
            {
                return;
            }

            var blockers = GetEndTurnBlockers();
            if (!force && blockers.Count > 0)
            {
                hudController?.SetEventMessage("Pending actions: " + string.Join(", ", blockers));
                UpdateAlertsAndEndTurnState();
                return;
            }

            UpdateCitySiegeStates();
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
            UpdateDiplomacyInfo();
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
            UpdateTopBarInfo();
        }
    }
}
