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
            if (child != optionTemplate.transform) Destroy(child.gameObject);
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
            text.text = label;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (selected.Contains(modifier)) selected.Remove(modifier);
                else selected.Add(modifier);
                text.text = (selected.Contains(modifier) ? "✓  " : "") + label;
                button.image.color = selected.Contains(modifier) ? new Color32(255, 211, 107, 255) : Color.white;
                summary.text = selected.Count == 0 ? "Standard delivery" : selected.Count + " selected • Standard records stay separate";
                preferences.PlayClick();
            });
        }
        summary.text = available.Count == 0 ? "No modifiers available yet. Play a standard delivery." : "Choose your modifiers, or play a standard delivery.";
        ui.SwitchMenuContent(levelPanel, panel);
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
