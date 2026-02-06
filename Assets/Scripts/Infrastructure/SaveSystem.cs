using System.IO;
using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Infrastructure
{
    public static class SaveSystem
    {
        public static void SaveGame(GameState state, string slotName)
        {
            if (state == null)
            {
                return;
            }

            string path = GetSlotPath(slotName);
            var dto = GameStateDto.FromState(state);
            string json = JsonUtility.ToJson(dto, true);
            File.WriteAllText(path, json);
        }

        public static GameState LoadGame(string slotName)
        {
            string path = GetSlotPath(slotName);
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            var dto = JsonUtility.FromJson<GameStateDto>(json);
            return dto?.ToState();
        }

        private static string GetSlotPath(string slotName)
        {
            string safeSlot = string.IsNullOrEmpty(slotName) ? "autosave" : slotName;
            string fileName = $"{safeSlot}.json";
            string dir = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Path.Combine(dir, fileName);
        }
    }
}
