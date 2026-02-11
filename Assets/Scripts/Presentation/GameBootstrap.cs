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
            var generator = new MapGenerator(mapConfig);
            var map = generator.Generate();

            var state = new GameState
            {
                Map = map
            };

            var player = new Player(0, "Player 1");
            player.Units.Add(new Unit("scout", new GridPosition(2, 2), 2, player.Id));
            player.Units.Add(new Unit("settler", new GridPosition(3, 2), 2, player.Id));
            state.Players.Add(player);

            var rival = new Player(1, "Rival");
            rival.Units.Add(new Unit("scout", new GridPosition(6, 2), 2, rival.Id));
            state.Players.Add(rival);

            InitializeDiplomacy(state);

            return state;
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
