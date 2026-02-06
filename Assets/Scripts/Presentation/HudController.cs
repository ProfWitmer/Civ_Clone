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
        private const string EndTurnButtonName = "endturn-button";
        private const string CityLabelName = "city-label";
        private const string ResearchLabelName = "research-label";

        private GameState state;
        private TurnSystem turnSystem;

        private Label turnLabel;
        private Label selectionLabel;
        private Label cityLabel;
        private Label researchLabel;
        private Button endTurnButton;

        private System.Action onEndTurn;

        private void Awake()
        {
            var document = GetComponent<UIDocument>();
            VisualElement root = document.rootVisualElement;

            turnLabel = root.Q<Label>(TurnLabelName);
            selectionLabel = root.Q<Label>(SelectionLabelName);
            cityLabel = root.Q<Label>(CityLabelName);
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

        public void SetCityInfo(string cityInfo)
        {
            if (cityLabel != null)
            {
                cityLabel.text = cityInfo;
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

            if (cityLabel != null && string.IsNullOrEmpty(cityLabel.text))
            {
                cityLabel.text = "City: None";
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
