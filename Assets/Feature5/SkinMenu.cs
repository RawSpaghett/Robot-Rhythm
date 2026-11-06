using System.Collections;
using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.Feature5
{
    public class SkinMenu : MonoBehaviour
    {
        public SkinLibrary library;
        public GameObject panel, mainMenuContent, lockIcon;
        public Image preview;
        public Text title, status, selectLabel;
        public Button previous, next, select;
        public UiController audioControls;
        public float transitionSeconds = .75f;
        private Material previewMaterial, incomingMaterial;
        private Image incoming;
        private Coroutine transition;
        private int index;
        private float frame;
        private bool[] previewLocked;
        private Button lockPreviewButton;
        private Text lockPreviewLabel;

        public void Open()
        {
            if (GameManager.Instance.UI.IsTransitioning) return;
            index = library.Selected;
            frame = 0;
            if (previewMaterial == null) previewMaterial = new Material(library.material);
            preview.material = previewMaterial;
            previewMaterial.SetFloat("_SheenScale", 1f / Mathf.Max(1, preview.rectTransform.rect.height));
            CreateLockPreview();
            Refresh();
            GameManager.Instance.UI.SwitchMenuContent(mainMenuContent, panel);
            audioControls.PlayClick();
        }

        public void Close()
        {
            if (GameManager.Instance.UI.IsTransitioning) return;
            if (transition != null) StopCoroutine(transition);
            transition = null;
            ClearIncoming();
            preview.rectTransform.anchoredPosition = Vector2.zero;
            GameManager.Instance.UI.SwitchMenuContent(panel, mainMenuContent);
            audioControls.PlayBackClick();
        }

        public void Previous() { Change(-1); }
        public void Next() { Change(1); }
        public void Equip() { if (Available(index) && transition == null && !GameManager.Instance.UI.IsTransitioning) { library.Select(index); Refresh(); audioControls.PlayClick(); } }

        bool Available(int skin)
        {
            return library.previewAllUnlocked ? previewLocked == null || !previewLocked[skin] : library.Available(skin);
        }

        void CreateLockPreview()
        {
            // Preview switches never change saved unlocks.
            if (!library.previewAllUnlocked || lockPreviewButton != null) return;
            previewLocked = new bool[library.palettes.Length];
            lockPreviewButton = Instantiate(previous, select.transform.parent);
            lockPreviewButton.name = "PreviewLockToggle";
            lockPreviewButton.onClick = new Button.ButtonClickedEvent();
            lockPreviewButton.onClick.AddListener(ToggleLockPreview);
            var rect = (RectTransform)lockPreviewButton.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(260, -90);
            rect.sizeDelta = new Vector2(190, 55);
            lockPreviewLabel = lockPreviewButton.GetComponentInChildren<Text>(true);
            lockPreviewLabel.fontSize = 17;
            lockPreviewLabel.resizeTextForBestFit = false;
            lockPreviewButton.gameObject.SetActive(true);
        }

        public void ToggleLockPreview()
        {
            if (!library.previewAllUnlocked || transition != null || GameManager.Instance.UI.IsTransitioning) return;
            previewLocked[index] = !previewLocked[index];
            Refresh();
        }

        void Change(int direction)
        {
            int target = index + direction;
            if (transition != null || GameManager.Instance.UI.IsTransitioning || target < 0 || target >= library.palettes.Length) return;
            if (direction > 0) audioControls.PlayClick(); else audioControls.PlayBackClick();
            if (audioControls.ReducedMotion)
            {
                index = target;
                Refresh();
                return;
            }
            transition = StartCoroutine(ChangeRobot(target, direction));
        }

        IEnumerator ChangeRobot(int target, int direction)
        {
            var frames = direction > 0 ? library.accelerateFrames : library.brakeFrames;
            if (frames.Length == 0) frames = library.previewFrames;
            incoming = Instantiate(preview, preview.transform.parent);
            incoming.name = "IncomingRobot";
            incomingMaterial = new Material(library.material);
            incomingMaterial.SetFloat("_SheenScale", 1f / Mathf.Max(1, preview.rectTransform.rect.height));
            incoming.material = incomingMaterial;
            bool available = Available(target);
            incoming.color = available ? Color.white : Color.black;
            incoming.transform.GetChild(0).gameObject.SetActive(!available);
            select.interactable = false;
            selectLabel.text = "";
            float elapsed = 0;
            while (elapsed < transitionSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / transitionSeconds);
                if (t >= .5f)
                {
                    UpdateLabels(target, available);
                }
                float leave = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / .65f));
                float arrive = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - .3f) / .7f));
                preview.rectTransform.anchoredPosition = new Vector2(direction * 620 * leave, 0);
                incoming.rectTransform.anchoredPosition = new Vector2(-direction * 620 * (1 - arrive), 0);
                var sprite = frames[Mathf.Min((int)(t * frames.Length), frames.Length - 1)];
                preview.sprite = incoming.sprite = sprite;
                library.Apply(previewMaterial, sprite.texture, index, false);
                library.Apply(incomingMaterial, sprite.texture, target, false);
                yield return null;
            }
            index = target;
            preview.rectTransform.anchoredPosition = Vector2.zero;
            frame = 0;
            ClearIncoming();
            transition = null;
            Refresh();
        }

        void ClearIncoming()
        {
            if (incoming != null) Destroy(incoming.gameObject);
            if (incomingMaterial != null) Destroy(incomingMaterial);
        }

        void Refresh()
        {
            bool available = Available(index);
            bool selected = index == library.Selected;
            UpdateLabels(index, available);
            preview.color = available ? Color.white : Color.black;
            lockIcon.SetActive(!available);
            selectLabel.text = !available ? "LOCKED" : selected ? "SELECTED" : "USE SKIN";
            select.interactable = available && !selected;
            previous.gameObject.SetActive(index > 0);
            next.gameObject.SetActive(index < library.palettes.Length - 1);
            if (lockPreviewLabel != null) lockPreviewLabel.text = available ? "SHOW LOCKED" : "SHOW UNLOCKED";
        }

        void UpdateLabels(int skin, bool available)
        {
            bool mystery = !available && skin == library.palettes.Length - 1;
            title.text = available ? library.names[skin] : mystery ? "???" : "LOCKED";
            status.text = available ? "Choose your robot's colors" : mystery ? "????????" : skin == 0 ? "Available from the start" : library.objectives[skin];
        }

        void Update()
        {
            if (!panel.activeSelf || previewMaterial == null || transition != null || library.previewFrames.Length == 0) return;
            if (!audioControls.ReducedMotion) frame += Time.unscaledDeltaTime * 12;
            preview.sprite = library.previewFrames[(int)frame % library.previewFrames.Length];
            library.Apply(previewMaterial, preview.sprite.texture, index, audioControls.ReducedMotion);
        }

        void OnDestroy()
        {
            if (previewMaterial != null) Destroy(previewMaterial);
            if (incomingMaterial != null) Destroy(incomingMaterial);
        }
    }
}

