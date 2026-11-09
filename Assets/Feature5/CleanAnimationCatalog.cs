using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobotRhythm.Feature5
{
    [CreateAssetMenu(menuName = "Robot Rhythm/Feature 5/Clean Animation Catalog")]
    public class CleanAnimationCatalog : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string sourceName;
            public string resourceFolder;
        }
        public Entry[] entries;
        private Dictionary<string, Entry> lookup;

        public Entry Find(Texture texture)
        {
            if (texture == null || entries == null) return null;
            if (lookup == null)
            {
                lookup = new Dictionary<string, Entry>();
                foreach (var entry in entries) lookup[entry.sourceName] = entry;
            }
            return lookup.TryGetValue(texture.name, out var found) ? found : null;
        }

        void OnEnable() { lookup = null; }
    }
}
