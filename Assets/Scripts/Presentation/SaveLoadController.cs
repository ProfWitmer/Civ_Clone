using CivClone.Infrastructure;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class SaveLoadController : MonoBehaviour
    {
        [SerializeField] private string slotName = "autosave";
        [SerializeField] private KeyCode saveKey = KeyCode.F5;
        [SerializeField] private KeyCode loadKey = KeyCode.F9;
        [SerializeField] private bool loadOnStart = false;

        private GameBootstrap bootstrap;

        public void Bind(GameBootstrap gameBootstrap)
        {
            bootstrap = gameBootstrap;
        }

        private void Start()
        {
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<GameBootstrap>();
            }

            if (loadOnStart)
            {
                Load();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(saveKey))
            {
                Save();
            }

            if (Input.GetKeyDown(loadKey))
            {
                Load();
            }
        }

        public void Save()
        {
            if (bootstrap == null || bootstrap.State == null)
            {
                return;
            }

            SaveSystem.SaveGame(bootstrap.State, slotName);
        }

        public void Load()
        {
            if (bootstrap == null)
            {
                return;
            }

            var state = SaveSystem.LoadGame(slotName);
            if (state == null)
            {
                return;
            }

            bootstrap.ApplyState(state);
        }
    }
}
