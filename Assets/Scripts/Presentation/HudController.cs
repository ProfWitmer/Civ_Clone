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
        private const string PromotionDetailLabelName = "promotion-detail-label";
        private const string CombatLogLabelName = "combat-log-label";
        private const string TechPanelName = "tech-panel";
        private const string TechOption1Name = "tech-option1";
        private const string TechOption2Name = "tech-option2";
        private const string TechOption3Name = "tech-option3";
        private const string PromotionPanelName = "promotion-panel";
        private const string PromotionOption1Name = "promotion-option1";
        private const string PromotionOption2Name = "promotion-option2";
        private const string PromotionOption3Name = "promotion-option3";
        private const string ResearchLabelName = "research-label";

        private GameState state;
        private TurnSystem turnSystem;

        private Label turnLabel;
        private Label selectionLabel;
        private Label eventLabel;
        private Label cityLabel;
        private Label productionLabel;
        private Label promotionLabel;
        private Label promotionDetailLabel;
        private Label combatLogLabel;
        private VisualElement techPanel;
        private Label techOption1;
        private Label techOption2;
        private Label techOption3;
        private VisualElement promotionPanel;
        private Label promotionOption1;
        private Label promotionOption2;
        private Label promotionOption3;
        private Label researchLabel;
        private Button endTurnButton;

        private System.Action onEndTurn;
        private Coroutine eventClearRoutine;
        private readonly System.Collections.Generic.List<string> combatLog = new System.Collections.Generic.List<string>();
        private const int CombatLogMax = 6;

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
            promotionDetailLabel = root.Q<Label>(PromotionDetailLabelName);
            combatLogLabel = root.Q<Label>(CombatLogLabelName);
            techPanel = root.Q<VisualElement>(TechPanelName);
            techOption1 = root.Q<Label>(TechOption1Name);
            techOption2 = root.Q<Label>(TechOption2Name);
            techOption3 = root.Q<Label>(TechOption3Name);
            promotionPanel = root.Q<VisualElement>(PromotionPanelName);
            promotionOption1 = root.Q<Label>(PromotionOption1Name);
            promotionOption2 = root.Q<Label>(PromotionOption2Name);
            promotionOption3 = root.Q<Label>(PromotionOption3Name);
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

        public void SetPromotionDetail(string detail)
        {
            if (promotionDetailLabel != null)
            {
                promotionDetailLabel.text = detail;
            }
        }

        public void LogCombat(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                return;
            }

            combatLog.Add(entry);
            if (combatLog.Count > CombatLogMax)
            {
                combatLog.RemoveAt(0);
            }

            if (combatLogLabel != null)
            {
                combatLogLabel.text = string.Join("\n", combatLog);
            }
        }

        public void ShowTechPanel(string option1, string option2, string option3)
        {
            if (techPanel == null)
            {
                return;
            }

            techPanel.style.display = DisplayStyle.Flex;
            if (techOption1 != null) techOption1.text = option1;
            if (techOption2 != null) techOption2.text = option2;
            if (techOption3 != null) techOption3.text = option3;
        }

        public void HideTechPanel()
        {
            if (techPanel == null)
            {
                return;
            }

            techPanel.style.display = DisplayStyle.None;
        }

        public void ShowPromotionPanel(string option1, string option2, string option3)
        {
            if (promotionPanel == null)
            {
                return;
            }

            promotionPanel.style.display = DisplayStyle.Flex;
            if (promotionOption1 != null) promotionOption1.text = option1;
            if (promotionOption2 != null) promotionOption2.text = option2;
            if (promotionOption3 != null) promotionOption3.text = option3;
        }

        public void HidePromotionPanel()
        {
            if (promotionPanel == null)
            {
                return;
            }

            promotionPanel.style.display = DisplayStyle.None;
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

            if (promotionDetailLabel != null && string.IsNullOrEmpty(promotionDetailLabel.text))
            {
                promotionDetailLabel.text = string.Empty;
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
