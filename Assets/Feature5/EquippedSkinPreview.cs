using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.Feature5
{
    [RequireComponent(typeof(Image))]
    public class EquippedSkinPreview : MonoBehaviour
    {
        public SkinLibrary library;
        public UiController preferences;
        private Image preview;
        private Material material;
        private float frame;

        void OnEnable()
        {
            preview = GetComponent<Image>();
            material = new Material(library.material);
            preview.material = material;
            frame = 0;
            Refresh();
        }

        void Update()
        {
            if (!preferences.ReducedMotion) frame += Time.unscaledDeltaTime * 12;
            Refresh();
        }

        void Refresh()
        {
            if (library.previewFrames.Length == 0) return;
            preview.sprite = library.previewFrames[(int)frame % library.previewFrames.Length];
            material.SetFloat("_SheenScale", 1f / Mathf.Max(1, preview.rectTransform.rect.height));
            library.Apply(material, preview.sprite.texture, library.Selected, preferences.ReducedMotion);
        }

        void OnDisable()
        {
            if (material == null) return;
            SkinTextureCache.Release(material);
            preview.material = null;
            Destroy(material);
            material = null;
        }
    }
}
