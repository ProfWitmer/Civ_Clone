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
        private const string TooltipPanelName = "tooltip-panel";
        private const string TooltipLabelName = "tooltip-label";
        private const string ResourceGoldLabelName = "resource-gold-label";
        private const string ResourceScienceLabelName = "resource-science-label";
        private const string ResourceCultureLabelName = "resource-culture-label";
        private const string ResourceFoodLabelName = "resource-food-label";
        private const string ResourceProductionLabelName = "resource-production-label";
        private const string UnitPanelName = "unit-panel";
        private const string UnitNameLabelName = "unit-name-label";
        private const string UnitHpLabelName = "unit-hp-label";
        private const string UnitMoveLabelName = "unit-move-label";
        private const string UnitStrengthLabelName = "unit-strength-label";
        private const string CityPanelName = "city-panel";
        private const string CityNameLabelName = "city-name-label";
        private const string CityPopLabelName = "city-pop-label";
        private const string CityGrowthLabelName = "city-growth-label";
        private const string CityProductionLabelName = "city-production-label";
        private const string CityDefenseLabelName = "city-defense-label";
        private const string CityLabelName = "city-label";
        private const string CityHoverLabelName = "city-hover-label";
        private const string UnitHoverLabelName = "unit-hover-label";
        private const string AlertLabelName = "alert-label";
        private const string ProductionLabelName = "production-label";
        private const string PromotionLabelName = "promotion-label";
        private const string PromotionDetailLabelName = "promotion-detail-label";
        private const string CombatLogLabelName = "combat-log-label";
        private const string TechPanelName = "tech-panel";
        private const string TechOption1Name = "tech-option1";
        private const string TechOption2Name = "tech-option2";
        private const string TechOption3Name = "tech-option3";
        private const string TechPanelTitleName = "tech-panel-title";
        private const string TechTreePanelName = "tech-tree-panel";
        private const string TechTreeTitleName = "tech-tree-title";
        private const string TechTreeLabelName = "tech-tree-label";
        private const string PromotionPanelName = "promotion-panel";
        private const string PromotionPanelTitleName = "promotion-panel-title";
        private const string PromotionOption1Name = "promotion-option1";
        private const string PromotionOption2Name = "promotion-option2";
        private const string PromotionOption3Name = "promotion-option3";
        private const string ResearchLabelName = "research-label";
        private const string CivicLabelName = "civic-label";
        private const string ResourceLabelName = "resource-label";
        private const string ResourceUnitLabelName = "resource-unit-label";
        private const string TradeLabelName = "trade-label";
        private const string DiplomacyStatusLabelName = "diplomacy-status-label";
        private const string CivicPanelName = "civic-panel";
        private const string CivicPanelTitleName = "civic-panel-title";
        private const string CivicOption1Name = "civic-option1";
        private const string CivicOption2Name = "civic-option2";
        private const string CivicOption3Name = "civic-option3";
        private const string DiplomacyPanelName = "diplomacy-panel";
        private const string DiplomacyPanelTitleName = "diplomacy-panel-title";
        private const string DiplomacyOption1Name = "diplomacy-option1";
        private const string DiplomacyOption2Name = "diplomacy-option2";
        private const string DiplomacyOption3Name = "diplomacy-option3";

        private GameState state;
        private TurnSystem turnSystem;

        private Label turnLabel;
        private Label selectionLabel;
        private Label eventLabel;
        private Label cityLabel;
        private Label cityHoverLabel;
        private Label unitHoverLabel;
        private Label alertLabel;
        private Label productionLabel;
        private Label promotionLabel;
        private Label promotionDetailLabel;
        private Label combatLogLabel;
        private VisualElement techPanel;
        private Label techOption1;
        private Label techOption2;
        private Label techOption3;
        private Label techPanelTitle;
        private VisualElement techTreePanel;
        private Label techTreeTitle;
        private Label techTreeLabel;
        private VisualElement promotionPanel;
        private Label promotionPanelTitle;
        private Label promotionOption1;
        private Label promotionOption2;
        private Label promotionOption3;
        private Label researchLabel;
        private Label civicLabel;
        private Label resourceLabel;
        private Label resourceUnitLabel;
        private Label tradeLabel;
        private Label diplomacyStatusLabel;
        private VisualElement civicPanel;
        private Label civicPanelTitle;
        private Label civicOption1;
        private Label civicOption2;
        private Label civicOption3;
        private VisualElement diplomacyPanel;
        private Label diplomacyPanelTitle;
        private Label diplomacyOption1;
        private Label diplomacyOption2;
        private Label diplomacyOption3;
        private Button endTurnButton;
        private VisualElement tooltipPanel;
        private Label tooltipLabel;
        private Label resourceGoldLabel;
        private Label resourceScienceLabel;
        private Label resourceCultureLabel;
        private Label resourceFoodLabel;
        private Label resourceProductionLabel;
        private VisualElement unitPanel;
        private Label unitNameLabel;
        private Label unitHpLabel;
        private Label unitMoveLabel;
        private Label unitStrengthLabel;
        private VisualElement cityPanel;
        private Label cityNameLabel;
        private Label cityPopLabel;
        private Label cityGrowthLabel;
        private Label cityProductionLabel;
        private Label cityDefenseLabel;

        private System.Action onEndTurn;
        private Coroutine eventClearRoutine;
        private readonly System.Collections.Generic.List<string> combatLog = new System.Collections.Generic.List<string>();
        private const int CombatLogMax = 6;
        private const string EndTurnBlockedClass = "hud-endturn-blocked";

        private void Awake()
        {
            var document = GetComponent<UIDocument>();
            VisualElement root = document.rootVisualElement;

            turnLabel = root.Q<Label>(TurnLabelName);
            selectionLabel = root.Q<Label>(SelectionLabelName);
            eventLabel = root.Q<Label>(EventLabelName);
            cityLabel = root.Q<Label>(CityLabelName);
            cityHoverLabel = root.Q<Label>(CityHoverLabelName);
            unitHoverLabel = root.Q<Label>(UnitHoverLabelName);
            alertLabel = root.Q<Label>(AlertLabelName);
            productionLabel = root.Q<Label>(ProductionLabelName);
            promotionLabel = root.Q<Label>(PromotionLabelName);
            promotionDetailLabel = root.Q<Label>(PromotionDetailLabelName);
            combatLogLabel = root.Q<Label>(CombatLogLabelName);
            techPanel = root.Q<VisualElement>(TechPanelName);
            techOption1 = root.Q<Label>(TechOption1Name);
            techOption2 = root.Q<Label>(TechOption2Name);
            techOption3 = root.Q<Label>(TechOption3Name);
            techPanelTitle = root.Q<Label>(TechPanelTitleName);
            techTreePanel = root.Q<VisualElement>(TechTreePanelName);
            techTreeTitle = root.Q<Label>(TechTreeTitleName);
            techTreeLabel = root.Q<Label>(TechTreeLabelName);
            promotionPanel = root.Q<VisualElement>(PromotionPanelName);
            promotionPanelTitle = root.Q<Label>(PromotionPanelTitleName);
            promotionOption1 = root.Q<Label>(PromotionOption1Name);
            promotionOption2 = root.Q<Label>(PromotionOption2Name);
            promotionOption3 = root.Q<Label>(PromotionOption3Name);
            researchLabel = root.Q<Label>(ResearchLabelName);
            civicLabel = root.Q<Label>(CivicLabelName);
            resourceLabel = root.Q<Label>(ResourceLabelName);
            resourceUnitLabel = root.Q<Label>(ResourceUnitLabelName);
            tradeLabel = root.Q<Label>(TradeLabelName);
            diplomacyStatusLabel = root.Q<Label>(DiplomacyStatusLabelName);
            civicPanel = root.Q<VisualElement>(CivicPanelName);
            civicPanelTitle = root.Q<Label>(CivicPanelTitleName);
            civicOption1 = root.Q<Label>(CivicOption1Name);
            civicOption2 = root.Q<Label>(CivicOption2Name);
            civicOption3 = root.Q<Label>(CivicOption3Name);
            diplomacyPanel = root.Q<VisualElement>(DiplomacyPanelName);
            diplomacyPanelTitle = root.Q<Label>(DiplomacyPanelTitleName);
            diplomacyOption1 = root.Q<Label>(DiplomacyOption1Name);
            diplomacyOption2 = root.Q<Label>(DiplomacyOption2Name);
            diplomacyOption3 = root.Q<Label>(DiplomacyOption3Name);
            endTurnButton = root.Q<Button>(EndTurnButtonName);
            tooltipPanel = root.Q<VisualElement>(TooltipPanelName);
            tooltipLabel = root.Q<Label>(TooltipLabelName);
            resourceGoldLabel = root.Q<Label>(ResourceGoldLabelName);
            resourceScienceLabel = root.Q<Label>(ResourceScienceLabelName);
            resourceCultureLabel = root.Q<Label>(ResourceCultureLabelName);
            resourceFoodLabel = root.Q<Label>(ResourceFoodLabelName);
            resourceProductionLabel = root.Q<Label>(ResourceProductionLabelName);
            unitPanel = root.Q<VisualElement>(UnitPanelName);
            unitNameLabel = root.Q<Label>(UnitNameLabelName);
            unitHpLabel = root.Q<Label>(UnitHpLabelName);
            unitMoveLabel = root.Q<Label>(UnitMoveLabelName);
            unitStrengthLabel = root.Q<Label>(UnitStrengthLabelName);
            cityPanel = root.Q<VisualElement>(CityPanelName);
            cityNameLabel = root.Q<Label>(CityNameLabelName);
            cityPopLabel = root.Q<Label>(CityPopLabelName);
            cityGrowthLabel = root.Q<Label>(CityGrowthLabelName);
            cityProductionLabel = root.Q<Label>(CityProductionLabelName);
            cityDefenseLabel = root.Q<Label>(CityDefenseLabelName);

            if (endTurnButton != null)
            {
                endTurnButton.clicked += HandleEndTurn;
            }

            if (tooltipPanel != null)
            {
                tooltipPanel.style.display = DisplayStyle.None;
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

        public void Refresh()
        {
            if (turnLabel == null)
            {
                return;
            }

            if (state?.ActivePlayer == null)
            {
                turnLabel.text = "Turn -";
                return;
            }

            string playerName = string.IsNullOrWhiteSpace(state.ActivePlayer.Name)
                ? $"Player {state.ActivePlayer.Id}"
                : state.ActivePlayer.Name;
            turnLabel.text = $"Turn {state.CurrentTurn} - {playerName}";
        }

        public void SetSelection(string selection)
        {
            if (selectionLabel != null)
            {
                selectionLabel.text = selection ?? string.Empty;
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
                cityLabel.text = cityInfo ?? string.Empty;
            }
        }

        public void SetCityHoverInfo(string cityInfo)
        {
            if (cityHoverLabel != null)
            {
                cityHoverLabel.text = cityInfo ?? string.Empty;
            }
        }

        public void SetUnitHoverInfo(string unitInfo)
        {
            if (unitHoverLabel != null)
            {
                unitHoverLabel.text = unitInfo ?? string.Empty;
            }
        }

        public void SetAlertInfo(string alert)
        {
            if (alertLabel != null)
            {
                alertLabel.text = alert ?? string.Empty;
            }
        }

        public void SetTooltip(string text)
        {
            if (tooltipPanel == null || tooltipLabel == null)
            {
                return;
            }

            bool hasText = !string.IsNullOrWhiteSpace(text);
            tooltipPanel.style.display = hasText ? DisplayStyle.Flex : DisplayStyle.None;
            tooltipLabel.text = hasText ? text : string.Empty;
        }

        public void SetEndTurnState(bool blocked, string tooltip)
        {
            if (endTurnButton == null)
            {
                return;
            }

            endTurnButton.text = blocked ? "Needs Orders" : "End Turn";
            endTurnButton.tooltip = tooltip ?? string.Empty;
            if (blocked)
            {
                endTurnButton.AddToClassList(EndTurnBlockedClass);
            }
            else
            {
                endTurnButton.RemoveFromClassList(EndTurnBlockedClass);
            }
        }

        public void SetTopBarYields(string gold, string science, string culture, string food, string production)
        {
            if (resourceGoldLabel != null) resourceGoldLabel.text = gold ?? string.Empty;
            if (resourceScienceLabel != null) resourceScienceLabel.text = science ?? string.Empty;
            if (resourceCultureLabel != null) resourceCultureLabel.text = culture ?? string.Empty;
            if (resourceFoodLabel != null) resourceFoodLabel.text = food ?? string.Empty;
            if (resourceProductionLabel != null) resourceProductionLabel.text = production ?? string.Empty;
        }

        public void SetTopBarTooltips(string gold, string science, string culture, string food, string production)
        {
            if (resourceGoldLabel != null) resourceGoldLabel.tooltip = gold ?? string.Empty;
            if (resourceScienceLabel != null) resourceScienceLabel.tooltip = science ?? string.Empty;
            if (resourceCultureLabel != null) resourceCultureLabel.tooltip = culture ?? string.Empty;
            if (resourceFoodLabel != null) resourceFoodLabel.tooltip = food ?? string.Empty;
            if (resourceProductionLabel != null) resourceProductionLabel.tooltip = production ?? string.Empty;
        }

        public void SetUnitPanel(string name, string hp, string movement, string strength)
        {
            if (unitPanel == null)
            {
                return;
            }

            if (unitNameLabel != null) unitNameLabel.text = name ?? string.Empty;
            if (unitHpLabel != null) unitHpLabel.text = hp ?? string.Empty;
            if (unitMoveLabel != null) unitMoveLabel.text = movement ?? string.Empty;
            if (unitStrengthLabel != null) unitStrengthLabel.text = strength ?? string.Empty;
        }

        public void SetCityPanel(string name, string population, string growth, string production, string defense)
        {
            if (cityPanel == null)
            {
                return;
            }

            if (cityNameLabel != null) cityNameLabel.text = name ?? string.Empty;
            if (cityPopLabel != null) cityPopLabel.text = population ?? string.Empty;
            if (cityGrowthLabel != null) cityGrowthLabel.text = growth ?? string.Empty;
            if (cityProductionLabel != null) cityProductionLabel.text = production ?? string.Empty;
            if (cityDefenseLabel != null) cityDefenseLabel.text = defense ?? string.Empty;
        }

        public void SetProductionInfo(string productionInfo)
        {
            if (productionLabel != null)
            {
                productionLabel.text = productionInfo ?? string.Empty;
            }
        }

        public void SetProductionTooltip(string tooltip)
        {
            if (productionLabel != null)
            {
                productionLabel.tooltip = tooltip ?? string.Empty;
            }
        }

        public void SetPromotionInfo(string promotionInfo)
        {
            if (promotionLabel != null)
            {
                promotionLabel.text = promotionInfo ?? string.Empty;
            }
        }

        public void SetPromotionDetail(string detail)
        {
            if (promotionDetailLabel != null)
            {
                promotionDetailLabel.text = detail ?? string.Empty;
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

        public void ShowTechPanel(string title, string option1, string option2, string option3)
        {
            if (techPanel == null)
            {
                return;
            }

            techPanel.style.display = DisplayStyle.Flex;
            if (techPanelTitle != null)
            {
                techPanelTitle.text = title ?? string.Empty;
            }
            if (techOption1 != null) techOption1.text = option1 ?? string.Empty;
            if (techOption2 != null) techOption2.text = option2 ?? string.Empty;
            if (techOption3 != null) techOption3.text = option3 ?? string.Empty;
        }

        public void HideTechPanel()
        {
            if (techPanel == null)
            {
                return;
            }

            techPanel.style.display = DisplayStyle.None;
        }

        public void ShowTechTree(string text)
        {
            if (techTreePanel == null)
            {
                return;
            }

            techTreePanel.style.display = DisplayStyle.Flex;
            if (techTreeTitle != null)
            {
                techTreeTitle.text = "Tech Tree";
            }
            if (techTreeLabel != null)
            {
                techTreeLabel.text = text ?? string.Empty;
            }
        }

        public void HideTechTree()
        {
            if (techTreePanel == null)
            {
                return;
            }

            techTreePanel.style.display = DisplayStyle.None;
        }

        public void SetTechTreeTooltip(string tooltip)
        {
            if (techTreeLabel != null)
            {
                techTreeLabel.tooltip = tooltip ?? string.Empty;
            }
        }

        public void ShowPromotionPanel(string option1, string option2, string option3)
        {
            if (promotionPanel == null)
            {
                return;
            }

            promotionPanel.style.display = DisplayStyle.Flex;
            if (promotionPanelTitle != null)
            {
                promotionPanelTitle.text = "Choose Promotion";
            }
            if (promotionOption1 != null) promotionOption1.text = option1 ?? string.Empty;
            if (promotionOption2 != null) promotionOption2.text = option2 ?? string.Empty;
            if (promotionOption3 != null) promotionOption3.text = option3 ?? string.Empty;
        }

        public void HidePromotionPanel()
        {
            if (promotionPanel == null)
            {
                return;
            }

            promotionPanel.style.display = DisplayStyle.None;
        }

        public void ShowCivicPanel(string title, string option1, string option2, string option3)
        {
            if (civicPanel == null)
            {
                return;
            }

            civicPanel.style.display = DisplayStyle.Flex;
            if (civicPanelTitle != null)
            {
                civicPanelTitle.text = title ?? string.Empty;
            }
            if (civicOption1 != null) civicOption1.text = option1 ?? string.Empty;
            if (civicOption2 != null) civicOption2.text = option2 ?? string.Empty;
            if (civicOption3 != null) civicOption3.text = option3 ?? string.Empty;
        }

        public void HideCivicPanel()
        {
            if (civicPanel == null)
            {
                return;
            }

            civicPanel.style.display = DisplayStyle.None;
        }

        public void ShowDiplomacyPanel(string title, string option1, string option2, string option3)
        {
            if (diplomacyPanel == null)
            {
                return;
            }

            diplomacyPanel.style.display = DisplayStyle.Flex;
            if (diplomacyPanelTitle != null)
            {
                diplomacyPanelTitle.text = title ?? string.Empty;
            }
            if (diplomacyOption1 != null) diplomacyOption1.text = option1 ?? string.Empty;
            if (diplomacyOption2 != null) diplomacyOption2.text = option2 ?? string.Empty;
            if (diplomacyOption3 != null) diplomacyOption3.text = option3 ?? string.Empty;
        }

        public void HideDiplomacyPanel()
        {
            if (diplomacyPanel == null)
            {
                return;
            }

            diplomacyPanel.style.display = DisplayStyle.None;
        }

        public void SetResearchInfo(string text)
        {
            if (researchLabel != null)
            {
                researchLabel.text = text ?? string.Empty;
            }
        }

        public void SetCivicInfo(string text)
        {
            if (civicLabel != null)
            {
                civicLabel.text = text ?? string.Empty;
            }
        }

        public void SetResourceInfo(string text)
        {
            if (resourceLabel != null)
            {
                resourceLabel.text = text ?? string.Empty;
            }
        }

        public void SetResourceUnitInfo(string text)
        {
            if (resourceUnitLabel != null)
            {
                resourceUnitLabel.text = text ?? string.Empty;
            }
        }

        public void SetResourceUnitTooltip(string tooltip)
        {
            if (resourceUnitLabel != null)
            {
                resourceUnitLabel.tooltip = tooltip ?? string.Empty;
            }
        }

        public void SetTradeInfo(string text)
        {
            if (tradeLabel != null)
            {
                tradeLabel.text = text ?? string.Empty;
            }
        }

        public void SetDiplomacyStatus(string text)
        {
            if (diplomacyStatusLabel != null)
            {
                diplomacyStatusLabel.text = text ?? string.Empty;
            }
        }

        private void HandleEndTurn()
        {
            onEndTurn?.Invoke();
        }
    }
}
