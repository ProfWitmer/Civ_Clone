using System.Collections.Generic;
using CivClone.Infrastructure.Data.Definitions;
using UnityEngine;

namespace CivClone.Infrastructure
{
    [CreateAssetMenu(menuName = "Civ Clone/Game Data Catalog", fileName = "GameDataCatalog")]
    public class GameDataCatalog : ScriptableObject
    {
        public TerrainType[] TerrainTypes;
        public UnitType[] UnitTypes;

        private Dictionary<string, TerrainType> _terrainLookup;
        private Dictionary<string, UnitType> _unitLookup;

                public bool TryGetTerrainType(string id, out TerrainType terrainType)
        {
            EnsureTerrainLookup();
            if (_terrainLookup != null && _terrainLookup.TryGetValue(id, out terrainType))
            {
                return true;
            }

            terrainType = null;
            return false;
        }

public bool TryGetTerrainColor(string id, out Color color)
        {
            EnsureTerrainLookup();
            if (_terrainLookup != null && _terrainLookup.TryGetValue(id, out var terrain))
            {
                color = terrain.Color;
                return true;
            }

            color = default;
            return false;
        }

        public bool TryGetUnitType(string id, out UnitType unitType)
        {
            EnsureUnitLookup();
            if (_unitLookup != null && _unitLookup.TryGetValue(id, out unitType))
            {
                return true;
            }

            unitType = null;
            return false;
        }

        public void LoadFromDefinitions(IEnumerable<TerrainTypeDefinition> terrainDefinitions, IEnumerable<UnitTypeDefinition> unitDefinitions)
        {
            var terrainList = new List<TerrainType>();
            if (terrainDefinitions != null)
            {
                foreach (var definition in terrainDefinitions)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    {
                        continue;
                    }

                    var terrain = ScriptableObject.CreateInstance<TerrainType>();
                    terrain.Id = definition.Id;
                    terrain.DisplayName = definition.DisplayName;
                    terrain.MovementCost = definition.MovementCost;
                    terrain.Color = definition.Color;
                    terrainList.Add(terrain);
                }
            }

            var unitList = new List<UnitType>();
            if (unitDefinitions != null)
            {
                foreach (var definition in unitDefinitions)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    {
                        continue;
                    }

                    var unit = ScriptableObject.CreateInstance<UnitType>();
                    unit.Id = definition.Id;
                    unit.DisplayName = definition.DisplayName;
                    unit.MovementPoints = definition.MovementPoints;
                    unit.Attack = definition.Attack;
                    unit.Defense = definition.Defense;
                    unitList.Add(unit);
                }
            }

            if (terrainList.Count > 0)
            {
                TerrainTypes = terrainList.ToArray();
            }

            if (unitList.Count > 0)
            {
                UnitTypes = unitList.ToArray();
            }

            _terrainLookup = null;
            _unitLookup = null;
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

        private void EnsureUnitLookup()
        {
            if (_unitLookup != null)
            {
                return;
            }

            _unitLookup = new Dictionary<string, UnitType>();
            if (UnitTypes == null)
            {
                return;
            }

            foreach (var unit in UnitTypes)
            {
                if (unit != null && !string.IsNullOrEmpty(unit.Id))
                {
                    _unitLookup[unit.Id] = unit;
                }
            }
        }
    }
}
