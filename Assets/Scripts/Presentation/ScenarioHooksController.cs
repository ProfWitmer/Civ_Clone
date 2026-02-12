using System.Collections.Generic;
using CivClone.Infrastructure;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class ScenarioHooksController : MonoBehaviour
    {
        [SerializeField] private bool enableScenarioHooks = true;

        private ScenarioDefinition scenario;
        private GameState state;
        private TurnSystem turnSystem;
        private GameDataCatalog dataCatalog;
        private HudController hudController;
        private UnitPresenter unitPresenter;
        private CityPresenter cityPresenter;
        private MapPresenter mapPresenter;

        private int lastTurn = -1;
        private readonly HashSet<string> firedEvents = new HashSet<string>();
        private bool victoryTriggered;

        public void Bind(ScenarioDefinition scenarioDefinition, GameState gameState, TurnSystem turnSystemRef, GameDataCatalog catalog, HudController hud)
        {
            scenario = scenarioDefinition;
            state = gameState;
            turnSystem = turnSystemRef;
            dataCatalog = catalog;
            hudController = hud;
            lastTurn = -1;
            firedEvents.Clear();
            victoryTriggered = false;
        }

        private void Update()
        {
            if (!enableScenarioHooks || scenario == null || state == null)
            {
                return;
            }

            if (state.CurrentTurn != lastTurn)
            {
                lastTurn = state.CurrentTurn;
                ApplyTurnEvents(state.CurrentTurn);
            }

            CheckVictoryConditions();
        }

        private void ApplyTurnEvents(int turn)
        {
            if (scenario.Events == null || scenario.Events.Count == 0)
            {
                return;
            }

            foreach (var evt in scenario.Events)
            {
                if (evt == null || evt.Turn != turn)
                {
                    continue;
                }

                string eventId = string.IsNullOrWhiteSpace(evt.Id) ? $"{evt.Type}:{evt.TargetPlayerId}:{evt.Turn}:{evt.UnitTypeId}:{evt.TechId}:{evt.ResourceId}:{evt.X}:{evt.Y}:{evt.OtherPlayerId}" : evt.Id;
                if (firedEvents.Contains(eventId))
                {
                    continue;
                }

                firedEvents.Add(eventId);
                ApplyEvent(evt);
            }
        }

        private void ApplyEvent(ScenarioEventDefinition evt)
        {
            switch (evt.Type)
            {
                case "grant_tech":
                    GrantTech(evt.TargetPlayerId, evt.TechId);
                    break;
                case "spawn_unit":
                    SpawnUnit(evt.TargetPlayerId, evt.UnitTypeId, evt.X, evt.Y);
                    break;
                case "give_resource":
                    GiveResource(evt.TargetPlayerId, evt.ResourceId);
                    break;
                case "declare_war":
                    SetWarStatus(evt.TargetPlayerId, evt.OtherPlayerId, true);
                    break;
                case "make_peace":
                    SetWarStatus(evt.TargetPlayerId, evt.OtherPlayerId, false);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(evt.Message))
            {
                hudController?.SetEventMessage(evt.Message, 4f);
            }
        }

        private void GrantTech(int playerId, string techId)
        {
            if (string.IsNullOrWhiteSpace(techId))
            {
                return;
            }

            var player = GetPlayerById(playerId);
            if (player == null)
            {
                return;
            }

            if (!player.KnownTechs.Contains(techId))
            {
                player.KnownTechs.Add(techId);
            }

            if (player.CurrentTechId == techId)
            {
                player.CurrentTechId = string.Empty;
                player.ResearchProgress = 0;
            }
        }

        private void SpawnUnit(int playerId, string unitTypeId, int x, int y)
        {
            if (string.IsNullOrWhiteSpace(unitTypeId))
            {
                return;
            }

            var player = GetPlayerById(playerId);
            if (player == null)
            {
                return;
            }

            int movement = 2;
            if (dataCatalog != null && dataCatalog.TryGetUnitType(unitTypeId, out var unitType))
            {
                movement = Mathf.Max(1, unitType.MovementPoints);
            }

            var unit = new Unit(unitTypeId, new GridPosition(x, y), movement, player.Id);
            player.Units.Add(unit);
            EnsurePresenters();
            unitPresenter?.RenderUnits(state, mapPresenter);
        }

        private void GiveResource(int playerId, string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return;
            }

            var player = GetPlayerById(playerId);
            if (player == null)
            {
                return;
            }

            if (!player.AvailableResources.Contains(resourceId))
            {
                player.AvailableResources.Add(resourceId);
            }
        }

        private void SetWarStatus(int playerId, int otherId, bool atWar)
        {
            var player = GetPlayerById(playerId);
            var other = GetPlayerById(otherId);
            if (player == null || other == null)
            {
                return;
            }

            EnsureDiplomacyEntry(player, otherId, atWar);
            EnsureDiplomacyEntry(other, playerId, atWar);
        }

        private void EnsureDiplomacyEntry(Player player, int otherId, bool atWar)
        {
            if (player.Diplomacy == null)
            {
                player.Diplomacy = new List<DiplomacyStatus>();
            }

            foreach (var status in player.Diplomacy)
            {
                if (status != null && status.OtherPlayerId == otherId)
                {
                    status.AtWar = atWar;
                    return;
                }
            }

            player.Diplomacy.Add(new DiplomacyStatus
            {
                OtherPlayerId = otherId,
                AtWar = atWar
            });
        }

        private void CheckVictoryConditions()
        {
            if (victoryTriggered || scenario?.Victory == null || state?.Players == null)
            {
                return;
            }

            if (scenario.Victory.TurnLimit > 0 && state.CurrentTurn > scenario.Victory.TurnLimit)
            {
                victoryTriggered = true;
                hudController?.SetEventMessage("Scenario complete: Turn limit reached.", 6f);
                return;
            }

            if (scenario.Victory.EliminateAllOpponents)
            {
                var player = state.ActivePlayer;
                if (player == null)
                {
                    return;
                }

                bool anyOpponents = false;
                foreach (var other in state.Players)
                {
                    if (other == null || other.Id == player.Id)
                    {
                        continue;
                    }

                    if ((other.Units != null && other.Units.Count > 0) || (other.Cities != null && other.Cities.Count > 0))
                    {
                        anyOpponents = true;
                        break;
                    }
                }

                if (!anyOpponents)
                {
                    victoryTriggered = true;
                    hudController?.SetEventMessage("Scenario complete: All opponents defeated.", 6f);
                }
            }
        }

        private Player GetPlayerById(int id)
        {
            if (state?.Players == null)
            {
                return null;
            }

            foreach (var player in state.Players)
            {
                if (player != null && player.Id == id)
                {
                    return player;
                }
            }

            return null;
        }

        private void EnsurePresenters()
        {
            if (unitPresenter == null)
            {
                unitPresenter = FindFirstObjectByType<UnitPresenter>();
            }

            if (cityPresenter == null)
            {
                cityPresenter = FindFirstObjectByType<CityPresenter>();
            }

            if (mapPresenter == null)
            {
                mapPresenter = FindFirstObjectByType<MapPresenter>();
            }
        }
    }
}
