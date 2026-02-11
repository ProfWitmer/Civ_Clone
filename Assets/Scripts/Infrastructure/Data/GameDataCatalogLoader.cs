using System;
using System.Collections.Generic;
using CivClone.Infrastructure.Data.Definitions;
using UnityEngine;

namespace CivClone.Infrastructure.Data
{
    public sealed class GameDataCatalogLoader
    {
        private const string TerrainResourcePath = "Data/terrain_types";
        private const string UnitResourcePath = "Data/unit_types";
        private const string ImprovementResourcePath = "Data/improvement_types";
        private const string TechResourcePath = "Data/tech_types";
        private const string PromotionResourcePath = "Data/promotion_types";
        private const string ResourceResourcePath = "Data/resource_types";
        private const string CivicResourcePath = "Data/civic_types";
        private const string BuildingResourcePath = "Data/building_types";

        private const string TerrainCsvResourcePath = "Csv/terrain_types";
        private const string UnitCsvResourcePath = "Csv/unit_types";
        private const string ImprovementCsvResourcePath = "Csv/improvement_types";
        private const string TechCsvResourcePath = "Csv/tech_types";
        private const string PromotionCsvResourcePath = "Csv/promotion_types";
        private const string ResourceCsvResourcePath = "Csv/resource_types";
        private const string CivicCsvResourcePath = "Csv/civic_types";
        private const string BuildingCsvResourcePath = "Csv/building_types";

        public bool TryLoadFromResources(GameDataCatalog catalog)
        {
            if (catalog == null)
            {
                return false;
            }

            var dataLoader = new DataLoader();
            var terrainDefinitions = LoadTerrainDefinitions(dataLoader);
            var unitDefinitions = LoadUnitDefinitions(dataLoader);
            var improvementDefinitions = LoadImprovementDefinitions(dataLoader);
            var techDefinitions = LoadTechDefinitions(dataLoader);
            var promotionDefinitions = LoadPromotionDefinitions(dataLoader);
            var resourceDefinitions = LoadResourceDefinitions(dataLoader);
            var civicDefinitions = LoadCivicDefinitions(dataLoader);
            var buildingDefinitions = LoadBuildingDefinitions(dataLoader);

            if (terrainDefinitions.Count == 0 && unitDefinitions.Count == 0 && improvementDefinitions.Count == 0 && techDefinitions.Count == 0 && promotionDefinitions.Count == 0 && resourceDefinitions.Count == 0 && civicDefinitions.Count == 0 && buildingDefinitions.Count == 0)
            {
                return false;
            }

            catalog.LoadFromDefinitions(terrainDefinitions, unitDefinitions, improvementDefinitions, techDefinitions, promotionDefinitions, resourceDefinitions, civicDefinitions, buildingDefinitions);
            return true;
        }

        private static List<TerrainTypeDefinition> LoadTerrainDefinitions(DataLoader loader)
        {
            var definitions = new List<TerrainTypeDefinition>();
            var json = loader.LoadResourceText(TerrainResourcePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonUtility.FromJson<TerrainTypeDefinitionList>(json);
                if (list?.Items != null)
                {
                    definitions.AddRange(list.Items);
                }

                return definitions;
            }

            var csv = loader.LoadResourceText(TerrainCsvResourcePath);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return definitions;
            }

            var rows = SimpleCsv.Parse(csv);
            if (rows.Count == 0)
            {
                return definitions;
            }

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = new TerrainTypeDefinition
                {
                    Id = GetValue(header, row, "Id"),
                    DisplayName = GetValue(header, row, "DisplayName"),
                    MovementCost = GetIntValue(header, row, "MovementCost", 1),
                    DefenseBonus = GetIntValue(header, row, "DefenseBonus", 0),
                    Color = ParseColor(GetValue(header, row, "ColorHex"))
                };

                definitions.Add(definition);
            }

