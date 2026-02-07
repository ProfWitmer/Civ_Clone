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
        public ImprovementType[] ImprovementTypes;
        public TechType[] TechTypes;
        public PromotionType[] PromotionTypes;

        private Dictionary<string, TerrainType> terrainLookup;
        private Dictionary<string, UnitType> unitLookup;
        private Dictionary<string, ImprovementType> improvementLookup;
        private Dictionary<string, TechType> techLookup;
        private Dictionary<string, PromotionType> promotionLookup;

        public bool TryGetTerrainType(string id, out TerrainType terrainType)
        {
            EnsureTerrainLookup();
            if (terrainLookup != null && terrainLookup.TryGetValue(id, out terrainType))
            {
                return true;
            }

            terrainType = null;
            return false;
        }

        public bool TryGetTerrainColor(string id, out Color color)
        {
            if (TryGetTerrainType(id, out var terrain))
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
            if (unitLookup != null && unitLookup.TryGetValue(id, out unitType))
            {
                return true;
            }

            unitType = null;
            return false;
        }

        public bool TryGetImprovementType(string id, out ImprovementType improvementType)
        {
            EnsureImprovementLookup();
            if (improvementLookup != null && improvementLookup.TryGetValue(id, out improvementType))
            {
                return true;
            }

            improvementType = null;
            return false;
        }

        public bool TryGetImprovementColor(string id, out Color color)
        {
            if (TryGetImprovementType(id, out var improvement))
            {
                color = improvement.Color;
                return true;
            }

            color = default;
            return false;
        }

        public bool TryGetTechType(string id, out TechType techType)
        {
            EnsureTechLookup();
            if (techLookup != null && techLookup.TryGetValue(id, out techType))
            {
                return true;
            }

            techType = null;
            return false;
        }
        public bool TryGetPromotionType(string id, out PromotionType promotionType)
        {
            EnsurePromotionLookup();
            if (promotionLookup != null && promotionLookup.TryGetValue(id, out promotionType))
            {
                return true;
            }

            promotionType = null;
            return false;
        }


        public void LoadFromDefinitions(IEnumerable<TerrainTypeDefinition> terrainDefinitions,
            IEnumerable<UnitTypeDefinition> unitDefinitions,
            IEnumerable<ImprovementTypeDefinition> improvementDefinitions,
            IEnumerable<TechTypeDefinition> techDefinitions,
            IEnumerable<PromotionTypeDefinition> promotionDefinitions)
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
                    terrain.DefenseBonus = definition.DefenseBonus;
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
                    unit.ProductionCost = definition.ProductionCost;
                    unitList.Add(unit);
                }
            }

            var improvementList = new List<ImprovementType>();
            if (improvementDefinitions != null)
            {
                foreach (var definition in improvementDefinitions)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    {
                        continue;
                    }

                    var improvement = ScriptableObject.CreateInstance<ImprovementType>();
                    improvement.Id = definition.Id;
                    improvement.DisplayName = definition.DisplayName;
                    improvement.Color = definition.Color;
                    improvement.FoodBonus = definition.FoodBonus;
                    improvement.ProductionBonus = definition.ProductionBonus;
                    improvementList.Add(improvement);
                }
            }

            var techList = new List<TechType>();
            if (techDefinitions != null)
            {
                foreach (var definition in techDefinitions)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    {
                        continue;
                    }

                    var tech = ScriptableObject.CreateInstance<TechType>();
                    tech.Id = definition.Id;
                    tech.DisplayName = definition.DisplayName;
                    tech.Cost = definition.Cost;
                    techList.Add(tech);
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

            if (improvementList.Count > 0)
            {
                ImprovementTypes = improvementList.ToArray();
            }


            var promotionList = new List<PromotionType>();
            if (promotionDefinitions != null)
            {
                foreach (var definition in promotionDefinitions)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    {
                        continue;
                    }

                    var promotion = ScriptableObject.CreateInstance<PromotionType>();
                    promotion.Id = definition.Id;
                    promotion.DisplayName = definition.DisplayName;
                    promotion.Description = definition.Description;
                    promotionList.Add(promotion);
                }
            }
            if (techList.Count > 0)
            {
                TechTypes = techList.ToArray();
            }

            terrainLookup = null;
            unitLookup = null;
            improvementLookup = null;
            techLookup = null;
            promotionLookup = null;
        }

        private void EnsureTerrainLookup()
        {
            if (terrainLookup != null)
            {
                return;
            }

            terrainLookup = new Dictionary<string, TerrainType>();
            if (TerrainTypes == null)
            {
                return;
            }

            foreach (var terrain in TerrainTypes)
            {
                if (terrain != null && !string.IsNullOrEmpty(terrain.Id))
                {
                    terrainLookup[terrain.Id] = terrain;
                }
            }
        }

        private void EnsureUnitLookup()
        {
            if (unitLookup != null)
            {
                return;
            }

            unitLookup = new Dictionary<string, UnitType>();
            if (UnitTypes == null)
            {
                return;
            }

            foreach (var unit in UnitTypes)
            {
                if (unit != null && !string.IsNullOrEmpty(unit.Id))
                {
                    unitLookup[unit.Id] = unit;
                }
            }
        }

        private void EnsureImprovementLookup()
        {
            if (improvementLookup != null)
            {
                return;
            }

            improvementLookup = new Dictionary<string, ImprovementType>();
            if (ImprovementTypes == null)
            {
                return;
            }

            foreach (var improvement in ImprovementTypes)
            {
                if (improvement != null && !string.IsNullOrEmpty(improvement.Id))
                {
                    improvementLookup[improvement.Id] = improvement;
                }
            }
        }

        private void EnsureTechLookup()
        {
            if (techLookup != null)
            {
                return;
            }

            techLookup = new Dictionary<string, TechType>();
            if (TechTypes == null)
            {
                return;
            }

            foreach (var tech in TechTypes)
            {
                if (tech != null && !string.IsNullOrEmpty(tech.Id))
                {
                    techLookup[tech.Id] = tech;
                }
            }
        }
    }
}
