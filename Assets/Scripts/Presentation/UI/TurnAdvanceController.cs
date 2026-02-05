using CivClone.Simulation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CivClone.Presentation.UI
{
    public sealed class TurnAdvanceController : MonoBehaviour
    {
        [SerializeField] private MiniInfoPanel infoPanel;
        [SerializeField] private KeyCode advanceKey = KeyCode.Space;
        [SerializeField] private Button advanceButton;

        private TurnSystem turnSystem;

        private void Awake()
        {
            turnSystem = new TurnSystem();
            if (advanceButton != null)
            {
                advanceButton.onClick.AddListener(AdvanceTurn);
            }
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (advanceButton != null)
            {
                advanceButton.onClick.RemoveListener(AdvanceTurn);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(advanceKey))
            {
                AdvanceTurn();
            }
        }

        public void AdvanceTurn()
        {
            turnSystem.AdvanceTurn();
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (infoPanel == null)
            {
                return;
            }

            infoPanel.SetTurn(turnSystem.CurrentTurn);
            infoPanel.SetYear(turnSystem.CurrentYearLabel);
        }
    }
}
