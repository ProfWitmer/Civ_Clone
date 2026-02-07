using CivClone.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace CivClone.Presentation
{
    [RequireComponent(typeof(UIDocument))]
    public class HudController : MonoBehaviour
    {
        private const string TurnLabelName = "turn-label";
        private const string SelectionLabelName = "selection-label";
        private const string EventLabelName = "event-label";
        private const string EndTurnButtonName = "endturn-button";
        private const string CityLabelName = "city-label";
        private const string ProductionLabelName = "production-label";
        private const string PromotionLabelName = "promotion-label";
        private const string ResearchLabelName = "research-label";

        private GameState state;
        private TurnSystem turnSystem;

        private Label turnLabel;
        private Label selectionLabel;
        private Label eventLabel;
        private Label cityLabel;
        private Label productionLabel;
        private Label promotionLabel;
        private Label researchLabel;
        private Button endTurnButton;

        private System.Action onEndTurn;
        private Coroutine eventClearRoutine;

        private void Awake()
        {
            var document = GetComponent<UIDocument>();
            VisualElement root = document.rootVisualElement;

            turnLabel = root.Q<Label>(TurnLabelName);
            selectionLabel = root.Q<Label>(SelectionLabelName);
            eventLabel = root.Q<Label>(EventLabelName);
            cityLabel = root.Q<Label>(CityLabelName);
            productionLabel = root.Q<Label>(ProductionLabelName);
            promotionLabel = root.Q<Label>(PromotionLabelName);
            researchLabel = root.Q<Label>(ResearchLabelName);
            endTurnButton = root.Q<Button>(EndTurnButtonName);

            if (endTurnButton != null)
            {
                endTurnButton.clicked += HandleEndTurn;
            }
        }

        public void Bind(GameState stateRef, TurnSystem turnSystemRef)
        {
            state = stateRef;
            turnSystem = turnSystemRef;
            Refresh();
        }

        public void SetEndTurnHandler(System.Action handler)
        {
            onEndTurn = handler;
        }

        public void SetSelection(string selection)
        {
            if (selectionLabel != null)
            {
                selectionLabel.text = selection;
            }
        }

        public void SetEventMessage(string message, float duration = 2.5f)
        {
            if (eventLabel == null)
            {
                return;
            }

            eventLabel.text = message ?? string.Empty;
            if (eventClearRoutine != null)
            {
                StopCoroutine(eventClearRoutine);
            }
            eventClearRoutine = StartCoroutine(ClearEventAfter(duration));
        }

        private System.Collections.IEnumerator ClearEventAfter(float duration)
        {
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }
            if (eventLabel != null)
            {
                eventLabel.text = string.Empty;
            }
            eventClearRoutine = null;
        }

        public void SetCityInfo(string cityInfo)
        {
            if (cityLabel != null)
            {
                cityLabel.text = cityInfo;
            }
        }

        public void SetProductionInfo(string productionInfo)
        {
            if (productionLabel != null)
            {
                productionLabel.text = productionInfo;
            }
        }

        public void SetPromotionInfo(string promotionInfo)
        {
            if (promotionLabel != null)
            {
                promotionLabel.text = promotionInfo;
            }
        }

        public void SetResearchInfo(string researchInfo)
        {
            if (researchLabel != null)
            {
                researchLabel.text = researchInfo;
            }
        }

        public void Refresh()
        {
            UpdateTurnLabel();

            if (selectionLabel != null && string.IsNullOrEmpty(selectionLabel.text))
            {
                selectionLabel.text = "Selection: None";
            }
            if (eventLabel != null && string.IsNullOrEmpty(eventLabel.text))
            {
                eventLabel.text = string.Empty;
            }

            if (cityLabel != null && string.IsNullOrEmpty(cityLabel.text))
            {
                cityLabel.text = "City: None";
            }

            if (productionLabel != null && string.IsNullOrEmpty(productionLabel.text))
            {
                productionLabel.text = "Production: None";
            }

            if (promotionLabel != null && string.IsNullOrEmpty(promotionLabel.text))
            {
                promotionLabel.text = "Promotions: None";
            }

            if (researchLabel != null && string.IsNullOrEmpty(researchLabel.text))
            {
                researchLabel.text = "Research: None";
            }
        }

        private void UpdateTurnLabel()
        {
            if (state == null)
            {
                return;
            }

            if (turnLabel != null)
            {
                string playerName = state.ActivePlayer != null ? state.ActivePlayer.Name : "-";
                turnLabel.text = $"Turn {state.CurrentTurn} - {playerName}";
            }
        }

        private void HandleEndTurn()
        {
            if (onEndTurn != null)
            {
                onEndTurn.Invoke();
                return;
            }

            if (turnSystem == null)
            {
                return;
            }

            turnSystem.EndTurn();
            Refresh();
        }
    }
}
