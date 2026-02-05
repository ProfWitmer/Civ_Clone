using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class UnitView : MonoBehaviour
    {
        public Unit Unit { get; private set; }

        public void Bind(Unit unit)
        {
            Unit = unit;
        }
    }
}
