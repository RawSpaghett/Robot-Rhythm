using UnityEngine;

namespace RobotRhythm.Feature5
{
    [CreateAssetMenu(menuName = "Robot Rhythm/Robot Skins")]
    public class SkinLibrary : ScriptableObject
    {
        public CleanAnimationCatalog masks;
        public Material material;
        public RobotPalette[] palettes;
        public string[] names;
        public string[] scoredLevelIds;
        public string[] unlockLevelIds;
        public string[] objectives;
        public Sprite[] previewFrames;
        public Sprite[] accelerateFrames, brakeFrames;
        public bool previewAllUnlocked;
        private int previewSelection;
        public const string SelectionKey = "rr.skin.v1.selected";
        public const string GoldKey = "rr.skin.v1.gold";

        public bool GoldUnlocked()
        {
            if (PlayerPrefs.GetInt(GoldKey, 0) == 1) return true;
            if (scoredLevelIds == null || scoredLevelIds.Length == 0) return false;
            foreach (string id in scoredLevelIds)
                if (string.IsNullOrEmpty(id) || !LevelRecords.Load(id).perfect) return false;
            PlayerPrefs.SetInt(GoldKey, 1);
            PlayerPrefs.Save();
            return true;
        }

        public bool Available(int index)
        {
            if (index < 0 || index >= palettes.Length) return false;
            if (previewAllUnlocked) return true;
            if (index == 0) return true;
            if (index == palettes.Length - 1) return GoldUnlocked();
            return unlockLevelIds != null && index < unlockLevelIds.Length &&
                LevelRecords.Load(unlockLevelIds[index]).completed;
        }

        public int Selected
        {
            get
            {
                if (previewAllUnlocked) return previewSelection;
                int index = PlayerPrefs.GetInt(SelectionKey, 0);
                return Available(index) ? index : 0;
            }
        }

        public void Select(int index)
        {
            if (!Available(index)) return;
            if (previewAllUnlocked) { previewSelection = index; return; }
            PlayerPrefs.SetInt(SelectionKey, index);
            PlayerPrefs.Save();
        }

        public void Apply(Material target, Texture source, int index, bool reducedMotion)
        {
            index = Mathf.Clamp(index, 0, palettes.Length - 1);
            var entry = masks.Find(source);
            var palette = palettes[index];
            SkinTextureCache.Begin(target, index);
            bool recolor = entry != null && !palette.original && !palette.spaceColors;
            target.SetTexture("_CleanTex", entry != null
                ? SkinTextureCache.Load(target, entry.resourceFolder, palette.spaceColors ? "Space" : "Source") : source);
            target.SetTexture("_Regions", recolor ? SkinTextureCache.Load(target, entry.resourceFolder, "Regions") : Texture2D.blackTexture);
            target.SetTexture("_Accents", recolor ? SkinTextureCache.Load(target, entry.resourceFolder, "Accents") : Texture2D.blackTexture);
            target.SetTexture("_FaceMask", recolor ? SkinTextureCache.Load(target, entry.resourceFolder, "Face") : Texture2D.blackTexture);
            SkinTextureCache.Trim();
            target.SetColor("_BodyColor", palette.body);
            target.SetColor("_AccentColor", palette.accent);
            target.SetColor("_FaceColor", palette.face);
            target.SetColor("_EyeColor", palette.eyes);
            target.SetColor("_WheelColor", palette.wheelColor);
            target.SetFloat("_WheelOverride", palette.overrideWheelColor ? 1 : 0);
            target.SetFloat("_MetalValue", palette.metalBrightness);
            target.SetFloat("_Iridescence", palette.iridescence);
            target.SetFloat("_SheenTime", reducedMotion ? 0 : -1);
            target.SetFloat("_Strength", entry != null && !palette.original && !palette.spaceColors ? 1 : 0);
        }
    }
}



