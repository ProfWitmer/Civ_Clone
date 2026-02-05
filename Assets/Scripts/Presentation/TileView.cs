using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class TileView : MonoBehaviour
    {
        public GridPosition Position { get; private set; }

        public void Bind(GridPosition position)
        {
            Position = position;
        }
    }
}
