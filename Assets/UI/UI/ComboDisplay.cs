using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

// Displays values supplied by gameplay. It does not calculate streaks or award points.
public class ComboDisplay : MonoBehaviour
{
    public GameObject badge;
    public Text comboText;
    public Text multiplierText;
    public UiController preferences;
    public Color lowColor = new Color32(255, 211, 70, 255);
    public Color middleColor = new Color32(255, 112, 55, 255);
    public Color highColor = new Color32(190, 85, 255, 255);

    private int previousCount;
    private float pulse;

    public void SetCombo(int count, float multiplier)
    {
        count = Mathf.Max(0, count);
        badge.SetActive(count > 0);
        comboText.text = count.ToString();
        multiplierText.text = "x" + Mathf.Max(1f, multiplier).ToString("0.#");
        float progress = Mathf.InverseLerp(1f, ComboTracker.maxMultiplier, multiplier);
        multiplierText.color = progress < .5f
            ? Color.Lerp(lowColor, middleColor, progress * 2f)
            : Color.Lerp(middleColor, highColor, (progress - .5f) * 2f);
        if (count > previousCount && !preferences.ReducedMotion) pulse = 1;
        previousCount = count;
    }

    public void ClearCombo()
    {
        previousCount = 0;
        pulse = 0;
        badge.transform.localScale = Vector3.one;
        badge.SetActive(false);
    }

    private void Update()
    {
        pulse = Mathf.MoveTowards(pulse, 0, Time.unscaledDeltaTime / .2f);
        badge.transform.localScale = Vector3.one * (1 + .06f * pulse);
    }
}
