using System.Collections.Generic;
using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Game Data Catalog", fileName = "GameDataCatalog")]
    public class GameDataCatalog : ScriptableObject
    {
        public TerrainType[] TerrainTypes;
        public UnitType[] UnitTypes;

        private Dictionary<string, TerrainType> _terrainLookup;

        public bool TryGetTerrainColor(string id, out Color color)
        {
            EnsureTerrainLookup();
            if (_terrainLookup.TryGetValue(id, out var terrain))
            {
                color = terrain.Color;
                return true;
            }

            color = default;
            return false;
        }

        private void EnsureTerrainLookup()
        {
            if (_terrainLookup != null)
            {
                return;
            }

            _terrainLookup = new Dictionary<string, TerrainType>();
            if (TerrainTypes == null)
            {
                return;
            }

            foreach (var terrain in TerrainTypes)
            {
                if (terrain != null && !string.IsNullOrEmpty(terrain.Id))
                {
                    _terrainLookup[terrain.Id] = terrain;
                }
            }
        }
    }
}
