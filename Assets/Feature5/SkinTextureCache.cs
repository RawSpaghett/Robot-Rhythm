using System.Collections.Generic;
using UnityEngine;

namespace RobotRhythm.Feature5
{
    // Only textures used by an active player or preview are pinned. Recently used
    // frames share a bounded cache so animations don't reload every frame.
    public static class SkinTextureCache
    {
        class Item
        {
            public Texture2D texture;
            public long bytes;
            public int used;
        }
        const long Budget = 64L * 1024 * 1024;
        static readonly Dictionary<string, Item> textures = new Dictionary<string, Item>();
        static readonly Dictionary<Material, HashSet<string>> users = new Dictionary<Material, HashSet<string>>();
        static readonly Dictionary<Material, int> skins = new Dictionary<Material, int>();
        static long bytes;
        static int clock;
        public static long CachedBytes => bytes;
        public static int LoadedCount => textures.Count;

        public static void Begin(Material material, int skin)
        {
            if (skins.TryGetValue(material, out int previous) && previous != skin) Release(material);
            skins[material] = skin;
            if (!users.TryGetValue(material, out var paths)) users[material] = new HashSet<string>();
            else paths.Clear();
        }

        public static Texture2D Load(Material material, string folder, string name)
        {
            string path = folder + "/" + name;
            if (!textures.TryGetValue(path, out var item))
            {
                var texture = Resources.Load<Texture2D>(path);
                if (texture == null) { Debug.LogError("Missing skin texture: " + path); return Texture2D.blackTexture; }
                // Four bytes per pixel also bounds memory on devices that decompress textures.
                item = new Item { texture = texture, bytes = (long)texture.width * texture.height * 4 };
                textures.Add(path, item);
                bytes += item.bytes;
            }
            item.used = ++clock;
            users[material].Add(path);
            return item.texture;
        }

        public static void Trim(bool allUnused = false)
        {
            if (!allUnused && bytes <= Budget) return;
            var pinned = new HashSet<string>();
            foreach (var paths in users.Values) pinned.UnionWith(paths);
            var candidates = new List<string>();
            foreach (var pair in textures) if (!pinned.Contains(pair.Key)) candidates.Add(pair.Key);
            candidates.Sort((a, b) => textures[a].used.CompareTo(textures[b].used));
            foreach (string path in candidates)
            {
                if (!allUnused && bytes <= Budget) break;
                var item = textures[path];
                bytes -= item.bytes;
                Resources.UnloadAsset(item.texture);
                textures.Remove(path);
            }
        }

        public static void Release(Material material)
        {
            if (material == null) return;
            material.SetTexture("_CleanTex", Texture2D.whiteTexture);
            material.SetTexture("_Regions", Texture2D.blackTexture);
            material.SetTexture("_Accents", Texture2D.blackTexture);
            material.SetTexture("_FaceMask", Texture2D.blackTexture);
            users.Remove(material);
            skins.Remove(material);
            Trim(true);
        }
    }
}
