using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RobotRhythm.Feature5
{
    public class SkinTextureImporter : AssetPostprocessor
    {
        [Serializable] private class Entry { public string source, folder; public int maxTextureSize; }
        [Serializable] private class Manifest { public Entry[] entries; }
        private static Manifest manifest;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Feature5/CleanAnimations/") || !assetPath.EndsWith(".png")) return;
            if (manifest == null)
                manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/Feature5/CleanAnimations/manifest.json"));
            int size = 256;
            foreach (var entry in manifest.entries)
            {
                if (Path.GetFileName(entry.folder) != Path.GetFileName(Path.GetDirectoryName(assetPath))) continue;
                // A sheet contains several frames, so it needs more resolution than a single frame.
                if (entry.source.Contains("Sheet") || entry.source.Contains("FULL")) size = 2048;
                if (entry.maxTextureSize > 0) size = entry.maxTextureSize;
                break;
            }
            var texture = (TextureImporter)assetImporter;
            texture.textureType = TextureImporterType.Default;
            texture.sRGBTexture = assetPath.EndsWith("Source.png") || assetPath.EndsWith("Space.png");
            texture.alphaIsTransparency = false;
            texture.mipmapEnabled = false;
            texture.isReadable = false;
            texture.maxTextureSize = size;
            texture.textureCompression = TextureImporterCompression.CompressedHQ;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.npotScale = TextureImporterNPOTScale.ToNearest;
            var desktop = texture.GetPlatformTextureSettings("Standalone");
            desktop.overridden = true;
            desktop.maxTextureSize = size;
            desktop.format = TextureImporterFormat.BC7;
            texture.SetPlatformTextureSettings(desktop);
            var android = texture.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = size;
            android.format = TextureImporterFormat.ASTC_4x4;
            texture.SetPlatformTextureSettings(android);
        }
    }
}

