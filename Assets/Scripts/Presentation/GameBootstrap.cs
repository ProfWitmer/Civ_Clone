using CivClone.Infrastructure;
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
        [SerializeField] private MapInputController inputController;
        [SerializeField] private HudController hudController;

        private GameState _state;
        private TurnSystem _turnSystem;

        private void Awake()
        {
            _state = BuildInitialState();
            _turnSystem = new TurnSystem(_state);

            if (mapPresenter != null)
            {
                mapPresenter.Render(_state.Map, dataCatalog);
            }

            if (unitPresenter != null && mapPresenter != null)
            {
                unitPresenter.RenderUnits(_state, mapPresenter);
            }

            if (hudController != null)
            {
                hudController.Bind(_state, _turnSystem);
            }

            if (inputController != null)
            {
                inputController.Bind(_state, _turnSystem, mapPresenter, unitPresenter, hudController, Camera.main);
            }
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
            state.Players.Add(player);

            return state;
        }
    }
}
