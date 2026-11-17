using System.Collections.Generic;
using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

public class ModifierMenu : MonoBehaviour
{
    public UIManager ui;
    public UiController preferences;
    public GameObject panel, levelPanel;
    public RectTransform options;
    public Button optionTemplate;
    public Text summary;
    public Button playButton;
    public Text browseHint;
    [System.Serializable]
    public class Description
    {
        public string modifierType;
        public string title;
        public string badge;
        public Color accent = new Color32(173, 48, 33, 255);
        [TextArea] public string explanation;
    }
    public Description[] descriptions;
    private string scenePath;
    private readonly List<ModifierBase> selected = new List<ModifierBase>();
    private static ModifierBase[] selection;

    public static bool HasModifiers => selection != null && selection.Length > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSelection() { selection = null; }

    public static void UseStandard() { selection = new ModifierBase[0]; }

    public static void ApplySelection(ModifierManager manager)
    {
        if (selection != null) manager.activeModifiers = (ModifierBase[])selection.Clone();
    }

    public void Open(string path)
    {
        if (ui.IsTransitioning) return;
        scenePath = path;
        selected.Clear();
        foreach (Transform child in options)
            if (child != optionTemplate.transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        var database = ModifierDatabase.Instance;
        var available = new HashSet<ModifierBase>();
        if (database != null && database.modifierLoader != null)
            foreach (var modifier in database.modifierLoader)
                if (modifier != null) available.Add(modifier);
        if (database != null)
            foreach (var modifier in database.ModifierLibrary.Values)
                if (modifier != null) available.Add(modifier);
        foreach (var modifier in available)
        {
            var button = Instantiate(optionTemplate, options);
            button.gameObject.SetActive(true);
            var text = button.GetComponentInChildren<Text>();
            string label = modifier.GetType().Name.Replace("Modifier_", "").Replace("_", " ");
            string explanation = "Select to enable this modifier for the run.";
            string badge = "CHALLENGE";
            Color accent = new Color32(173, 48, 33, 255);
            if (descriptions != null) foreach (var description in descriptions)
                if (description.modifierType == modifier.GetType().Name)
                {
                    label = description.title;
                    badge = description.badge;
                    accent = description.accent;
                    explanation = description.explanation;
                    break;
                }
            text.text = OptionText(label, explanation, badge, accent, false);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (selected.Contains(modifier)) selected.Remove(modifier);
                else selected.Add(modifier);
                text.text = OptionText(label, explanation, badge, accent, selected.Contains(modifier));
                button.targetGraphic.color = selected.Contains(modifier) ? new Color32(255, 211, 107, 255) : new Color32(255, 240, 208, 255);
                RefreshSummary();
                preferences.PlayClick();
            });
        }
        if (playButton != null) playButton.interactable = !string.IsNullOrEmpty(scenePath);
        var scroll = options.GetComponentInParent<ScrollRect>();
        if (scroll != null) { scroll.StopMovement(); scroll.verticalNormalizedPosition = 1; }
        LayoutRebuilder.MarkLayoutForRebuild(options);
        RefreshSummary();
        ui.SwitchMenuContent(levelPanel, panel);
    }

    static string OptionText(string title, string explanation, string badge, Color accent, bool enabled)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(accent) + "><size=34><b>" + title + "</b></size>\n<size=14>" + badge + "</size></color>  <size=16>" + (enabled ? "[ ON ]" : "[ OFF ]") + "</size>\n<size=16>" + explanation + "</size>";
    }

    void RefreshSummary()
    {
        summary.text = string.IsNullOrEmpty(scenePath)
            ? "Choose a level other than Tutorial to play with modifiers."
            : selected.Count == 0 ? "No modifiers selected. This will be a standard run."
            : selected.Count + " selected. This run won't update standard records.";
    }

    public void Back()
    {
        if (ui.IsTransitioning) return;
        preferences.PlayBackClick();
        ui.SwitchMenuContent(panel, levelPanel);
    }

    public void Play()
    {
        if (ui.IsTransitioning || string.IsNullOrEmpty(scenePath)) return;
        selection = selected.ToArray();
        preferences.PlayClick();
        panel.SetActive(false);
        ui.StartLevel(scenePath);
    }
}

