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
        public ResourceType[] ResourceTypes;
        public CivicType[] CivicTypes;

        private Dictionary<string, TerrainType> terrainLookup;
        private Dictionary<string, UnitType> unitLookup;
        private Dictionary<string, ImprovementType> improvementLookup;
        private Dictionary<string, TechType> techLookup;
        private Dictionary<string, PromotionType> promotionLookup;
        private Dictionary<string, ResourceType> resourceLookup;
        private Dictionary<string, CivicType> civicLookup;

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
            color = Color.white;
            if (TryGetTerrainType(id, out var terrain) && terrain != null)
            {
                color = terrain.Color;
                return true;
            }

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
            color = Color.white;
            if (TryGetImprovementType(id, out var improvement) && improvement != null)
            {
                color = improvement.Color;
                return true;
            }

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

        public bool TryGetResourceType(string id, out ResourceType resourceType)
        {
            EnsureResourceLookup();
            if (resourceLookup != null && resourceLookup.TryGetValue(id, out resourceType))
            {
                return true;
            }

            resourceType = null;
            return false;
        }

        public bool TryGetResourceColor(string id, out Color color)
        {
            color = Color.white;
            if (TryGetResourceType(id, out var resource) && resource != null)
            {
                color = resource.Color;
                return true;
            }

            return false;
        }

        public bool TryGetCivicType(string id, out CivicType civicType)
        {
            EnsureCivicLookup();
            if (civicLookup != null && civicLookup.TryGetValue(id, out civicType))
            {
                return true;
            }

            civicType = null;
            return false;
        }

        public void LoadFromDefinitions(IEnumerable<TerrainTypeDefinition> terrainDefinitions,
            IEnumerable<UnitTypeDefinition> unitDefinitions,
            IEnumerable<ImprovementTypeDefinition> improvementDefinitions,
            IEnumerable<TechTypeDefinition> techDefinitions,
            IEnumerable<PromotionTypeDefinition> promotionDefinitions,
            IEnumerable<ResourceTypeDefinition> resourceDefinitions,
            IEnumerable<CivicTypeDefinition> civicDefinitions)
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
                    unit.WorkCost = definition.WorkCost;
                    unit.Range = definition.Range;
                    unit.RequiresResource = definition.RequiresResource;
                    unit.RequiresTech = definition.RequiresTech;
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
                    improvement.FoodBonus = definition.FoodBonus;
                    improvement.ProductionBonus = definition.ProductionBonus;
                    improvement.Color = definition.Color;
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
                    tech.Prerequisites = definition.Prerequisites;
                    techList.Add(tech);
                }
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
                    promotion.Requires = definition.Requires;
                    promotionList.Add(promotion);
                }
            }

            var resourceList = new List<ResourceType>();
            if (resourceDefinitions != null)
            {
                foreach (var definition in resourceDefinitions)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    {
                        continue;
                    }

                    var resource = ScriptableObject.CreateInstance<ResourceType>();
                    resource.Id = definition.Id;
                    resource.DisplayName = definition.DisplayName;
                    resource.Category = string.IsNullOrWhiteSpace(definition.Category) ? "Bonus" : definition.Category;
                    resource.FoodBonus = definition.FoodBonus;
                    resource.ProductionBonus = definition.ProductionBonus;
                    resource.ScienceBonus = definition.ScienceBonus;
                    resource.Color = definition.Color;
                    resourceList.Add(resource);
                }
            }

            var civicList = new List<CivicType>();
            if (civicDefinitions != null)
            {
                foreach (var definition in civicDefinitions)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    {
                        continue;
                    }

                    var civic = ScriptableObject.CreateInstance<CivicType>();
                    civic.Id = definition.Id;
                    civic.DisplayName = definition.DisplayName;
                    civic.Category = string.IsNullOrWhiteSpace(definition.Category) ? "Government" : definition.Category;
                    civic.Description = definition.Description;
                    civicList.Add(civic);
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
            if (techList.Count > 0)
            {
                TechTypes = techList.ToArray();
            }
            if (promotionList.Count > 0)
            {
                PromotionTypes = promotionList.ToArray();
            }
            if (resourceList.Count > 0)
            {
                ResourceTypes = resourceList.ToArray();
            }
            if (civicList.Count > 0)
            {
                CivicTypes = civicList.ToArray();
            }

            terrainLookup = null;
            unitLookup = null;
            improvementLookup = null;
            techLookup = null;
            promotionLookup = null;
            resourceLookup = null;
            civicLookup = null;
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
                if (terrain != null && !string.IsNullOrWhiteSpace(terrain.Id))
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
                if (unit != null && !string.IsNullOrWhiteSpace(unit.Id))
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
                if (improvement != null && !string.IsNullOrWhiteSpace(improvement.Id))
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
                if (tech != null && !string.IsNullOrWhiteSpace(tech.Id))
                {
                    techLookup[tech.Id] = tech;
                }
            }
        }

        private void EnsurePromotionLookup()
        {
            if (promotionLookup != null)
            {
                return;
            }

            promotionLookup = new Dictionary<string, PromotionType>();
            if (PromotionTypes == null)
            {
                return;
            }

            foreach (var promotion in PromotionTypes)
            {
                if (promotion != null && !string.IsNullOrWhiteSpace(promotion.Id))
                {
                    promotionLookup[promotion.Id] = promotion;
                }
            }
        }

        private void EnsureResourceLookup()
        {
            if (resourceLookup != null)
            {
                return;
            }

            resourceLookup = new Dictionary<string, ResourceType>();
            if (ResourceTypes == null)
            {
                return;
            }

            foreach (var resource in ResourceTypes)
            {
                if (resource != null && !string.IsNullOrWhiteSpace(resource.Id))
                {
                    resourceLookup[resource.Id] = resource;
                }
            }
        }

        private void EnsureCivicLookup()
        {
            if (civicLookup != null)
            {
                return;
            }

            civicLookup = new Dictionary<string, CivicType>();
            if (CivicTypes == null)
            {
                return;
            }

            foreach (var civic in CivicTypes)
            {
                if (civic != null && !string.IsNullOrWhiteSpace(civic.Id))
                {
                    civicLookup[civic.Id] = civic;
                }
            }
        }
    }
}
