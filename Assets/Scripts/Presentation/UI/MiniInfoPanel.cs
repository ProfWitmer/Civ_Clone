using TMPro;
using UnityEngine;

namespace CivClone.Presentation.UI
{
    public sealed class MiniInfoPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text compassLabel;
        [SerializeField] private TMP_Text turnLabel;
        [SerializeField] private TMP_Text yearLabel;

        [SerializeField] private string compassText = "N";
        [SerializeField] private int startTurn = 1;
        [SerializeField] private string startYear = "4000 BC";

        private int currentTurn;
        private string currentYear;

        private void Awake()
        {
            currentTurn = startTurn;
            currentYear = startYear;
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
    }
}
