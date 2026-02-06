using System.IO;
using UnityEngine;

namespace CivClone.Infrastructure.Data
{
    // Loads external data from StreamingAssets at runtime.
    public sealed class DataLoader
    {
        public string LoadText(string relativePath)
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
        }

        public string LoadResourceText(string resourcePath)
        {
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            return textAsset != null ? textAsset.text : string.Empty;
        }
    }
}