            return definitions;
        }

        private static List<UnitTypeDefinition> LoadUnitDefinitions(DataLoader loader)
        {
            var definitions = new List<UnitTypeDefinition>();
            var json = loader.LoadResourceText(UnitResourcePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonUtility.FromJson<UnitTypeDefinitionList>(json);
                if (list?.Items != null)
                {
                    definitions.AddRange(list.Items);
                }

                return definitions;
            }

            var csv = loader.LoadResourceText(UnitCsvResourcePath);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return definitions;
            }

            var rows = SimpleCsv.Parse(csv);
            if (rows.Count == 0)
            {
                return definitions;
            }

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = new UnitTypeDefinition
                {
                    Id = GetValue(header, row, "Id"),
                    DisplayName = GetValue(header, row, "DisplayName"),
                    MovementPoints = GetIntValue(header, row, "MovementPoints", 1),
                    Attack = GetIntValue(header, row, "Attack", 1),
                    Defense = GetIntValue(header, row, "Defense", 1),
                    ProductionCost = GetIntValue(header, row, "ProductionCost", 10),
                    WorkCost = GetIntValue(header, row, "WorkCost", 2),
                    Range = GetIntValue(header, row, "Range", 1),
                    RequiresResource = GetValue(header, row, "RequiresResource"),
                    RequiresTech = GetValue(header, row, "RequiresTech")
                };

                definitions.Add(definition);
            }

            return definitions;
        }

        private static List<ImprovementTypeDefinition> LoadImprovementDefinitions(DataLoader loader)
        {
            var definitions = new List<ImprovementTypeDefinition>();
            var json = loader.LoadResourceText(ImprovementResourcePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonUtility.FromJson<ImprovementTypeDefinitionList>(json);
                if (list?.Items != null)
                {
                    definitions.AddRange(list.Items);
                }

                return definitions;
            }

            var csv = loader.LoadResourceText(ImprovementCsvResourcePath);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return definitions;
            }

            var rows = SimpleCsv.Parse(csv);
            if (rows.Count == 0)
            {
                return definitions;
            }

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = new ImprovementTypeDefinition
                {
                    Id = GetValue(header, row, "Id"),
                    DisplayName = GetValue(header, row, "DisplayName"),
                    FoodBonus = GetIntValue(header, row, "FoodBonus", 0),
                    ProductionBonus = GetIntValue(header, row, "ProductionBonus", 0),
                    Color = ParseColor(GetValue(header, row, "ColorHex"))
                };

                definitions.Add(definition);
            }

            return definitions;
        }

        private static List<TechTypeDefinition> LoadTechDefinitions(DataLoader loader)
        {
            var definitions = new List<TechTypeDefinition>();
            var json = loader.LoadResourceText(TechResourcePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonUtility.FromJson<TechTypeDefinitionList>(json);
                if (list?.Items != null)
                {
                    definitions.AddRange(list.Items);
                }

                return definitions;
            }

            var csv = loader.LoadResourceText(TechCsvResourcePath);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return definitions;
            }

            var rows = SimpleCsv.Parse(csv);
            if (rows.Count == 0)
            {
                return definitions;
            }

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = new TechTypeDefinition
                {
                    Id = GetValue(header, row, "Id"),
                    DisplayName = GetValue(header, row, "DisplayName"),
                    Cost = GetIntValue(header, row, "Cost", 20),
                    Prerequisites = GetValue(header, row, "Prerequisites")
                };

                definitions.Add(definition);
            }

            return definitions;
        }

        private static List<PromotionTypeDefinition> LoadPromotionDefinitions(DataLoader loader)
        {
            var definitions = new List<PromotionTypeDefinition>();
            var json = loader.LoadResourceText(PromotionResourcePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonUtility.FromJson<PromotionTypeDefinitionList>(json);
                if (list?.Items != null)
                {
                    definitions.AddRange(list.Items);
                }

                return definitions;
            }

            var csv = loader.LoadResourceText(PromotionCsvResourcePath);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return definitions;
            }

            var rows = SimpleCsv.Parse(csv);
            if (rows.Count == 0)
            {
                return definitions;
            }

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = new PromotionTypeDefinition
                {
                    Id = GetValue(header, row, "Id"),
                    DisplayName = GetValue(header, row, "DisplayName"),
                    Description = GetValue(header, row, "Description"),
                    Requires = GetValue(header, row, "Requires")
                };

                definitions.Add(definition);
            }

            return definitions;
        }

        private static List<ResourceTypeDefinition> LoadResourceDefinitions(DataLoader loader)
        {
            var definitions = new List<ResourceTypeDefinition>();
            var json = loader.LoadResourceText(ResourceResourcePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonUtility.FromJson<ResourceTypeDefinitionList>(json);
                if (list?.Items != null)
                {
                    definitions.AddRange(list.Items);
                }

                return definitions;
            }

            var csv = loader.LoadResourceText(ResourceCsvResourcePath);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return definitions;
            }

            var rows = SimpleCsv.Parse(csv);
            if (rows.Count == 0)
            {
                return definitions;
            }

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = new ResourceTypeDefinition
                {
                    Id = GetValue(header, row, "Id"),
                    DisplayName = GetValue(header, row, "DisplayName"),
                    Category = GetValue(header, row, "Category"),
                    FoodBonus = GetIntValue(header, row, "FoodBonus", 0),
                    ProductionBonus = GetIntValue(header, row, "ProductionBonus", 0),
                    ScienceBonus = GetIntValue(header, row, "ScienceBonus", 0),
                    Color = ParseColor(GetValue(header, row, "ColorHex"))
                };

                definitions.Add(definition);
            }

            return definitions;
        }

        private static List<CivicTypeDefinition> LoadCivicDefinitions(DataLoader loader)
        {
            var definitions = new List<CivicTypeDefinition>();
            var json = loader.LoadResourceText(CivicResourcePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonUtility.FromJson<CivicTypeDefinitionList>(json);
                if (list?.Items != null)
                {
                    definitions.AddRange(list.Items);
                }

                return definitions;
            }

            var csv = loader.LoadResourceText(CivicCsvResourcePath);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return definitions;
            }

            var rows = SimpleCsv.Parse(csv);
            if (rows.Count == 0)
            {
                return definitions;
            }

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = new CivicTypeDefinition
                {
                    Id = GetValue(header, row, "Id"),
                    DisplayName = GetValue(header, row, "DisplayName"),
                    Category = GetValue(header, row, "Category"),
                    Description = GetValue(header, row, "Description")
                };

                definitions.Add(definition);
            }

            return definitions;
        }

        private static List<BuildingTypeDefinition> LoadBuildingDefinitions(DataLoader loader)
        {
            var definitions = new List<BuildingTypeDefinition>();
            var json = loader.LoadResourceText(BuildingResourcePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonUtility.FromJson<BuildingTypeDefinitionList>(json);
                if (list?.Items != null)
                {
                    definitions.AddRange(list.Items);
                }

                return definitions;
            }

            var csv = loader.LoadResourceText(BuildingCsvResourcePath);
            if (string.IsNullOrWhiteSpace(csv))
            {
                return definitions;
            }

            var rows = SimpleCsv.Parse(csv);
            if (rows.Count == 0)
            {
                return definitions;
            }

            var header = rows[0];
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var definition = new BuildingTypeDefinition
                {
                    Id = GetValue(header, row, "Id"),
                    DisplayName = GetValue(header, row, "DisplayName"),
                    ProductionCost = GetIntValue(header, row, "ProductionCost", 20),
                    FoodBonus = GetIntValue(header, row, "FoodBonus", 0),
                    ProductionBonus = GetIntValue(header, row, "ProductionBonus", 0),
                    ScienceBonus = GetIntValue(header, row, "ScienceBonus", 0),
                    DefenseBonus = GetIntValue(header, row, "DefenseBonus", 0),
                    RequiresTech = GetValue(header, row, "RequiresTech")
                };

                definitions.Add(definition);
            }

            return definitions;
        }

        private static string GetValue(string[] header, string[] row, string column)
        {
            var index = Array.FindIndex(header, cell => cell.Trim() == column);
            if (index < 0 || index >= row.Length)
            {
                return string.Empty;
            }

            return row[index].Trim();
        }

        private static int GetIntValue(string[] header, string[] row, string column, int fallback)
        {
            var raw = GetValue(header, row, column);
            return int.TryParse(raw, out var value) ? value : fallback;
        }

        private static Color ParseColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return Color.white;
            }

            var value = hex.Trim().TrimStart('#');
            if (value.Length != 6 && value.Length != 8)
            {
                return Color.white;
            }

            if (!uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var packed))
            {
                return Color.white;
            }

            byte r;
            byte g;
            byte b;
            byte a;
            if (value.Length == 6)
            {
                r = (byte)((packed >> 16) & 0xFF);
                g = (byte)((packed >> 8) & 0xFF);
                b = (byte)(packed & 0xFF);
                a = 255;
            }
            else
            {
                r = (byte)((packed >> 24) & 0xFF);
                g = (byte)((packed >> 16) & 0xFF);
                b = (byte)((packed >> 8) & 0xFF);
                a = (byte)(packed & 0xFF);
            }

            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
    }
}
