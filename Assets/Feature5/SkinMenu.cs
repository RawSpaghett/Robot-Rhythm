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
        public RectTransform choices;
        public Button choiceTemplate;
        [Header("Skin title colors")]
        public Color[] titleColors;
        private Button[] choiceButtons;
        private Material previewMaterial, incomingMaterial;
        private Image incoming;
        private Coroutine transition;
        private int index;
        private float frame;
        private bool previewWasVisible;
        private bool[] previewLocked;
        private Button lockPreviewButton;
        private Text lockPreviewLabel;
        private Rect lastSafeArea;
        private Vector2 lastScreen;

        public void Open()
        {
            if (GameManager.Instance.UI.IsTransitioning) return;
            index = library.Selected;
            frame = 0;
            previewWasVisible = false;
            if (previewMaterial == null) previewMaterial = new Material(library.material);
            preview.material = previewMaterial;
            previewMaterial.SetFloat("_SheenScale", 1f / Mathf.Max(1, preview.rectTransform.rect.height));
            CreateLockPreview();
            BuildChoices();
            LayoutScreen();
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

        void BuildChoices()
        {
            if (choices == null || choiceTemplate == null || choiceButtons != null) return;
            choiceButtons = new Button[library.names.Length];
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int target = i;
                var button = Instantiate(choiceTemplate, choices);
                button.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { if (target != index) Change(target - index); });
                choiceButtons[i] = button;
            }
        }

        void LayoutScreen()
        {
            var paper = (RectTransform)preview.transform.parent.parent;
            var canvas = paper.GetComponentInParent<Canvas>().rootCanvas;
            Rect area = Screen.safeArea;
            float scale = Mathf.Min(area.height / 720f, area.width / 1280f);
            if (scale <= 0) return;
            float width = area.width / scale;
            paper.anchorMin = paper.anchorMax = paper.pivot = new Vector2(.5f, .5f);
            paper.sizeDelta = new Vector2(width, 720);
            paper.localScale = Vector3.one * (scale / canvas.scaleFactor);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)paper.parent,
                area.center, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out var center);
            paper.localPosition = new Vector3(center.x, center.y, 0);

            Place(paper.Find("SkinHeading"), width * .5f - 185, 40, 370, 75);
            float robotWidth = Mathf.Min(560, width * .41f);
            Place(preview.transform.parent, width * .29f - robotWidth * .5f, 155, robotWidth, 385);
            preview.rectTransform.sizeDelta = new Vector2(430, 370);
            Place(previous.transform, 45, 300, 100, 85);
            Place(next.transform, width * .55f - 100, 300, 100, 85);
            float detailsX = width * .60f;
            float detailsWidth = width * .35f;
            Place(paper.Find("SkinDetails"), detailsX, 155, detailsWidth, 390);
            paper.Find("SkinDetails").GetComponent<Image>().preserveAspect = false;
            Place(title.transform, detailsX + 30, 205, detailsWidth - 60, 85);
            Place(status.transform, detailsX + 40, 280, detailsWidth - 80, 115);
            Place(select.transform, detailsX + 55, 420, detailsWidth - 110, 80);
            var selectFace = (RectTransform)select.transform.Find("StyledFace");
            selectFace.anchorMin = Vector2.zero;
            selectFace.anchorMax = Vector2.one;
            selectFace.offsetMin = selectFace.offsetMax = Vector2.zero;
            var selectText = selectLabel.rectTransform;
            selectText.anchorMin = Vector2.zero;
            selectText.anchorMax = Vector2.one;
            selectText.offsetMin = new Vector2(16, 12);
            selectText.offsetMax = new Vector2(-16, -12);
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 26;
            title.resizeTextMaxSize = 36;

            Place(choices.parent, 50, 585, width - 100, 110);
            var row = choices.GetComponent<HorizontalLayoutGroup>();
            row.spacing = 14;
            row.padding = new RectOffset(5, 5, 5, 7);
            float buttonWidth = (width - 110 - row.spacing * (choiceButtons.Length - 1)) / Mathf.Max(1, choiceButtons.Length);
            foreach (var button in choiceButtons)
            {
                var element = button.GetComponent<LayoutElement>();
                if (element == null) element = button.gameObject.AddComponent<LayoutElement>();
                element.minWidth = element.preferredWidth = buttonWidth;
                element.minHeight = element.preferredHeight = 98;
                var label = button.GetComponentInChildren<Text>();
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = new Vector2(10, 14);
                label.rectTransform.offsetMax = new Vector2(-10, -14);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 17;
                label.resizeTextMaxSize = 20;
            }
            // Old touch targets were wider than their visible arrow buttons.
            foreach (var button in new[] { previous, next, select })
            {
                var touch = button.transform.Find("TouchTarget") as RectTransform;
                if (touch == null) continue;
                touch.anchorMin = Vector2.zero;
                touch.anchorMax = Vector2.one;
                touch.offsetMin = touch.offsetMax = Vector2.zero;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(choices);
            lastSafeArea = area;
            lastScreen = new Vector2(Screen.width, Screen.height);
        }

        static void Place(Transform target, float x, float y, float width, float height)
        {
            var rect = (RectTransform)target;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
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
            transition = StartCoroutine(ChangeRobot(target, direction > 0 ? 1 : -1));
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
                library.Apply(previewMaterial, sprite.texture, Available(index) ? index : 0, false);
                library.Apply(incomingMaterial, sprite.texture, available ? target : 0, false);
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
            if (incomingMaterial != null) { SkinTextureCache.Release(incomingMaterial); Destroy(incomingMaterial); incomingMaterial = null; }
        }

        void Refresh()
        {
            // Update the sprite and its palette together before the next canvas render.
            if (previewMaterial != null && library.previewFrames.Length > 0)
            {
                preview.sprite = library.previewFrames[(int)frame % library.previewFrames.Length];
                library.Apply(previewMaterial, preview.sprite.texture, Available(index) ? index : 0, audioControls.ReducedMotion);
            }
            bool available = Available(index);
            bool selected = index == library.Selected;
            UpdateLabels(index, available);
            preview.color = available ? Color.white : Color.black;
            lockIcon.SetActive(!available);
            selectLabel.text = !available ? "LOCKED" : selected ? "SELECTED" : "USE SKIN";
            selectLabel.color = !available || selected ? new Color32(23, 49, 60, 255) : new Color32(255, 240, 208, 255);
            select.interactable = available && !selected;
            previous.gameObject.SetActive(index > 0);
            next.gameObject.SetActive(index < library.palettes.Length - 1);
            if (lockPreviewLabel != null) lockPreviewLabel.text = available ? "SHOW LOCKED" : "SHOW UNLOCKED";
            if (choiceButtons != null) for (int i = 0; i < choiceButtons.Length; i++)
            {
                choiceButtons[i].GetComponentInChildren<Text>().text = Available(i) ? library.names[i] : i == choiceButtons.Length - 1 ? "???" : "LOCKED";
                choiceButtons[i].GetComponentInChildren<Text>().color = TitleColor(i, Available(i));
                choiceButtons[i].image.color = i == index ? new Color32(255, 211, 107, 255) : Color.white;
            }
        }

        void UpdateLabels(int skin, bool available)
        {
            bool mystery = !available && skin == library.palettes.Length - 1;
            title.text = available ? library.names[skin] : mystery ? "???" : "LOCKED";
            title.color = TitleColor(skin, available);
            status.text = available ? "Choose your robot's colors" : mystery ? "????????" : skin == 0 ? "Available from the start" : library.objectives[skin];
        }

        Color TitleColor(int skin, bool available)
        {
            if (!available || titleColors == null || skin >= titleColors.Length)
                return new Color32(23, 49, 60, 255);
            return titleColors[skin];
        }

        void Update()
        {
            if (panel.activeSelf) previewWasVisible = true;
            if (panel.activeSelf && choiceButtons != null &&
                (lastSafeArea != Screen.safeArea || lastScreen != new Vector2(Screen.width, Screen.height))) LayoutScreen();
            if (previewWasVisible && !panel.activeSelf && previewMaterial != null)
            {
                SkinTextureCache.Release(previewMaterial);
                Destroy(previewMaterial);
                previewMaterial = null;
                preview.material = null;
            }
            if (!panel.activeSelf || previewMaterial == null || transition != null || library.previewFrames.Length == 0) return;
            if (!audioControls.ReducedMotion) frame += Time.unscaledDeltaTime * 12;
            preview.sprite = library.previewFrames[(int)frame % library.previewFrames.Length];
            library.Apply(previewMaterial, preview.sprite.texture, Available(index) ? index : 0, audioControls.ReducedMotion);
        }

        void OnDisable()
        {
            if (previewMaterial != null) SkinTextureCache.Release(previewMaterial);
            ClearIncoming();
        }

        void OnDestroy()
        {
            if (previewMaterial != null) { SkinTextureCache.Release(previewMaterial); Destroy(previewMaterial); }
            if (incomingMaterial != null) { SkinTextureCache.Release(incomingMaterial); Destroy(incomingMaterial); incomingMaterial = null; }
        }
    }
}

