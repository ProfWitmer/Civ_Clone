using System;
using System.Collections.Generic;

namespace CivClone.Simulation
{
    [Serializable]
    public class Player
    {
        public int Id;
        public string Name;
        public List<Unit> Units = new List<Unit>();
        public List<City> Cities = new List<City>();
        public List<string> KnownTechs = new List<string>();
        public List<CivicSelection> Civics = new List<CivicSelection>();
        public List<string> AvailableResources = new List<string>();
        public List<TradeRoute> TradeRoutes = new List<TradeRoute>();
        public string CurrentTechId;
        public int ResearchProgress;

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
