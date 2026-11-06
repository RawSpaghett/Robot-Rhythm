using System;
using UnityEngine;

namespace RobotRhythm.Feature5
{
    [CreateAssetMenu(menuName = "Robot Rhythm/Feature 5/Clean Animation Catalog")]
    public class CleanAnimationCatalog : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public Texture2D source;
            public string sourceName;
            public Texture2D cleanSource;
            public Texture2D regions;
            public Texture2D accents;
            public Texture2D face;
            public Texture2D space;
        }
        public Entry[] entries;
        private Texture lastTexture;
        private Entry lastEntry;

        public Entry Find(Texture texture)
        {
            if (texture == null || entries == null) return null;
            if (texture == lastTexture) return lastEntry;
            lastTexture = texture;
            lastEntry = null;
            string name = texture.name;
            foreach (var entry in entries)
                if (entry.source == texture || entry.cleanSource == texture || entry.sourceName == name)
                {
                    lastEntry = entry;
                    break;
                }
            return lastEntry;
        }
    }
}

