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

        private GameState _state;
        private TurnSystem _turnSystem;

        private Label _turnLabel;
        private Label _selectionLabel;
        private Button _endTurnButton;

        private void Awake()
        {
            var document = GetComponent<UIDocument>();
            VisualElement root = document.rootVisualElement;

            _turnLabel = root.Q<Label>(TurnLabelName);
            _selectionLabel = root.Q<Label>(SelectionLabelName);
            _endTurnButton = root.Q<Button>(EndTurnButtonName);

            if (_endTurnButton != null)
            {
                _endTurnButton.clicked += HandleEndTurn;
            }
        }

        public void Bind(GameState state, TurnSystem turnSystem)
        {
            _state = state;
            _turnSystem = turnSystem;
            Refresh();
        }

        public void SetSelection(string selection)
        {
            if (_selectionLabel != null)
            {
                _selectionLabel.text = selection;
            }
        }

        private void HandleEndTurn()
        {
            if (_turnSystem == null)
            {
                return;
            }

            _turnSystem.EndTurn();
            Refresh();
        }

        private void Refresh()
        {
            if (_state == null)
            {
                return;
            }

            if (_turnLabel != null)
            {
                string playerName = _state.ActivePlayer != null ? _state.ActivePlayer.Name : "-";
                _turnLabel.text = $"Turn {_state.CurrentTurn} - {playerName}";
            }

            if (_selectionLabel != null && string.IsNullOrEmpty(_selectionLabel.text))
            {
                _selectionLabel.text = "Selection: None";
            }
        }
    }
}
