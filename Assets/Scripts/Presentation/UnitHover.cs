using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class UnitHover : MonoBehaviour
    {
        private HudController hud;
        private Unit unit;

        public void Bind(HudController hudController, Unit unitRef)
        {
            hud = hudController;
            unit = unitRef;
        }

        private void OnMouseEnter()
        {
            if (hud == null || unit == null)
            {
                return;
            }

            int max = Mathf.Max(1, unit.MaxHealth);
            int move = Mathf.Max(0, unit.MovementRemaining);
            string info = $"{unit.UnitTypeId} HP {unit.Health}/{max} Move {move}/{unit.MovementPoints}";
            if (unit.Promotions != null && unit.Promotions.Count > 0)
            {
                info += " Promos: " + string.Join(", ", unit.Promotions);
            }

            hud.SetUnitHoverInfo(info);
        }

        private void OnMouseExit()
        {
            hud?.SetUnitHoverInfo(string.Empty);
        }
    }
}
