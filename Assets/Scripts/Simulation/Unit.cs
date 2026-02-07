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
        public int MaxHealth = 10;
        public int Health = 10;
        public System.Collections.Generic.List<string> Promotions = new System.Collections.Generic.List<string>();
        public string WorkTargetImprovementId;
        public bool WorkTargetIsRoad;
        public GridPosition WorkTargetPosition;

        public Unit(string unitTypeId, GridPosition position, int movementPoints, int ownerId)
        {
            UnitTypeId = unitTypeId;
            Position = position;
            MovementPoints = movementPoints;
            MovementRemaining = movementPoints;
            WorkRemaining = 0;
            OwnerId = ownerId;
            Health = MaxHealth;
            WorkTargetImprovementId = string.Empty;
            WorkTargetIsRoad = false;
            WorkTargetPosition = position;
        }

        public void ResetMovement()
        {
            MovementRemaining = MovementPoints;
        }

        public void ResetWork(int workCost)
        {
            WorkRemaining = workCost;
        }

        public void StartWork(GridPosition position, string improvementId, int workCost)
        {
            WorkTargetPosition = position;
            WorkTargetImprovementId = improvementId ?? string.Empty;
            WorkTargetIsRoad = false;
            WorkRemaining = workCost;
        }

        public void StartRoadWork(GridPosition position, int workCost)
        {
            WorkTargetPosition = position;
            WorkTargetImprovementId = string.Empty;
            WorkTargetIsRoad = true;
            WorkRemaining = workCost;
        }
    }
}
