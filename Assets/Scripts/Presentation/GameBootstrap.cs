using System;
using System.IO;
using CivClone.Infrastructure;
using CivClone.Infrastructure.Data;
using CivClone.Presentation.Camera;
using CivClone.Presentation.Map;
using CivClone.Presentation.UI;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private MapConfig mapConfig = new MapConfig();
        [SerializeField] private GameDataCatalog dataCatalog;

        [Header("Scene References")]
        [SerializeField] private MapPresenter mapPresenter;
        [SerializeField] private UnitPresenter unitPresenter;
        [SerializeField] private CityPresenter cityPresenter;
        [SerializeField] private MapInputController inputController;
        [SerializeField] private SaveLoadController saveLoadController;
        [SerializeField] private MiniMapPresenter miniMapPresenter;
        [SerializeField] private HudController hudController;

        private GameState _state;
        private TurnSystem _turnSystem;
        private FogOfWarSystem _fogOfWar;
        private ScenarioDefinition _scenario;

        public GameState State => _state;

        private void Awake()
        {
            if (dataCatalog != null)
            {
                var catalogLoader = new GameDataCatalogLoader();
                catalogLoader.TryLoadFromResources(dataCatalog);
            }

            var initialState = BuildInitialState();
            ApplyState(initialState);

            if (saveLoadController == null)
            {
                saveLoadController = GetComponent<SaveLoadController>();
                if (saveLoadController == null)
                {
                    saveLoadController = gameObject.AddComponent<SaveLoadController>();
                }
            }

            if (saveLoadController != null)
            {
                saveLoadController.Bind(this);
            }

            EnsureCameraController();
            EnsureTileHighlighter();
        }

        public void ApplyState(GameState state)
        {
            if (state == null)
            {
                return;
            }

            _state = state;
            _turnSystem = new TurnSystem(_state, dataCatalog);
            _fogOfWar = new FogOfWarSystem();
            _fogOfWar.Apply(_state);

            if (mapPresenter != null)
            {
                mapPresenter.Render(_state.Map, dataCatalog);
            }

            if (unitPresenter != null && mapPresenter != null)
            {
                unitPresenter.RenderUnits(_state, mapPresenter);
            }

            if (cityPresenter == null)
            {
                cityPresenter = GetComponent<CityPresenter>();
                if (cityPresenter == null)
                {
                    cityPresenter = gameObject.AddComponent<CityPresenter>();
                }
            }

            if (cityPresenter != null && mapPresenter != null)
            {
                cityPresenter.RenderCities(_state, mapPresenter);
            }

            if (hudController != null)
            {
                hudController.Bind(_state, _turnSystem);
            }

            if (inputController != null)
            {
                inputController.Bind(_state, _turnSystem, _fogOfWar, dataCatalog, mapPresenter, unitPresenter, cityPresenter, hudController, UnityEngine.Camera.main);
            }

            if (hudController != null && inputController != null)
            {
                hudController.SetEndTurnHandler(inputController.RequestEndTurn);
            }

            if (miniMapPresenter == null)
            {
                miniMapPresenter = FindFirstObjectByType<MiniMapPresenter>();
                if (miniMapPresenter == null && hudController != null)
                {
                    miniMapPresenter = hudController.gameObject.AddComponent<MiniMapPresenter>();
                }
                else if (miniMapPresenter == null)
                {
                    miniMapPresenter = gameObject.AddComponent<MiniMapPresenter>();
                }
            }

            if (miniMapPresenter != null)
            {
                miniMapPresenter.Bind(_state, dataCatalog);
            }

            if (mapPresenter != null)
            {
                mapPresenter.UpdateFog(_state.Map);
                mapPresenter.UpdateImprovements(_state.Map, dataCatalog);
            }

            EnsureScenarioHooks();
            EnsureScenarioSelectionUI();
        }

        private void EnsureCameraController()
        {
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            if (!mainCamera.TryGetComponent(out IsometricOrthoCameraController controller))
            {
                controller = mainCamera.gameObject.AddComponent<IsometricOrthoCameraController>();
            }

            controller.Bind(mapPresenter);
        }

        private void EnsureTileHighlighter()
        {
            if (mapPresenter == null)
            {
                return;
            }

            if (!mapPresenter.TryGetComponent(out IsometricTileHighlighter highlighter))
            {
                highlighter = mapPresenter.gameObject.AddComponent<IsometricTileHighlighter>();
            }

            highlighter.Bind(mapPresenter, UnityEngine.Camera.main);
        }

        private GameState BuildInitialState()
        {
            var scenario = LoadScenarioDefinition();
            _scenario = scenario;
            if (scenario?.Map != null)
            {
                mapConfig.Width = scenario.Map.Width;
                mapConfig.Height = scenario.Map.Height;
                mapConfig.Seed = scenario.Map.Seed;
                if (!string.IsNullOrWhiteSpace(scenario.Map.DefaultTerrainId))
                {
                    mapConfig.DefaultTerrainId = scenario.Map.DefaultTerrainId;
                }
            }

            var generator = new MapGenerator(mapConfig);
            var map = generator.Generate();

            var state = new GameState
            {
                Map = map
            };

            if (scenario != null && scenario.Players != null && scenario.Players.Count > 0)
            {
                state.ScenarioId = LoadScenarioId();
                foreach (var scenarioPlayer in scenario.Players)
                {
                    if (scenarioPlayer == null)
                    {
                        continue;
                    }

                    var player = new Player(scenarioPlayer.Id, string.IsNullOrWhiteSpace(scenarioPlayer.Name) ? $"Player {scenarioPlayer.Id}" : scenarioPlayer.Name);
                    if (scenarioPlayer.StartingTechs != null)
                    {
                        player.KnownTechs.AddRange(scenarioPlayer.StartingTechs);
                    }

                    if (!string.IsNullOrWhiteSpace(scenarioPlayer.CurrentTechId))
                    {
                        player.CurrentTechId = scenarioPlayer.CurrentTechId;
                    }

                    if (scenarioPlayer.Cities != null)
                    {
                        foreach (var cityDef in scenarioPlayer.Cities)
                        {
                            if (cityDef == null)
                            {
                                continue;
                            }

                            int population = Mathf.Max(1, cityDef.Population);
                            var city = new City(string.IsNullOrWhiteSpace(cityDef.Name) ? $"City {player.Cities.Count + 1}" : cityDef.Name,
                                new GridPosition(cityDef.X, cityDef.Y),
                                player.Id,
                                population);

                            string target = GetDefaultProductionTarget();
                            city.ProductionTargetId = target;
                            city.ProductionCost = GetProductionCost(target);
                            player.Cities.Add(city);
                        }
                    }

                    if (scenarioPlayer.Units != null)
                    {
                        foreach (var unitDef in scenarioPlayer.Units)
                        {
                            if (unitDef == null || string.IsNullOrWhiteSpace(unitDef.UnitTypeId))
                            {
                                continue;
                            }

                            int movement = unitDef.MovementPoints;
                            if (movement <= 0 && dataCatalog != null && dataCatalog.TryGetUnitType(unitDef.UnitTypeId, out var unitType))
                            {
                                movement = unitType.MovementPoints;
                            }
                            movement = Mathf.Max(1, movement);

                            var unit = new Unit(unitDef.UnitTypeId, new GridPosition(unitDef.X, unitDef.Y), movement, player.Id);
                            player.Units.Add(unit);
                        }
                    }

                    state.Players.Add(player);
                }

                state.ActivePlayerIndex = Mathf.Clamp(scenario.ActivePlayerIndex, 0, Mathf.Max(0, state.Players.Count - 1));
            }
            else
            {
                var player = new Player(0, "Player 1");
                player.Units.Add(new Unit("scout", new GridPosition(2, 2), 2, player.Id));
                player.Units.Add(new Unit("settler", new GridPosition(3, 2), 2, player.Id));
                state.Players.Add(player);

                var rival = new Player(1, "Rival");
                rival.Units.Add(new Unit("scout", new GridPosition(6, 2), 2, rival.Id));
                state.Players.Add(rival);
            }

            InitializeDiplomacy(state);

            return state;
        }

        private void EnsureScenarioHooks()
        {
            if (_scenario == null)
            {
                return;
            }

            if (!TryGetComponent(out ScenarioHooksController hooks))
            {
                hooks = gameObject.AddComponent<ScenarioHooksController>();
            }

            hooks.Bind(_scenario, _state, _turnSystem, dataCatalog, hudController);
        }

        private void EnsureScenarioSelectionUI()
        {
            if (hudController == null)
            {
                return;
            }

            if (!hudController.TryGetComponent(out UI.ScenarioSelectionController selector))
            {
                selector = hudController.gameObject.AddComponent<UI.ScenarioSelectionController>();
            }

            selector.Refresh();
        }

        private ScenarioDefinition LoadScenarioDefinition()
        {
            var loader = new DataLoader();
            string selectorJson = LoadScenarioSelectorJson();
            if (string.IsNullOrWhiteSpace(selectorJson))
            {
                return null;
            }

            try
            {
                var selector = JsonUtility.FromJson<ScenarioSelector>(selectorJson);
                if (selector != null && !string.IsNullOrWhiteSpace(selector.ScenarioId))
                {
                    var catalog = LoadScenarioCatalog(loader);
                    var entry = FindScenarioEntry(catalog, selector.ScenarioId);
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.Path))
                    {
                        string scenarioJson = loader.LoadText(entry.Path);
                        if (!string.IsNullOrWhiteSpace(scenarioJson))
                        {
                            return JsonUtility.FromJson<ScenarioDefinition>(scenarioJson);
                        }
                    }
                }

                return JsonUtility.FromJson<ScenarioDefinition>(selectorJson);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load scenario.json: {ex.Message}");
                return null;
            }
        }

        private string LoadScenarioSelectorJson()
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, "Data/scenario.json");
            if (File.Exists(persistentPath))
            {
                return File.ReadAllText(persistentPath);
            }

            var loader = new DataLoader();
            return loader.LoadText("Data/scenario.json");
        }

        private string LoadScenarioId()
        {
            try
            {
                var selectorJson = LoadScenarioSelectorJson();
                if (string.IsNullOrWhiteSpace(selectorJson))
                {
                    return string.Empty;
                }

                var selector = JsonUtility.FromJson<ScenarioSelector>(selectorJson);
                return selector?.ScenarioId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private ScenarioCatalog LoadScenarioCatalog(DataLoader loader)
        {
            if (loader == null)
            {
                return null;
            }

            string json = loader.LoadText("Data/scenario_catalog.json");
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<ScenarioCatalog>(json);
            }
            catch
            {
                return null;
            }
        }

        private ScenarioCatalogEntry FindScenarioEntry(ScenarioCatalog catalog, string id)
        {
            if (catalog?.Items == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            foreach (var entry in catalog.Items)
            {
                if (entry != null && entry.Id == id)
                {
                    return entry;
                }
            }

            return null;
        }

        private string GetDefaultProductionTarget()
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType("warrior", out _))
            {
                return "warrior";
            }

            return "scout";
        }

        private int GetProductionCost(string productionId)
        {
            if (dataCatalog != null && dataCatalog.TryGetUnitType(productionId, out var unitType))
            {
                return unitType.ProductionCost;
            }

            if (dataCatalog != null && dataCatalog.TryGetBuildingType(productionId, out var buildingType))
            {
                return buildingType.ProductionCost;
            }

            return 10;
        }

        private void InitializeDiplomacy(GameState state)
        {
            if (state?.Players == null)
            {
                return;
            }

            for (int i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                if (player == null)
                {
                    continue;
                }

                if (player.Diplomacy == null)
                {
                    player.Diplomacy = new System.Collections.Generic.List<DiplomacyStatus>();
                }

                for (int j = 0; j < state.Players.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    int otherId = state.Players[j].Id;
                    bool exists = false;
                    foreach (var status in player.Diplomacy)
                    {
                        if (status != null && status.OtherPlayerId == otherId)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        player.Diplomacy.Add(new DiplomacyStatus
                        {
                            OtherPlayerId = otherId,
                            AtWar = false
                        });
                    }
                }
            }
        }
    }
}
