using UnityEngine;

namespace RobotRhythm.Feature5
{
    // The animation system keeps ownership of the sprite; only its colors change.
    [RequireComponent(typeof(SpriteRenderer))]
    public class RobotSkin : MonoBehaviour
    {
        public SkinLibrary library;
        private SpriteRenderer robot;
        private Material skinMaterial;
        private Material originalMaterial;
        private Texture lastTexture;
        private int lastSkin = -1;
        private bool lastReducedMotion;

        void OnEnable()
        {
            robot = GetComponent<SpriteRenderer>();
            originalMaterial = robot.sharedMaterial;
            skinMaterial = new Material(library.material);
            robot.sharedMaterial = skinMaterial;
            lastTexture = null;
        }

        void LateUpdate()
        {
            if (robot.sprite == null) return;
            skinMaterial.SetFloat("_SheenScale", 1f / Mathf.Max(.01f, robot.sprite.bounds.size.y));
            int skin = library.Selected;
            bool reducedMotion = PlayerPrefs.GetInt("rr.ui.v1.motion", 0) == 1;
            if (lastTexture == robot.sprite.texture && lastSkin == skin && lastReducedMotion == reducedMotion) return;
            lastTexture = robot.sprite.texture;
            lastSkin = skin;
            lastReducedMotion = reducedMotion;
            library.Apply(skinMaterial, lastTexture, skin, reducedMotion);
        }

        void OnDisable()
        {
            if (robot != null) robot.sharedMaterial = originalMaterial;
            if (skinMaterial != null) { SkinTextureCache.Release(skinMaterial); Destroy(skinMaterial); }
        }
    }
}

