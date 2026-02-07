using System;
using UnityEngine;

namespace CivClone.Infrastructure.Data.Definitions
{
    [Serializable]
    public class ResourceTypeDefinition
    {
        public string Id;
        public string DisplayName;
        public string Category;
        public Color Color;
    }
}
