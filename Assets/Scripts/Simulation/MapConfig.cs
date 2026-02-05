using System;

namespace CivClone.Simulation
{
    [Serializable]
    public class MapConfig
    {
        public int Width = 20;
        public int Height = 12;
        public int Seed = 12345;
        public string DefaultTerrainId = "plains";
    }
}
