using CivClone.Simulation.Core;
using UnityEngine;

namespace CivClone.Presentation.UI
{
    public sealed class TurnAdvanceController : MonoBehaviour
    {
        [SerializeField] private MiniInfoPanel infoPanel;
        [SerializeField] private KeyCode advanceKey = KeyCode.Space;

        private TurnSystem turnSystem;

        private void Awake()
        {
            turnSystem = new TurnSystem();
            UpdateUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(advanceKey))
            {
                turnSystem.AdvanceTurn();
                UpdateUI();
            }
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
