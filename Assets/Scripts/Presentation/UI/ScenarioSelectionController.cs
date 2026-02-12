using System;
using System.Collections.Generic;
using System.IO;
using CivClone.Infrastructure;
using CivClone.Infrastructure.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace CivClone.Presentation.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class ScenarioSelectionController : MonoBehaviour
    {
        private const string ScenarioPanelName = "scenario-panel";
        private const string ScenarioListName = "scenario-list";
        private const string ScenarioSelectorPath = "Data/scenario.json";
        private const string ScenarioCatalogPath = "Data/scenario_catalog.json";

        private VisualElement panel;
        private ScrollView list;

        private void Awake()
        {
            var document = GetComponent<UIDocument>();
            var root = document.rootVisualElement;
            panel = root.Q<VisualElement>(ScenarioPanelName);
            list = root.Q<ScrollView>(ScenarioListName);
            Refresh();
        }

        public void Refresh()
        {
            if (panel == null || list == null)
            {
                return;
            }

            list.Clear();
            var catalog = LoadCatalog();
            if (catalog?.Items == null || catalog.Items.Length == 0)
            {
                panel.style.display = DisplayStyle.None;
                return;
            }

            panel.style.display = DisplayStyle.Flex;
            string selectedId = LoadSelectedScenarioId();
            foreach (var entry in catalog.Items)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id))
                {
                    continue;
                }

                var row = new VisualElement();
                row.AddToClassList("hud-scenario-row");

                var label = new Label($"{entry.Name} {(entry.Id == selectedId ? "(Current)" : string.Empty)}");
                label.AddToClassList("hud-sub");
                label.AddToClassList("hud-scenario-text");
                row.Add(label);

                var button = new Button(() => SelectScenario(entry.Id)) { text = "Select" };
                button.AddToClassList("hud-button");
                button.AddToClassList("hud-scenario-button");
                row.Add(button);

                list.Add(row);
            }
        }

        private ScenarioCatalog LoadCatalog()
        {
            var loader = new DataLoader();
            string json = loader.LoadText(ScenarioCatalogPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<ScenarioCatalog>(json);
            }
            catch
            {
                return null;
            }
        }

        private string LoadSelectedScenarioId()
        {
            string json = LoadSelectorJson();
            if (string.IsNullOrWhiteSpace(json))
            {
                return string.Empty;
            }

            try
            {
                var selector = JsonUtility.FromJson<ScenarioSelector>(json);
                return selector?.ScenarioId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void SelectScenario(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            var selector = new ScenarioSelector { ScenarioId = id };
            string json = JsonUtility.ToJson(selector, true);
            string path = Path.Combine(Application.persistentDataPath, ScenarioSelectorPath);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);
                Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to write scenario selector: {ex.Message}");
            }
        }

        private string LoadSelectorJson()
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, ScenarioSelectorPath);
            if (File.Exists(persistentPath))
            {
                return File.ReadAllText(persistentPath);
            }

            var loader = new DataLoader();
            return loader.LoadText(ScenarioSelectorPath);
        }
    }
}
