using CivClone.Infrastructure;
using CivClone.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CivClone.Infrastructure.Editor
{
    public static class MainSceneCreator
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string CatalogPath = "Assets/Data/GameDataCatalog.asset";
        private const string PanelSettingsPath = "Assets/UI/PanelSettings.asset";
        private const string HudLayoutPath = "Assets/UI/Layouts/Hud.uxml";

        [MenuItem("Tools/Civ Clone/Create Main Scene")]
        public static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var gameRoot = new GameObject("Game");
            var mapPresenter = gameRoot.AddComponent<MapPresenter>();
            var unitPresenter = gameRoot.AddComponent<UnitPresenter>();
            var inputController = gameRoot.AddComponent<MapInputController>();
            var bootstrap = gameRoot.AddComponent<GameBootstrap>();

            var hudRoot = new GameObject("HUD");

            var cameraRoot = new GameObject("Main Camera");
            var camera = cameraRoot.AddComponent<Camera>();
            cameraRoot.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.09f, 0.1f);
            cameraRoot.transform.position = new Vector3(10f, 6f, -10f);
            var hudController = GetOrAddComponent<HudController>(hudRoot);
            var document = GetOrAddComponent<UIDocument>(hudRoot);

            var hudLayout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HudLayoutPath);
            if (hudLayout == null)
            {
                Debug.LogError("HUD layout not found at " + HudLayoutPath + ".");
                return;
            }

            document.visualTreeAsset = hudLayout;
            document.panelSettings = GetOrCreatePanelSettings();

            var catalog = GetOrCreateCatalog();
            WireReferences(bootstrap, mapPresenter, unitPresenter, inputController, hudController, catalog);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = gameRoot;
        }

        private static PanelSettings GetOrCreatePanelSettings()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel != null)
            {
                return panel;
            }

            panel = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            AssetDatabase.SaveAssets();
            return panel;
        }

        private static GameDataCatalog GetOrCreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GameDataCatalog>(CatalogPath);
            if (catalog != null)
            {
                return catalog;
            }

            var plains = ScriptableObject.CreateInstance<TerrainType>();
            plains.Id = "plains";
            plains.DisplayName = "Plains";
            plains.MovementCost = 1;
            plains.Color = new Color(0.23f, 0.56f, 0.27f);

            var hills = ScriptableObject.CreateInstance<TerrainType>();
            hills.Id = "hills";
            hills.DisplayName = "Hills";
            hills.MovementCost = 2;
            hills.Color = new Color(0.4f, 0.5f, 0.25f);

            var scout = ScriptableObject.CreateInstance<UnitType>();
            scout.Id = "scout";
            scout.DisplayName = "Scout";
            scout.MovementPoints = 2;
            scout.Attack = 1;
            scout.Defense = 1;

            AssetDatabase.CreateAsset(plains, "Assets/Data/Plains.asset");
            AssetDatabase.CreateAsset(hills, "Assets/Data/Hills.asset");
            AssetDatabase.CreateAsset(scout, "Assets/Data/Scout.asset");

            catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
            catalog.TerrainTypes = new[] { plains, hills };
            catalog.UnitTypes = new[] { scout };

            AssetDatabase.CreateAsset(catalog, CatalogPath);
            AssetDatabase.SaveAssets();

            return catalog;
        }

        private static void WireReferences(GameBootstrap bootstrap, MapPresenter mapPresenter, UnitPresenter unitPresenter, MapInputController inputController, HudController hudController, GameDataCatalog catalog)
        {
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("mapPresenter").objectReferenceValue = mapPresenter;
            serialized.FindProperty("unitPresenter").objectReferenceValue = unitPresenter;
            serialized.FindProperty("inputController").objectReferenceValue = inputController;
            serialized.FindProperty("hudController").objectReferenceValue = hudController;
            serialized.FindProperty("dataCatalog").objectReferenceValue = catalog;
            serialized.ApplyModifiedProperties();
        }
        private static T GetOrAddComponent<T>(GameObject root) where T : Component
        {
            if (root.TryGetComponent(out T existing))
            {
                return existing;
            }

            return root.AddComponent<T>();
        }
    }
}
