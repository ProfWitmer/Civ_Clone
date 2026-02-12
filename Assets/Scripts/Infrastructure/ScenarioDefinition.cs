using System;
using System.Collections.Generic;

namespace CivClone.Infrastructure
{
    [Serializable]
    public class ScenarioDefinition
    {
        public ScenarioMapDefinition Map;
        public List<ScenarioPlayerDefinition> Players = new List<ScenarioPlayerDefinition>();
        public int ActivePlayerIndex = 0;
        public ScenarioVictoryDefinition Victory;
        public List<ScenarioEventDefinition> Events = new List<ScenarioEventDefinition>();
    }

    [Serializable]
    public class ScenarioSelector
    {
        public string ScenarioId = "";
    }

    [Serializable]
    public class ScenarioCatalog
    {
        public ScenarioCatalogEntry[] Items;
    }

    [Serializable]
    public class ScenarioCatalogEntry
    {
        public string Id;
        public string Name;
        public string Path;
        public string Description;
    }

    [Serializable]
    public class ScenarioMapDefinition
    {
        public int Width = 20;
        public int Height = 12;
        public int Seed = 12345;
        public string DefaultTerrainId = "plains";
    }

    [Serializable]
    public class ScenarioPlayerDefinition
    {
        public int Id = 0;
        public string Name = "Player";
        public string CurrentTechId = "";
        public List<string> StartingTechs = new List<string>();
        public List<ScenarioUnitDefinition> Units = new List<ScenarioUnitDefinition>();
        public List<ScenarioCityDefinition> Cities = new List<ScenarioCityDefinition>();
    }

    [Serializable]
    public class ScenarioUnitDefinition
    {
        public string UnitTypeId = "scout";
        public int X = 0;
        public int Y = 0;
        public int MovementPoints = 0;
    }

    [Serializable]
    public class ScenarioCityDefinition
    {
        public string Name = "City";
        public int X = 0;
        public int Y = 0;
        public int Population = 1;
    }

    [Serializable]
    public class ScenarioVictoryDefinition
    {
        public int TurnLimit = 0;
        public bool EliminateAllOpponents = false;
    }

    [Serializable]
    public class ScenarioEventDefinition
    {
        public string Id = "";
        public int Turn = 1;
        public string Type = "";
        public int TargetPlayerId = 0;
        public string TechId = "";
        public string ResourceId = "";
        public string UnitTypeId = "";
        public int X = 0;
        public int Y = 0;
        public int OtherPlayerId = 0;
        public string Message = "";
    }
}
