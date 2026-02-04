using System.IO;

namespace CivClone.Infrastructure.Data
{
    // Loads external data from StreamingAssets at runtime.
    public sealed class DataLoader
    {
        public string LoadText(string relativePath)
        {
            var fullPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, relativePath);
            return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
        }
    }
}
