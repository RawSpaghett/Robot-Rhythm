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

    private int previousCount;
    private float pulse;

    public void SetCombo(int count, float multiplier)
    {
        count = Mathf.Max(0, count);
        badge.SetActive(count > 0);
        comboText.text = count.ToString();
        multiplierText.text = "x" + Mathf.Max(1f, multiplier).ToString("0.#");
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
