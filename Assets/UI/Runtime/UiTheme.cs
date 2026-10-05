using UnityEngine;

namespace RobotRhythm.UI
{
    [CreateAssetMenu(menuName = "Robot Rhythm/UI Theme")]
    public sealed class UiTheme : ScriptableObject
    {
        [Header("Typography")]
        public Font displayFont;
        public Font bodyFont;
        public Font boldFont;
        [Header("Panel Sprites")]
        public Sprite rounded;
        public Sprite solid;
        [Header("Palette")]
        public Color ink = new Color32(23, 49, 60, 255);
        public Color teal = new Color32(77, 143, 136, 255);
        public Color cream = new Color32(244, 237, 218, 255);
        public Color lime = new Color32(239, 123, 80, 255);
        public Color orange = new Color32(239, 123, 80, 255);
        public Color yellow = new Color32(242, 190, 88, 255);
    }
}
