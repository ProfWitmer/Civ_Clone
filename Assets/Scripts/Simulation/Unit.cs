using System;

namespace CivClone.Simulation
{
    [Serializable]
    public class Unit
    {
        public string UnitTypeId;
        public GridPosition Position;
        public int MovementPoints;
        public int OwnerId;

        public Unit(string unitTypeId, GridPosition position, int movementPoints, int ownerId)
        {
            UnitTypeId = unitTypeId;
            Position = position;
            MovementPoints = movementPoints;
            OwnerId = ownerId;
        }
    }
}
