using System;

namespace CivClone.Simulation
{
    [Serializable]
    public class Unit
    {
        public string UnitTypeId;
        public GridPosition Position;
        public int MovementPoints;
        public int MovementRemaining;
        public int WorkRemaining;
        public int OwnerId;

        public Unit(string unitTypeId, GridPosition position, int movementPoints, int ownerId)
        {
            UnitTypeId = unitTypeId;
            Position = position;
            MovementPoints = movementPoints;
            MovementRemaining = movementPoints;
            WorkRemaining = 0;
            OwnerId = ownerId;
        }

        public void ResetMovement()
        {
            MovementRemaining = MovementPoints;
        }

        public void ResetWork(int workCost)
        {
            WorkRemaining = workCost;
        }
    }
}
