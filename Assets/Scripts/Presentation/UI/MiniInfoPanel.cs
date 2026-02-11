using UnityEngine;
using UnityEngine.UI;

namespace CivClone.Presentation.UI
{
    public sealed class MiniInfoPanel : MonoBehaviour
    {
        [SerializeField] private Text compassLabel;
        [SerializeField] private Text turnLabel;
        [SerializeField] private Text yearLabel;

        [SerializeField] private string compassText = "N";
        [SerializeField] private int startTurn = 1;
        [SerializeField] private string startYear = "4000 BC";

        [Header("Style")]
        [SerializeField] private Color textColor = new Color(0.95f, 0.85f, 0.55f, 1f);
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Vector2 shadowOffset = new Vector2(1.5f, -1.5f);

        private int currentTurn;
        private string currentYear;

        private void Awake()
        {
            currentTurn = startTurn;
            currentYear = startYear;
            ApplyStyle(compassLabel);
            ApplyStyle(turnLabel);
            ApplyStyle(yearLabel);
            Refresh();
        }

        public void SetCompass(string text)
        {
            compassText = text;
            Refresh();
        }

        public void SetTurn(int turn)
        {
            currentTurn = turn;
            Refresh();
        }

        public void SetYear(string year)
        {
            currentYear = year;
            Refresh();
        }

        private void Refresh()
        {
            if (compassLabel != null)
            {
                compassLabel.text = compassText;
            }

            if (turnLabel != null)
            {
                turnLabel.text = $"Turn {currentTurn}";
            }

            if (yearLabel != null)
            {
                yearLabel.text = currentYear;
            }
        }

        private void ApplyStyle(Text text)
        {
            if (text == null)
            {
                return;
            }

            text.color = textColor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            if (text.GetComponent<Shadow>() == null)
            {
                Shadow shadow = text.gameObject.AddComponent<Shadow>();
                shadow.effectColor = shadowColor;
                shadow.effectDistance = shadowOffset;
            }
            else
            {
                Shadow shadow = text.GetComponent<Shadow>();
                shadow.effectColor = shadowColor;
                shadow.effectDistance = shadowOffset;
            }
        }
    }
}
