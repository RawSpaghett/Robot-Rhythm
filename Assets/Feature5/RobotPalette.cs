using UnityEngine;

namespace RobotRhythm.Feature5
{
    [CreateAssetMenu(menuName = "Robot Rhythm/Feature 5/Robot Palette")]
    public class RobotPalette : ScriptableObject
    {
        public Color body;
        public Color accent;
        public Color face;
        public Color eyes;
        [Range(0,1)] public float metalBrightness=1;
        public Color wheelColor;
        public bool overrideWheelColor;
        [Range(0,1)] public float iridescence;
        public bool original;
        public bool spaceColors;
    }
}
